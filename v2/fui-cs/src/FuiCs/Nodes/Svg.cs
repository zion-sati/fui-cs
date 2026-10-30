using FuiCs.Bindings;

namespace FuiCs;

/// <summary>
/// An SVG loaded by the host from a URL, or shown by the id of an SVG loaded elsewhere, optionally tinted. An auto
/// width or height takes the SVG's intrinsic size once it loads, keeping its aspect ratio against a fixed other side.
/// </summary>
public class Svg : FlexBox, IImageContent
{
    private readonly ImageAssetBinding asset;
    private Length requestedWidth = Length.Auto;
    private Length requestedHeight = Length.Auto;
    private Color? tint;
    private ImageSampling sampling = ImageSampling.Linear;

    /// <summary>
    /// An SVG showing the loaded SVG <paramref name="svgId"/>, tinted when <paramref name="tint"/> is set.
    /// </summary>
    public Svg(uint svgId = 0, Color? tint = null)
    {
        this.tint = tint;
        asset = new ImageAssetBinding(AssetRegistry.Svgs, HandleAssetStateChanged);
        asset.UseId(svgId);
    }

    /// <summary>An SVG loading <paramref name="url"/>.</summary>
    public static Svg Load(string url, Color? tint = null) => new(0, tint) { Source = url };

    /// <summary>Raised as the SVG's asset loads or fails.</summary>
    public event Action<AssetLoadState>? AssetStateChanged;

    /// <summary>The SVG shown; setting it shows an SVG loaded elsewhere and clears <see cref="Source"/>.</summary>
    public uint SvgId
    {
        get => asset.Id;
        set
        {
            asset.UseId(value);
            ApplySource();
        }
    }

    /// <summary>The SVG's URL; setting it loads it.</summary>
    public string? Source
    {
        get => asset.Url.Length > 0 ? asset.Url : null;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
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

    /// <summary>Recolors the SVG's shapes; null draws its own colors.</summary>
    public Color? Tint
    {
        get => tint;
        set
        {
            tint = value;
            SendSvg();
        }
    }

    /// <summary>How the SVG is sampled when scaled.</summary>
    public ImageSampling Sampling
    {
        get => sampling;
        set
        {
            sampling = value;
            SendSvg();
        }
    }

    /// <summary>The SVG's text alternative for assistive technology.</summary>
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
        BuildStyledNode(NodeType.Svg, includeChildren: false);
        SendSvg();
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
        SendSvg();
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

    private void SendSvg()
    {
        if (!IsBuilt)
        {
            return;
        }
        Ui.SetSvg(Handle, asset.Id, tint?.Rgba ?? 0u, sampling.Kind, sampling.MaxAnisotropy);
        NotifyRetainedMutation();
    }
}
