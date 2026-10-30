using FuiCs;

namespace FuiCs.Demo.ScrollbarGutter;

internal sealed class ScrollbarGutterBugController : ManagedApplicationController
{
    private readonly ScrollbarGutterBugView view;

    public ScrollbarGutterBugController()
    {
        Model = new ScrollbarGutterBugModel();
        view = new ScrollbarGutterBugView(Model);
    }

    public ScrollbarGutterBugModel Model { get; }

    public override Node GetRoot() => view.Root;
}
