using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// A texture loaded by the host from a URL, or shown by the id of a texture loaded elsewhere. An auto width or
/// height takes the texture's intrinsic size once it loads, keeping its aspect ratio against a fixed other side.
/// </summary>
public class Image : FlexBox, IImageContent
{
    private readonly ImageAssetBinding asset;
    private Length requestedWidth = Length.Auto;
    private Length requestedHeight = Length.Auto;
    private ObjectFit objectFit;
    private ImageSampling sampling = ImageSampling.Linear;
    private Thickness? ninePatch;

    /// <summary>
    /// An image showing the loaded texture <paramref name="textureId"/>, fitted as <paramref name="objectFit"/>.
    /// </summary>
    public Image(uint textureId = 0, ObjectFit objectFit = ObjectFit.Fill)
    {
        this.objectFit = objectFit;
        asset = new ImageAssetBinding(AssetRegistry.Textures, HandleAssetStateChanged);
        asset.UseId(textureId);
    }

    /// <summary>An image loading <paramref name="url"/>.</summary>
    public static Image Load(string url, ObjectFit objectFit = ObjectFit.Fill) => new(0, objectFit) { Source = url };

    /// <summary>Raised as the image's asset loads or fails.</summary>
    public event Action<AssetLoadState>? AssetStateChanged;

    /// <summary>The texture shown; setting it shows a texture loaded elsewhere and clears <see cref="Source"/>.</summary>
    public uint TextureId
    {
        get => asset.Id;
        set
        {
            asset.UseId(value);
            ApplySource();
        }
    }

    /// <summary>The image's URL; setting it loads it.</summary>
    public string? Source
    {
        get => asset.Url.Length > 0 ? asset.Url : null;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                ninePatch = null;
                asset.UseId(0);
                ApplySource();
                return;
            }
            if (asset.UseUrl(value))
            {
                ApplySource();
            }
        }
    }

    /// <summary>How the image fits its box.</summary>
    public ObjectFit ObjectFit
    {
        get => objectFit;
        set
        {
            objectFit = value;
            SendImage();
        }
    }

    /// <summary>How the image is sampled when scaled.</summary>
    public ImageSampling Sampling
    {
        get => sampling;
        set
        {
            sampling = value;
            SendImage();
        }
    }

    /// <summary>Stretches the texture as a nine-patch: these insets stay unscaled at the edges. Null draws it whole.</summary>
    public Thickness? NinePatch
    {
        get => ninePatch;
        set
        {
            ninePatch = value;
            SendImage();
        }
    }

    /// <summary>The image's text alternative for assistive technology.</summary>
    public string? AltText
    {
        get => SemanticLabel;
        set
        {
            SemanticRole = FuiCs.SemanticRole.Image;
            SemanticLabel = value;
        }
    }

    /// <summary>The asset's loading state.</summary>
    public AssetLoadState AssetState => asset.Id == 0 ? AssetLoadState.Idle : asset.Record.State.Value;

    /// <summary>Why the asset failed to load; empty unless it failed.</summary>
    public string AssetError => asset.Id == 0 ? "" : asset.Record.Error;

    /// <summary>The asset's natural width once loaded.</summary>
    public float AssetWidth => asset.Id == 0 ? 0.0f : asset.Record.Width;

    /// <summary>The asset's natural height once loaded.</summary>
    public float AssetHeight => asset.Id == 0 ? 0.0f : asset.Record.Height;

    /// <inheritdoc/>
    public override Length Width
    {
        get => requestedWidth;
        set
        {
            requestedWidth = value;
            ApplyIntrinsicSize();
        }
    }

    /// <inheritdoc/>
    public override Length Height
    {
        get => requestedHeight;
        set
        {
            requestedHeight = value;
            ApplyIntrinsicSize();
        }
    }

    /// <inheritdoc/>
    public override ulong Build()
    {
        if (IsBuilt)
        {
            return Handle;
        }
        ApplyIntrinsicSize();
        BuildStyledNode(NodeType.Image, includeChildren: false);
        SendImage();
        return Handle;
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        asset.Dispose();
        base.Dispose();
    }

    private void HandleAssetStateChanged(AssetLoadState state)
    {
        ApplyIntrinsicSize();
        AssetStateChanged?.Invoke(state);
    }

    private void ApplySource()
    {
        ApplyIntrinsicSize();
        SendImage();
    }

    private void ApplyIntrinsicSize()
    {
        if (!FillWidth && FillWidthPercent is null)
        {
            base.Width = IntrinsicSize.Resolve(requestedWidth, requestedHeight, AssetWidth, AssetHeight);
        }
        if (!FillHeight && FillHeightPercent is null)
        {
            base.Height = IntrinsicSize.Resolve(requestedHeight, requestedWidth, AssetHeight, AssetWidth);
        }
    }

    private void SendImage()
    {
        if (!IsBuilt)
        {
            return;
        }
        if (ninePatch is { } insets)
        {
            Ui.SetImageNine(Handle, asset.Id, insets.Left, insets.Top, insets.Right, insets.Bottom, sampling.Kind, sampling.MaxAnisotropy);
        }
        else
        {
            Ui.SetImage(Handle, asset.Id, objectFit, sampling.Kind, sampling.MaxAnisotropy);
        }
        NotifyRetainedMutation();
    }
}
