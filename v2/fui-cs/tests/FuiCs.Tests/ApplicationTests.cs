using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Hosting;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class ApplicationTests
{
    private sealed class TestController(Node root, List<string> log) : ManagedApplicationController
    {
        public override Node GetRoot() => root;

        public override void Mount()
        {
            log.Add("mount");
            base.Mount();
        }

        public override void Dispose()
        {
            log.Add("dispose");
            base.Dispose();
        }
    }

    [Test]
    public async Task MountResetsTheTreeBuildsTheShellThenTheRootAndCommitsOnFlush()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();

        Application.Mount(root);

        await Assert.That(abi.Sequence("ui_reset", "ui_resize_window", "ui_create_node", "ui_node_add_child", "ui_set_root", "ui_commit_frame"))
            .IsEqualTo("ui_reset,ui_resize_window,ui_create_node,ui_create_node,ui_node_add_child,ui_set_root");
        await Assert.That(abi.Arguments("ui_set_root", 0)).IsEqualTo("1");
        await Assert.That(abi.Arguments("ui_set_width", 2)).IsEqualTo("Percent");
        await Assert.That(ReferenceEquals(root.Parent, null)).IsFalse();

        Application.FlushRenders();
        Application.FlushRenders();

        await Assert.That(abi.CountOf("ui_commit_frame")).IsEqualTo(1);
    }

    [Test]
    public async Task MountingANewRootDisposesAndUnregistersTheOldTree()
    {
        TestRuntime.Start();
        var first = new TestNode();
        Application.Mount(first);
        var firstHandle = first.Handle;

        Application.Mount(new TestNode());

        await Assert.That(first.IsDisposed).IsTrue();
        await Assert.That(EventRouter.GetRegisteredNode(firstHandle)).IsNull();
    }

    [Test]
    public async Task ARetainedUpdateCommitsWithoutRebuildingTheRoot()
    {
        var abi = TestRuntime.Start();
        var root = new TestNode();
        Application.Mount(root);
        Application.FlushRenders();
        abi.Calls.Clear();

        root.NodeId = "updated";
        Application.FlushRenders();

        await Assert.That(abi.Sequence("ui_reset", "ui_create_node", "ui_set_root", "ui_set_node_id", "request_render", "ui_commit_frame"))
            .IsEqualTo("ui_set_node_id,request_render,ui_commit_frame");
    }

    [Test]
    public async Task RunReplacesThePageAndDisposeUnmountsIt()
    {
        TestRuntime.Start();
        var app = Application.Create(() => new TestNode());

        app.Run();
        var first = app.ActivePage;
        app.Run();
        var second = app.ActivePage;
        app.Dispose();

        await Assert.That(first!.IsDisposed).IsTrue();
        await Assert.That(ReferenceEquals(first, second)).IsFalse();
        await Assert.That(second!.IsDisposed).IsTrue();
        await Assert.That(app.ActivePage).IsNull();
        await Assert.That(Application.MountedRoot).IsNull();
    }

    [Test]
    public async Task APageUnmountHandlerRunsOnceWhenThatPageUnmounts()
    {
        TestRuntime.Start();
        var log = new List<string>();
        var builds = 0;
        var app = Application.Create(() =>
        {
            var page = ++builds;
            Application.OnUnmount(() => log.Add("unmount " + page));
            return new TestNode();
        });

        app.Run();
        var afterMount = string.Join(",", log);
        app.Run();
        var afterReplace = string.Join(",", log);
        app.Dispose();
        app.Dispose();

        // Building the second page does not run its handler when the first page unmounts.
        await Assert.That(afterMount).IsEqualTo("");
        await Assert.That(afterReplace).IsEqualTo("unmount 1");
        await Assert.That(string.Join(",", log)).IsEqualTo("unmount 1,unmount 2");
    }

    [Test]
    public async Task AnUnmountHandlerRegisteredWhileMountedBelongsToThePageAndTheHostDisposeRunsIt()
    {
        TestRuntime.Start();
        var unmounted = 0;

        HostEvents.RunApp(() => Application.Create(() => new TestNode()));
        Application.OnUnmount(() => unmounted++);
        var beforeDispose = unmounted;
        HostEvents.DisposeApp();

        await Assert.That(beforeDispose).IsEqualTo(0);
        await Assert.That(unmounted).IsEqualTo(1);
    }

    [Test]
    public async Task AManagedControllerMountsAndDisposesItself()
    {
        TestRuntime.Start();
        var log = new List<string>();
        var root = new TestNode();
        var app = Application.CreateManaged(() => new TestController(root, log));

        app.Run();
        var mounted = ReferenceEquals(Application.MountedRoot, root);
        app.Dispose();

        await Assert.That(mounted).IsTrue();
        await Assert.That(string.Join(",", log)).IsEqualTo("mount,dispose");
        await Assert.That(root.IsDisposed).IsTrue();
    }

    [Test]
    public async Task TheHostCreatesRunsFlushesAndDisposesTheApplication()
    {
        var abi = TestRuntime.Start();
        var loaded = 0;
        FrameScheduler.OnLoaded(_ => loaded++);

        HostEvents.RunApp(() => Application.Register(app => app.Page(() => new TestNode())));
        HostEvents.FlushRenders();
        var mounted = Application.MountedRoot is not null;
        var current = Application.Current is not null;
        HostEvents.DisposeApp();

        await Assert.That(mounted).IsTrue();
        await Assert.That(current).IsTrue();
        await Assert.That(loaded).IsEqualTo(1);
        await Assert.That(abi.CountOf("ui_commit_frame")).IsEqualTo(1);
        await Assert.That(Application.MountedRoot).IsNull();
    }

    [Test]
    public async Task TheHostRunsAnExistingApplicationWithoutCreatingAnother()
    {
        TestRuntime.Start();
        var root = new TestNode();
        var existing = Application.Create(() => root);
        var created = 0;

        HostEvents.RunApp(() =>
        {
            created++;
            return Application.Create(() => new TestNode());
        });

        await Assert.That(created).IsEqualTo(0);
        await Assert.That(ReferenceEquals(Application.Current, existing)).IsTrue();
        await Assert.That(ReferenceEquals(Application.MountedRoot, root)).IsTrue();
    }

    [Test]
    public async Task TheHostUpdatesViewportAndFrameSignals()
    {
        TestRuntime.Start();

        HostEvents.OnViewportChanged(800.0f, 600.0f);
        HostEvents.OnFrame(16.5);

        await Assert.That(Application.ViewportWidth.Value).IsEqualTo(800.0f);
        await Assert.That(Application.ViewportHeight.Value).IsEqualTo(600.0f);
        await Assert.That(Application.FrameTime.Value).IsEqualTo(16.5);
    }
}
