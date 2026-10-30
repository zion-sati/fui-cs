using FuiCs;

namespace FuiApp;

/// <summary>The application: FuiCs runs <see cref="CreateApplication"/> when the page loads.</summary>
public static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(BuildPage);

    private static Node BuildPage()
    {
        var count = 0;
        var title = new Text("Hello from FuiApp").FontSize(28.0f).FontWeight(FontWeight.Bold);
        var label = new Text("Clicked 0 times").FontSize(18.0f);
        var button = new Button("Click me")
            .Margin(0.0f, 16.0f, 0.0f, 0.0f)
            .OnClick(_ =>
            {
                count++;
                label.Text = count == 1 ? "Clicked once" : "Clicked " + count + " times";
            });
        return FlexBox.Column(title, FlexBox.Column(label).Margin(0.0f, 8.0f, 0.0f, 0.0f), button)
            .Padding(32.0f)
            .AlignItems(AlignItems.Start);
    }
}
