using FuiCs.Bindings;

namespace FuiCs;

/// <summary>A font's slant.</summary>
public enum FontStyle
{
    /// <summary>Upright.</summary>
    Normal = 0,
    /// <summary>Italic.</summary>
    Italic = 1,
}

/// <summary>A font's weight, on the CSS scale.</summary>
public enum FontWeight
{
    /// <summary>Normal weight (400).</summary>
    Regular = 400,
    /// <summary>Medium (500).</summary>
    Medium = 500,
    /// <summary>Semibold (600).</summary>
    Semibold = 600,
    /// <summary>Bold (700).</summary>
    Bold = 700,
}

/// <summary>A font that finished loading.</summary>
public sealed class FontFaceLoadedEventArgs(FontFace font) : EventArgs
{
    /// <summary>The font.</summary>
    public FontFace Font { get; } = font;
}

/// <summary>A set of fonts that finished loading.</summary>
public sealed class FontsLoadedEventArgs(IReadOnlyList<uint> fontIds) : EventArgs
{
    /// <summary>The loaded fonts' ids.</summary>
    public IReadOnlyList<uint> FontIds { get; } = fontIds;
}

/// <summary>A font stack whose fonts finished loading.</summary>
public sealed class FontStackLoadedEventArgs(FontStack stack, IReadOnlyList<uint> fontIds) : EventArgs
{
    /// <summary>The stack.</summary>
    public FontStack Stack { get; } = stack;

    /// <summary>The ids of the stack's fonts.</summary>
    public IReadOnlyList<uint> FontIds { get; } = fontIds;
}

/// <summary>A font the host has loaded or is loading, identified by its font id.</summary>
public readonly record struct FontFace(uint Id)
{
    /// <summary>Loads a font from <paramref name="url"/> under a newly allocated id.</summary>
    public static FontFace Load(string url) => Load(url, FontRegistry.AllocateId());

    /// <summary>Loads a font from <paramref name="url"/> under <paramref name="id"/>.</summary>
    public static FontFace Load(string url, uint id)
    {
        FontRegistry.LoadFont(id, url, "FontFace.Load");
        return new FontFace(id);
    }

    /// <summary>Whether the host has loaded the font.</summary>
    public bool IsLoaded => FontRegistry.IsLoaded(Id);

    /// <summary>Runs <paramref name="handler"/> once the font has loaded, or now if it already has.</summary>
    public FontFace OnLoaded(Action<FontFaceLoadedEventArgs> handler)
    {
        WhenLoaded(Id, handler);
        return this;
    }

    /// <summary>Whether the host has loaded the font with <paramref name="fontId"/>.</summary>
    public static bool IsFontLoaded(uint fontId) => FontRegistry.IsLoaded(fontId);

    /// <summary>
    /// Runs <paramref name="handler"/> once the font with <paramref name="fontId"/> has loaded, or now if it already
    /// has.
    /// </summary>
    public static void WhenLoaded(uint fontId, Action<FontFaceLoadedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        FontRegistry.WhenLoaded([fontId], () => handler(new FontFaceLoadedEventArgs(new FontFace(fontId))));
    }

    /// <summary>Runs <paramref name="handler"/> once every font in <paramref name="fontIds"/> has loaded.</summary>
    public static void WhenFontsLoaded(IReadOnlyList<uint> fontIds, Action<FontsLoadedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var unique = FontRegistry.Unique(fontIds);
        FontRegistry.WhenLoaded(unique, () => handler(new FontsLoadedEventArgs(unique)));
    }
}

/// <summary>A primary font with fallbacks for glyphs it lacks.</summary>
public sealed class FontStack(FontFace face)
{
    private readonly List<uint> fallbackIds = [];

    /// <summary>The primary font.</summary>
    public FontFace Face { get; } = face;

    /// <summary>The primary font's id, which the stack is known by.</summary>
    public uint Id => Face.Id;

    /// <summary>Whether the primary font and every fallback have loaded.</summary>
    public bool IsLoaded => FontRegistry.AreLoaded(RequiredFontIds());

    /// <summary>A stack of the font at <paramref name="url"/>, loaded under a newly allocated id.</summary>
    public static FontStack Load(string url) => new(FontFace.Load(url));

    /// <summary>A stack of the font at <paramref name="url"/>, loaded under <paramref name="id"/>.</summary>
    public static FontStack Load(string url, uint id) => new(FontFace.Load(url, id));

    /// <summary>Adds <paramref name="face"/> as a fallback, tried after the fonts already in the stack.</summary>
    public FontStack Fallback(FontFace face) => AddFallback(face.Id);

    /// <summary>
    /// Adds <paramref name="stack"/>'s primary font as a fallback, tried after the fonts already in the stack.
    /// </summary>
    public FontStack Fallback(FontStack stack) => AddFallback(stack.Id);

    /// <summary>Loads a font from <paramref name="url"/> and adds it as a fallback.</summary>
    public FontStack FallbackLoaded(string url) => FallbackLoaded(url, FontRegistry.AllocateId());

    /// <summary>
    /// Loads a font from <paramref name="url"/> under <paramref name="fontId"/> and adds it as a fallback.
    /// </summary>
    public FontStack FallbackLoaded(string url, uint fontId)
    {
        FontRegistry.LoadFont(fontId, url, "FontStack.FallbackLoaded");
        return AddFallback(fontId);
    }

    /// <summary>The primary font and its fallbacks, without duplicates.</summary>
    public IReadOnlyList<uint> RequiredFontIds()
    {
        var fontIds = new List<uint>();
        FontRegistry.AddUnique(fontIds, Id);
        foreach (var fontId in fallbackIds)
        {
            FontRegistry.AddUnique(fontIds, fontId);
        }
        return fontIds;
    }

    /// <summary>Runs <paramref name="handler"/> once the primary font and every fallback have loaded.</summary>
    public FontStack OnLoaded(Action<FontStackLoadedEventArgs> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var fontIds = RequiredFontIds();
        FontRegistry.WhenLoaded(fontIds, () => handler(new FontStackLoadedEventArgs(this, fontIds)));
        return this;
    }

    internal static FontStack FromId(uint id) => new(new FontFace(id));

    internal FontStack AddFallback(uint fontId)
    {
        if (fontId == 0 || fontId == Id)
        {
            Logger.Warn("Typography", "FontStack.Fallback ignored font id 0 or the stack's own font.");
            return this;
        }
        if (fallbackIds.Contains(fontId))
        {
            return this;
        }
        FontRegistry.RegisterFallback(Id, fontId);
        fallbackIds.Add(fontId);
        return this;
    }
}

/// <summary>The stacks of one family by weight and style; <see cref="Resolve"/> picks the closest face.</summary>
public sealed class FontFamily(
    FontStack regular,
    FontStack? bold = null,
    FontStack? italic = null,
    FontStack? boldItalic = null,
    FontStack? medium = null,
    FontStack? mediumItalic = null,
    FontStack? semibold = null,
    FontStack? semiboldItalic = null)
{
    private const int StyleMismatchPenalty = 1000;

    /// <summary>The regular weight's stack, which every other style falls back to.</summary>
    public FontStack Regular { get; } = regular;

    /// <summary>The bold stack, if any.</summary>
    public FontStack? Bold { get; } = bold;

    /// <summary>The italic stack, if any.</summary>
    public FontStack? Italic { get; } = italic;

    /// <summary>The bold italic stack, if any.</summary>
    public FontStack? BoldItalic { get; } = boldItalic;

    /// <summary>The medium stack, if any.</summary>
    public FontStack? Medium { get; } = medium;

    /// <summary>The medium italic stack, if any.</summary>
    public FontStack? MediumItalic { get; } = mediumItalic;

    /// <summary>The semibold stack, if any.</summary>
    public FontStack? Semibold { get; } = semibold;

    /// <summary>The semibold italic stack, if any.</summary>
    public FontStack? SemiboldItalic { get; } = semiboldItalic;

    /// <summary>A family of one regular stack.</summary>
    public static FontFamily WithRegular(FontStack regular) => new(regular);

    /// <summary>A family of one regular font.</summary>
    public static FontFamily WithRegular(FontFace regular) => new(new FontStack(regular));

    /// <summary>A family of a regular and a bold stack.</summary>
    public static FontFamily RegularBold(FontStack regular, FontStack bold) => new(regular, bold);

    /// <summary>The ids of every font in the family's stacks, to wait on before measuring text in it.</summary>
    public IReadOnlyList<uint> RequiredFontIds()
    {
        var fontIds = new List<uint>();
        foreach (var stack in Stacks())
        {
            if (stack is not null)
            {
                FontRegistry.AddUnique(fontIds, stack.Id);
            }
        }
        return fontIds;
    }

    /// <summary>The font id of the face closest to <paramref name="weight"/>, preferring a matching <paramref name="style"/>.</summary>
    public uint Resolve(FontWeight weight = FontWeight.Regular, FontStyle style = FontStyle.Normal)
    {
        var bestId = 0u;
        var bestScore = int.MaxValue;
        Consider(Regular, FontWeight.Regular, FontStyle.Normal);
        Consider(Bold, FontWeight.Bold, FontStyle.Normal);
        Consider(Italic, FontWeight.Regular, FontStyle.Italic);
        Consider(BoldItalic, FontWeight.Bold, FontStyle.Italic);
        Consider(Medium, FontWeight.Medium, FontStyle.Normal);
        Consider(MediumItalic, FontWeight.Medium, FontStyle.Italic);
        Consider(Semibold, FontWeight.Semibold, FontStyle.Normal);
        Consider(SemiboldItalic, FontWeight.Semibold, FontStyle.Italic);
        if (bestId == 0)
        {
            Logger.Warn("Typography", "FontFamily.Resolve found no face for the requested weight and style; the text will use font id 0.");
        }
        return bestId;

        void Consider(FontStack? stack, FontWeight faceWeight, FontStyle faceStyle)
        {
            if (stack is null || stack.Id == 0)
            {
                return;
            }
            var score = Math.Abs((int)faceWeight - (int)weight) + (faceStyle != style ? StyleMismatchPenalty : 0);
            if (score < bestScore)
            {
                bestScore = score;
                bestId = stack.Id;
            }
        }
    }

    internal static FontFamily FromIds(uint regular, uint bold = 0, uint italic = 0, uint boldItalic = 0) =>
        new(FontStack.FromId(regular), StackOrNull(bold), StackOrNull(italic), StackOrNull(boldItalic));

    private static FontStack? StackOrNull(uint id) => id != 0 ? FontStack.FromId(id) : null;

    private FontStack?[] Stacks() => [Regular, Bold, Italic, BoldItalic, Medium, MediumItalic, Semibold, SemiboldItalic];
}

/// <summary>Font load state and the callbacks waiting on it.</summary>
internal static class FontRegistry
{
    private const uint FirstDynamicFontId = 1024;

    private static readonly HashSet<uint> loadedFontIds = [];
    private static readonly List<(uint FontId, uint FallbackId)> pendingFallbacks = [];
    private static readonly List<(uint FontId, Action Check)> waiting = [];
    private static uint nextDynamicFontId = FirstDynamicFontId;

    public static uint AllocateId() => nextDynamicFontId++;

    /// <summary>Registers a fallback, or queues it until the runtime starts (the theme's defaults are created early).</summary>
    public static void RegisterFallback(uint fontId, uint fallbackId)
    {
        if (!Runtime.IsInitialized)
        {
            pendingFallbacks.Add((fontId, fallbackId));
            return;
        }
        Ui.RegisterFontFallback(fontId, fallbackId);
    }

    public static void FlushPendingFallbacks()
    {
        foreach (var (fontId, fallbackId) in pendingFallbacks)
        {
            Ui.RegisterFontFallback(fontId, fallbackId);
        }
        pendingFallbacks.Clear();
    }

    public static void LoadFont(uint fontId, string url, string caller)
    {
        if (url.Length == 0)
        {
            Logger.Warn("Typography", caller + " received an empty font URL.");
        }
        Ui.LoadFont(fontId, url);
    }

    /// <summary>Font id 0 and the built-in fonts 1 to 6 are always available.</summary>
    public static bool IsLoaded(uint fontId) => fontId <= 6 || loadedFontIds.Contains(fontId);

    public static bool AreLoaded(IReadOnlyList<uint> fontIds)
    {
        for (var index = 0; index < fontIds.Count; ++index)
        {
            if (!IsLoaded(fontIds[index]))
            {
                return false;
            }
        }
        return true;
    }

    public static void AddUnique(List<uint> fontIds, uint fontId)
    {
        if (fontId != 0 && !fontIds.Contains(fontId))
        {
            fontIds.Add(fontId);
        }
    }

    public static List<uint> Unique(IReadOnlyList<uint> fontIds)
    {
        var unique = new List<uint>();
        for (var index = 0; index < fontIds.Count; ++index)
        {
            AddUnique(unique, fontIds[index]);
        }
        return unique;
    }

    /// <summary>Runs <paramref name="callback"/> once, when every font in <paramref name="fontIds"/> has loaded.</summary>
    public static void WhenLoaded(IReadOnlyList<uint> fontIds, Action callback)
    {
        if (AreLoaded(fontIds))
        {
            callback();
            return;
        }
        var done = false;
        void Check()
        {
            if (done || !AreLoaded(fontIds))
            {
                return;
            }
            done = true;
            callback();
        }
        for (var index = 0; index < fontIds.Count; ++index)
        {
            if (!IsLoaded(fontIds[index]))
            {
                waiting.Add((fontIds[index], Check));
            }
        }
    }

    public static void DispatchFontLoaded(uint fontId)
    {
        loadedFontIds.Add(fontId);
        var ready = new List<Action>();
        for (var index = waiting.Count - 1; index >= 0; --index)
        {
            if (waiting[index].FontId == fontId)
            {
                ready.Add(waiting[index].Check);
                waiting.RemoveAt(index);
            }
        }
        for (var index = ready.Count - 1; index >= 0; --index)
        {
            ready[index]();
        }
    }

    internal static void Reset()
    {
        loadedFontIds.Clear();
        waiting.Clear();
        nextDynamicFontId = FirstDynamicFontId;
    }
}
