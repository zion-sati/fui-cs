namespace FuiCs;

/// <summary>
/// A texture or SVG the host loads from a URL, for drawing or for showing by id. Everything holding the same URL
/// shares one host asset, which the host releases once the last holder is disposed; an <see cref="Image"/> or
/// <see cref="Svg"/> showing the URL is a holder too. A relative URL resolves against the page's address.
/// </summary>
public abstract class Asset : IDisposable
{
    private readonly AssetTable table;
    private Action<AssetLoadState>? stateChanged;
    private IDisposable? subscription;

    private protected Asset(AssetTable table, string url)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        this.table = table;
        Url = url;
        Id = table.Acquire(url);
    }

    /// <summary>The host's id for the asset, for <see cref="DrawContext"/> and nodes shown by id; 0 once disposed.</summary>
    public uint Id { get; private set; }

    /// <summary>The asset's URL.</summary>
    public string Url { get; }

    /// <summary>The asset's loading state.</summary>
    public AssetLoadState State => Id == 0 ? AssetLoadState.Idle : table.Record(Id).State.Value;

    /// <summary>Why the asset failed to load; empty otherwise.</summary>
    public string Error => Id == 0 ? "" : table.Record(Id).Error;

    /// <summary>The intrinsic width once loaded; 0 before.</summary>
    public float Width => Id == 0 ? 0.0f : table.Record(Id).Width;

    /// <summary>The asset's natural height once loaded.</summary>
    public float Height => Id == 0 ? 0.0f : table.Record(Id).Height;

    /// <summary>Raised when the asset becomes ready or fails, whoever loaded it.</summary>
    public event Action<AssetLoadState>? StateChanged
    {
        add
        {
            if (Id == 0)
            {
                return;
            }
            stateChanged += value;
            subscription ??= table.Record(Id).State.Subscribe(state => stateChanged?.Invoke(state));
        }
        remove => stateChanged -= value;
    }

    /// <summary>Drops this holder's reference to the asset.</summary>
    public void Dispose()
    {
        if (Id == 0)
        {
            return;
        }
        subscription?.Dispose();
        subscription = null;
        stateChanged = null;
        table.Release(Id);
        Id = 0;
    }
}

/// <summary>A texture the host loads from a URL; see <see cref="Asset"/>.</summary>
public sealed class TextureAsset : Asset
{
    private TextureAsset(string url)
        : base(AssetRegistry.Textures, url)
    {
    }

    /// <summary>Loads the texture at <paramref name="url"/>, sharing a load already started for it.</summary>
    public static TextureAsset Load(string url) => new(url);
}

/// <summary>An SVG the host loads from a URL; see <see cref="Asset"/>.</summary>
public sealed class SvgAsset : Asset
{
    private SvgAsset(string url)
        : base(AssetRegistry.Svgs, url)
    {
    }

    /// <summary>Loads the SVG at <paramref name="url"/>, sharing a load already started for it.</summary>
    public static SvgAsset Load(string url) => new(url);
}
