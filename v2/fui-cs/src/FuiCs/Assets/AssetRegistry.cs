using FuiCs.Bindings;
using FuiCs.Hosting;

namespace FuiCs;

/// <summary>Where an asset's load stands.</summary>
public enum AssetLoadState
{
    /// <summary>Not requested.</summary>
    Idle = 0,
    /// <summary>Requested, not finished.</summary>
    Loading = 1,
    /// <summary>Loaded and ready to draw.</summary>
    Ready = 2,
    /// <summary>Failed; the error says why.</summary>
    Failed = 3,
}

/// <summary>The load state, intrinsic size and error of one texture or SVG asset.</summary>
internal sealed class AssetRecord
{
    public Signal<AssetLoadState> State { get; } = new(AssetLoadState.Idle);

    public string Error { get; set; } = "";

    public float Width { get; set; }

    public float Height { get; set; }

    public string Url { get; set; } = "";
}

/// <summary>
/// The host-loaded assets of one kind. Assets acquired by URL share one id while referenced and are released with
/// the last reference; assets loaded with an explicit id stay pinned.
/// </summary>
internal sealed class AssetTable(string kind, uint firstDynamicId, Action<uint, string> load, Action<uint> release)
{
    private readonly Dictionary<uint, AssetRecord> records = [];
    private readonly Dictionary<string, uint> idsByUrl = [];
    private readonly Dictionary<uint, int> referenceCounts = [];
    private readonly HashSet<uint> pinned = [];
    private readonly uint firstId = firstDynamicId;
    private uint nextDynamicId = firstDynamicId;

    public AssetRecord Record(uint id)
    {
        if (!records.TryGetValue(id, out var record))
        {
            record = new AssetRecord();
            records[id] = record;
        }
        return record;
    }

    /// <summary>A fresh id from the dynamic range, for an asset the caller fills itself, such as a bitmap.</summary>
    public uint AllocateId() => AllocateDynamicId();

    /// <summary>Loads <paramref name="url"/> under a caller-chosen id that stays loaded.</summary>
    public void LoadPinned(uint id, string url) => Load(id, url, isPinned: true);

    /// <summary>The id of <paramref name="url"/>'s asset, loading it on first use; 0 for an empty URL.</summary>
    public uint Acquire(string url)
    {
        if (url.Length == 0)
        {
            return 0;
        }
        if (!idsByUrl.TryGetValue(url, out var id))
        {
            id = AllocateDynamicId();
            Load(id, url, isPinned: false);
        }
        referenceCounts[id] = referenceCounts.GetValueOrDefault(id) + 1;
        return id;
    }

    /// <summary>Drops one reference; the host releases the asset with the last one unless it is pinned.</summary>
    public void Release(uint id)
    {
        if (id == 0 || !referenceCounts.TryGetValue(id, out var count))
        {
            return;
        }
        if (count > 1)
        {
            referenceCounts[id] = count - 1;
            return;
        }
        referenceCounts.Remove(id);
        if (pinned.Contains(id))
        {
            return;
        }
        if (records.Remove(id, out var record))
        {
            RemoveUrlBinding(record.Url, id);
        }
        release(id);
    }

    public void MarkLoaded(uint id, float width, float height)
    {
        var record = Record(id);
        record.Error = "";
        record.Width = width;
        record.Height = height;
        record.State.Value = AssetLoadState.Ready;
    }

    public void MarkFailed(uint id, string error)
    {
        var record = Record(id);
        record.Error = error;
        record.Width = 0.0f;
        record.Height = 0.0f;
        record.State.Value = AssetLoadState.Failed;
        Logger.Warn("Assets", kind + " load failed for \"" + record.Url + "\": " + (error.Length > 0 ? error : "unknown error"));
    }

    public void Reset()
    {
        records.Clear();
        idsByUrl.Clear();
        referenceCounts.Clear();
        pinned.Clear();
        nextDynamicId = firstId;
    }

    private void Load(uint id, string url, bool isPinned)
    {
        var record = Record(id);
        if (record.Url.Length > 0 && record.Url != url)
        {
            RemoveUrlBinding(record.Url, id);
        }
        idsByUrl[url] = id;
        if (isPinned)
        {
            pinned.Add(id);
        }
        else
        {
            pinned.Remove(id);
        }
        record.Url = url;
        record.Error = "";
        record.Width = 0.0f;
        record.Height = 0.0f;
        record.State.Value = AssetLoadState.Loading;
        load(id, url);
    }

    private uint AllocateDynamicId()
    {
        if (nextDynamicId == uint.MaxValue)
        {
            throw new InvalidOperationException("The dynamic " + kind + " asset id space is exhausted.");
        }
        return nextDynamicId++;
    }

    private void RemoveUrlBinding(string url, uint id)
    {
        if (url.Length > 0 && idsByUrl.TryGetValue(url, out var bound) && bound == id)
        {
            idsByUrl.Remove(url);
        }
    }
}

/// <summary>The texture and SVG assets the host loads for <see cref="Image"/> and <see cref="Svg"/>.</summary>
internal static class AssetRegistry
{
    static AssetRegistry()
    {
        HostEvents.TextureLoaded = Textures.MarkLoaded;
        HostEvents.TextureFailed = Textures.MarkFailed;
        HostEvents.SvgLoaded = Svgs.MarkLoaded;
        HostEvents.SvgFailed = Svgs.MarkFailed;
    }

    public static AssetTable Textures { get; } = new("Texture", 0x20000000, Ui.LoadTexture, Ui.ReleaseTexture);

    public static AssetTable Svgs { get; } = new("SVG", 0x10000000, Ui.LoadSvg, Ui.ReleaseSvg);

    public static void Reset()
    {
        Textures.Reset();
        Svgs.Reset();
    }
}
