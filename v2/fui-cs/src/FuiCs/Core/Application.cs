using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A runnable application: <see cref="Run"/> builds and mounts its page, <see cref="IDisposable.Dispose"/> unmounts it.</summary>
public interface IApplication : IDisposable
{
    /// <summary>Builds and mounts the application's page.</summary>
    void Run();

    /// <summary>Commits pending changes to the screen now.</summary>
    void FlushRenders();
}

/// <summary>An application whose page is a <typeparamref name="TPage"/>, such as a controller that owns the root node.</summary>
public sealed class Application<TPage> : IApplication where TPage : class
{
    private readonly Func<TPage> buildPage;
    private readonly Func<TPage, Node> getRoot;
    private readonly Action<TPage>? mountPage;
    private readonly Action<TPage>? disposePage;

    internal Application(Func<TPage> buildPage, Func<TPage, Node> getRoot, Action<TPage>? mountPage, Action<TPage>? disposePage)
    {
        this.buildPage = buildPage;
        this.getRoot = getRoot;
        this.mountPage = mountPage;
        this.disposePage = disposePage;
    }

    /// <summary>The page built by the last run, or null before it.</summary>
    public TPage? ActivePage { get; private set; }

    /// <summary>Replaces any running page with a newly built one.</summary>
    public void Run()
    {
        Dispose();
        var page = buildPage();
        ActivePage = page;
        if (mountPage is not null)
        {
            mountPage(page);
            return;
        }
        Application.Mount(getRoot(page));
    }

    /// <summary>Unmounts the page and disposes it.</summary>
    public void Dispose()
    {
        var page = ActivePage;
        if (page is null)
        {
            return;
        }
        ActivePage = null;
        if (disposePage is not null)
        {
            disposePage(page);
            return;
        }
        Application.Unmount();
    }

    /// <inheritdoc/>
    public void FlushRenders() => Application.FlushRenders();
}

/// <summary>A page that mounts and unmounts itself; <see cref="Application.CreateManaged{TPage}(Func{TPage})"/> runs it.</summary>
public abstract class ManagedApplicationController : IDisposable
{
    /// <summary>The node to mount as the page's root.</summary>
    public abstract Node GetRoot();

    /// <summary>Mounts the root.</summary>
    public virtual void Mount() => Application.Mount(GetRoot());

    /// <summary>Unmounts the root.</summary>
    public virtual void Dispose() => Application.Unmount();
}

/// <summary>Collects an application's settings for <see cref="Application.Register"/>.</summary>
public sealed class ApplicationRegistration
{
    private Func<Node> buildPage = Application.CreateEmptyPage;
    private Action? applyTheme;

    /// <summary>Builds the page with <paramref name="build"/>.</summary>
    public ApplicationRegistration Page(Func<Node> build)
    {
        buildPage = build;
        return this;
    }

    /// <summary>Follows the host's dark mode and accent color.</summary>
    public ApplicationRegistration UseSystemTheme()
    {
        applyTheme = static () => Theme.UseSystem();
        return this;
    }

    /// <summary>Uses <paramref name="theme"/> instead of the system's.</summary>
    public ApplicationRegistration UseTheme(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        applyTheme = () => Theme.UseCustom(theme);
        return this;
    }

    /// <summary>Generates the theme from the current light/dark mode and <paramref name="accent"/>.</summary>
    public ApplicationRegistration AccentColor(Color accent)
    {
        applyTheme = () => Theme.SetAccentColor(accent);
        return this;
    }

    internal Application<Node> Register()
    {
        var app = Application.Create(buildPage);
        applyTheme?.Invoke();
        return app;
    }
}

/// <summary>Mounts retained roots inside the application shell and flushes their commits to the host.</summary>
public static class Application
{
    private static ApplicationShell? mountedShell;

    /// <summary>The application the host runs and disposes through <c>__runApp</c> and <c>__disposeApp</c>.</summary>
    public static IApplication? Current { get; private set; }

    /// <summary>The root node passed to the last <see cref="Mount"/>, until it is unmounted.</summary>
    public static Node? MountedRoot { get; private set; }

    /// <summary>The display's device pixel ratio, read from the host each time.</summary>
    public static float DevicePixelRatio => Ui.GetDevicePixelRatio();

    /// <summary>The viewport's width, updated as the window resizes.</summary>
    public static Signal<float> ViewportWidth { get; } = new(0.0f);

    /// <summary>The viewport's height, updated as the window resizes.</summary>
    public static Signal<float> ViewportHeight { get; } = new(0.0f);

    /// <summary>The host's timestamp for the frame being drawn, in milliseconds.</summary>
    public static Signal<double> FrameTime { get; } = new(0.0);

    /// <summary>Creates the application that builds its page with <paramref name="buildPage"/> and makes it <see cref="Current"/>.</summary>
    public static Application<Node> Create(Func<Node> buildPage)
    {
        var app = new Application<Node>(buildPage, static page => page, null, null);
        Current = app;
        return app;
    }

    /// <summary>
    /// Creates the application whose page is a controller that mounts and disposes itself, and makes it current.
    /// </summary>
    public static Application<TPage> CreateManaged<TPage>(Func<TPage> buildPage) where TPage : ManagedApplicationController =>
        CreateManaged(buildPage, static page => page.GetRoot(), static page => page.Mount(), static page => page.Dispose());

    /// <summary>
    /// Creates the application whose page is built by <paramref name="buildPage"/>, mounted from <paramref
    /// name="getRoot"/>, and makes it current.
    /// </summary>
    public static Application<TPage> CreateManaged<TPage>(
        Func<TPage> buildPage,
        Func<TPage, Node> getRoot,
        Action<TPage>? mountPage = null,
        Action<TPage>? disposePage = null)
        where TPage : class
    {
        var app = new Application<TPage>(buildPage, getRoot, mountPage, disposePage);
        Current = app;
        return app;
    }

    /// <summary>Creates the application from a registration callback, and makes it current.</summary>
    public static Application<Node> Register(Action<ApplicationRegistration> configure)
    {
        var registration = new ApplicationRegistration();
        configure(registration);
        return registration.Register();
    }

    /// <summary>
    /// Unmounts the current root, resets the native tree and mounts <paramref name="root"/> inside a new shell. The
    /// previous root's requests in flight are cancelled, and its focus ring, selection chrome and tooltip cleared.
    /// </summary>
    public static Node Mount(Node root)
    {
        if (mountedShell is { } previous)
        {
            previous.Dispose();
            mountedShell = null;
            MountedRoot = null;
        }
        DisposeAppScopedWork();
        FrameScheduler.ResetCommitState();
        Ui.Reset();
        Ui.ResizeWindow(Ui.GetViewportWidth(), Ui.GetViewportHeight());
        var shell = new ApplicationShell(root);
        shell.Build();
        Ui.SetRoot(shell.Handle);
        mountedShell = shell;
        MountedRoot = root;
        unmountHandlers = nextPageUnmountHandlers;
        nextPageUnmountHandlers = null;
        FrameScheduler.FireLoadedCallbacks();
        FrameScheduler.MarkNeedsCommit();
        return root;
    }

    /// <summary>Unmounts the page, disposing its tree.</summary>
    public static void Unmount()
    {
        var previous = mountedShell;
        if (previous is null)
        {
            return;
        }
        previous.Dispose();
        mountedShell = null;
        MountedRoot = null;
        DisposeAppScopedWork();
        FrameScheduler.ResetCommitState();
    }

    /// <summary>Commits pending retained changes. The host calls this after input and on each frame.</summary>
    public static void FlushRenders()
    {
        if (mountedShell is not { IsBuilt: true })
        {
            FrameScheduler.ResetCommitState();
            return;
        }
        FrameScheduler.FlushCommit();
        if (AfterCommit?.Invoke() == true)
        {
            FrameScheduler.MarkNeedsCommit();
            FrameScheduler.FlushCommit();
        }
    }

    /// <summary>
    /// Runs after each flush to update chrome that follows laid-out bounds, such as the focus ring; returning true
    /// commits again so the change shows in the same frame.
    /// </summary>
    internal static Func<bool>? AfterCommit { get; set; }

    /// <summary>Sets the browser tab's title.</summary>
    public static void SetCaption(string caption) => Runtime.Abi.HostSetApplicationCaption(caption);

    /// <summary>Turns the framework's page zoom (pinch and ctrl-wheel) on or off.</summary>
    public static void SetPageZoomEnabled(bool enabled) => Runtime.Abi.HostSetPageZoomEnabled(enabled);

    internal static Node CreateEmptyPage() => new FlexBox { Width = Length.Percent(100.0f), Height = Length.Percent(100.0f) };

    internal static void RunOrCreate(Func<IApplication> createApplication)
    {
        var app = Current ?? createApplication();
        Current = app;
        app.Run();
    }

    /// <summary>Saves the persisted state of the mounted page, as the host does before leaving a history entry.</summary>
    public static void CapturePersistedUiState() => MountedRoot?.CapturePersistedStateTree();

    /// <summary>Restores the mounted page's persisted state for the current history entry.</summary>
    public static void RestorePersistedUiState() => MountedRoot?.RestorePersistedStateTree();

    internal static void DisposeCurrent() => Current?.Dispose();

    internal static void FlushCurrent()
    {
        if (Current is { } app)
        {
            app.FlushRenders();
            return;
        }
        FlushRenders();
    }

    /// <summary>The application's built-in context menu, created in the shell on first use.</summary>
    internal static ContextMenu? ShellContextMenu => mountedShell?.ContextMenu;

    /// <summary>The portal the focus ring draws in, created in the shell on first use.</summary>
    internal static Portal? ShellFocusHost => mountedShell?.FocusHost;

    /// <summary>The full-viewport portal the selection handles draw in, created on first use.</summary>
    internal static Portal? ShellSelectionHandleHost => mountedShell?.SelectionHandleHost;

    /// <summary>The full-viewport portal the text selection toolbar draws in, created on first use.</summary>
    internal static Portal? ShellTextToolbarHost => mountedShell?.TextToolbarHost;

    /// <summary>
    /// Raised when the mounted root goes away. The framework's overlays and requests in flight subscribe when they are
    /// first used and clear themselves, so an application that never uses them does not carry their code.
    /// </summary>
    internal static event Action? Unmounting;

    // The mounted page's OnUnmount handlers, and those registered while no page was mounted (as while a page builds),
    // which are the next page's.
    private static Action? unmountHandlers;
    private static Action? nextPageUnmountHandlers;

    /// <summary>
    /// Runs <paramref name="handler"/> once when the page unmounts: when the application closes, when a routed site
    /// swaps in another route, or when <see cref="Mount"/> replaces the page. Dispose there what the page set up
    /// outside its nodes, such as a <c>JSSubscription</c> to the page's JavaScript. A handler registered while a page
    /// builds belongs to that page, and runs after the framework has cleared the page's overlays and requests.
    /// </summary>
    public static void OnUnmount(Action handler)
    {
        if (mountedShell is null)
        {
            nextPageUnmountHandlers += handler;
        }
        else
        {
            unmountHandlers += handler;
        }
    }

    private static void DisposeAppScopedWork()
    {
        Unmounting?.Invoke();
        var handlers = unmountHandlers;
        unmountHandlers = null;
        handlers?.Invoke();
    }

    internal static void Reset()
    {
        ContextMenu.HideActive();
        Dialog.Active?.Hide();
        FocusAdorner.Reset();
        SelectionHandles.Reset();
        TextSelectionToolbar.Reset();
        ToolTipManager.Reset();
        AnimationManager.Shared.Reset();
        Worker.Reset();
        AfterCommit = null;
        mountedShell = null;
        MountedRoot = null;
        unmountHandlers = null;
        nextPageUnmountHandlers = null;
        Current = null;
        ViewportWidth.Reset(0.0f);
        ViewportHeight.Reset(0.0f);
        FrameTime.Reset(0.0);
    }
}

/// <summary>
/// The native root, painted with the theme background: the application's root node, then the framework's overlays,
/// each created on first use and stacked in a fixed order.
/// </summary>
internal sealed class ApplicationShell : FlexBox
{
    private Portal? selectionHandleHost;
    private Portal? textToolbarHost;
    private Portal? focusHost;
    private ContextMenu? contextMenu;

    public ApplicationShell(Node root)
    {
        Width = Length.Percent(100.0f);
        Height = Length.Percent(100.0f);
        AddChild(root);
        Track(Theme.Bind(theme => Background = theme.Colors.Background));
    }

    public Portal SelectionHandleHost
    {
        get
        {
            if (selectionHandleHost is null)
            {
                selectionHandleHost = FullViewportPortal();
                AddOverlay(selectionHandleHost);
            }
            return selectionHandleHost;
        }
    }

    public Portal TextToolbarHost
    {
        get
        {
            if (textToolbarHost is null)
            {
                textToolbarHost = FullViewportPortal();
                AddOverlay(textToolbarHost);
            }
            return textToolbarHost;
        }
    }

    public Portal FocusHost
    {
        get
        {
            if (focusHost is null)
            {
                focusHost = new Portal();
                focusHost.PositionAbsolute().Position(0.0f, 0.0f).Width(0.0f).Height(0.0f);
                focusHost.ClipToBounds = false;
                AddOverlay(focusHost);
            }
            return focusHost;
        }
    }

    public ContextMenu ContextMenu
    {
        get
        {
            if (contextMenu is null)
            {
                contextMenu = new ContextMenu();
                AddOverlay(contextMenu);
            }
            return contextMenu;
        }
    }

    private static Portal FullViewportPortal()
    {
        var portal = new Portal();
        portal.PositionAbsolute().Position(0.0f, 0.0f).Width(Length.Percent(100.0f)).Height(Length.Percent(100.0f));
        return portal;
    }

    /// <summary>
    /// Adds an overlay over the application, below the overlays stacked above it: selection handles, then the text
    /// toolbar, the focus ring and the context menu on top. Overlays created earlier but stacked higher move back up.
    /// </summary>
    private void AddOverlay(Node overlay)
    {
        AddChild(overlay);
        var above = false;
        foreach (var existing in (ReadOnlySpan<Node?>)[selectionHandleHost, textToolbarHost, focusHost, contextMenu])
        {
            if (above && existing is not null)
            {
                RemoveChild(existing);
                AddChild(existing);
            }
            above |= ReferenceEquals(existing, overlay);
        }
    }
}
