namespace FuiCs;

/// <summary>A flex box rendered above the normal tree and not clipped by its ancestors, for overlays such as popups.</summary>
public class Portal : FlexBox
{
    /// <summary>An empty portal.</summary>
    public Portal()
    {
        ClipToBounds = false;
        IsPortal = true;
    }
}
