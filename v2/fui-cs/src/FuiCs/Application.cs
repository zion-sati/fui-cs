namespace FuiCs;

public static class Application
{
    private static BuiltNode? root;

    public static void Mount(INode nextRoot)
    {
        Runtime.Backend.Reset();
        Runtime.Backend.ResizeWindow(Runtime.Backend.GetViewportWidth(), Runtime.Backend.GetViewportHeight());

        root?.Destroy();
        root = nextRoot.Build();
        Runtime.Backend.SetRoot(root.Handle);
        Runtime.Backend.CommitFrame();
    }
}
