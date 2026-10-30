using FuiCs;
using FuiRoutedApp.Shared;

namespace FuiRoutedApp;

/// <summary>The home route: FuiCs runs <see cref="CreateApplication"/> when the route loads.</summary>
public static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(BuildPage);

    private static Node BuildPage()
    {
        var count = 0;
        var label = new Text("Clicked 0 times").FontSize(18.0f);
        var button = new Button("Click me")
            .Margin(0.0f, 16.0f, 0.0f, 0.0f)
            .OnClick(_ =>
            {
                count++;
                label.Text = count == 1 ? "Clicked once" : "Clicked " + count + " times";
            });
        return Site.Page(
            "Home",
            FlexBox.Column(new Text("Each route is an application of its own; the links swap them in place.").FontSize(16.0f)),
            FlexBox.Column(label).Margin(0.0f, 16.0f, 0.0f, 0.0f),
            button);
    }
}
