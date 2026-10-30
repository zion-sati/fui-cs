namespace FuiCs;

/// <summary>How a texture or SVG is sampled when it is scaled.</summary>
public readonly record struct ImageSampling(ImageSamplingKind Kind, uint MaxAnisotropy = 0)
{
    /// <summary>Bilinear filtering.</summary>
    public static ImageSampling Linear => new(ImageSamplingKind.Linear);

    /// <summary>Nearest-pixel sampling, for pixel art.</summary>
    public static ImageSampling Nearest => new(ImageSamplingKind.Nearest);

    /// <summary>Bilinear filtering between the nearest mipmap level's pixels.</summary>
    public static ImageSampling LinearMipmapNearest => new(ImageSamplingKind.LinearMipmapNearest);

    /// <summary>Trilinear filtering across mipmap levels.</summary>
    public static ImageSampling LinearMipmapLinear => new(ImageSamplingKind.LinearMipmapLinear);

    /// <summary>Mitchell cubic filtering.</summary>
    public static ImageSampling CubicMitchell => new(ImageSamplingKind.CubicMitchell);

    /// <summary>Catmull-Rom cubic filtering, sharper than Mitchell.</summary>
    public static ImageSampling CubicCatmullRom => new(ImageSamplingKind.CubicCatmullRom);

    /// <summary>Anisotropic filtering up to <paramref name="maxAnisotropy"/> samples.</summary>
    public static ImageSampling Anisotropic(uint maxAnisotropy = 8) => new(ImageSamplingKind.Anisotropic, maxAnisotropy);
}

/// <summary>A node that shows a host-loaded asset: <see cref="Image"/> or <see cref="Svg"/>.</summary>
public interface IImageContent
{
    /// <summary>The asset URL. Null or empty clears the content.</summary>
    string? Source { get; set; }

    /// <summary>How the content is sampled when scaled.</summary>
    ImageSampling Sampling { get; set; }

    /// <summary>Announces the node as an image with this label.</summary>
    string? AltText { get; set; }

    /// <summary>The asset's loading state.</summary>
    AssetLoadState AssetState { get; }

    /// <summary>Raised when the shown asset's load state changes, including when another node loaded the same URL.</summary>
    event Action<AssetLoadState>? AssetStateChanged;

    /// <summary>Why the asset failed to load; empty otherwise.</summary>
    string AssetError { get; }

    /// <summary>The asset's intrinsic width once loaded; 0 before.</summary>
    float AssetWidth { get; }

    /// <summary>The asset's natural height once loaded.</summary>
    float AssetHeight { get; }
}

/// <summary>The asset shown by one node: the id in use, the URL it owns a reference to, and its load state.</summary>
internal sealed class ImageAssetBinding(AssetTable table, Action<AssetLoadState> stateChanged) : IDisposable
{
    private uint ownedId;
    private IDisposable? subscription;
    private uint trackedId;

    public uint Id { get; private set; }

    public string Url { get; private set; } = "";

    public AssetRecord Record => table.Record(Id);

    /// <summary>Shows an asset by id, which the caller loaded; drops any URL-owned reference.</summary>
    public void UseId(uint id)
    {
        ReleaseOwned();
        Url = "";
        Id = id;
        Track();
    }

    /// <summary>Shows the asset at <paramref name="url"/>; returns false when it is already shown.</summary>
    public bool UseUrl(string url)
    {
        if (ownedId != 0 && Url == url)
        {
            return false;
        }
        ReleaseOwned();
        Url = url;
        Id = table.Acquire(url);
        ownedId = Id;
        Track();
        return true;
    }

    public void Dispose()
    {
        subscription?.Dispose();
        subscription = null;
        trackedId = 0;
        ReleaseOwned();
    }

    private void Track()
    {
        if (trackedId == Id && subscription is not null)
        {
            return;
        }
        subscription?.Dispose();
        subscription = null;
        trackedId = Id;
        if (Id != 0)
        {
            subscription = table.Record(Id).State.Subscribe(stateChanged);
        }
    }

    private void ReleaseOwned()
    {
        if (ownedId == 0)
        {
            return;
        }
        table.Release(ownedId);
        ownedId = 0;
    }
}

/// <summary>Resolves an image's auto width or height from the loaded asset, keeping its aspect ratio.</summary>
internal static class IntrinsicSize
{
    public static Length Resolve(Length requested, Length otherRequested, float assetSize, float otherAssetSize)
    {
        if (requested.Unit != Unit.Auto || assetSize <= 0.0f || otherAssetSize <= 0.0f)
        {
            return requested;
        }
        return otherRequested.Unit == Unit.Pixel
            ? Length.Pixels(otherRequested.Value * (assetSize / otherAssetSize))
            : Length.Pixels(assetSize);
    }
}
