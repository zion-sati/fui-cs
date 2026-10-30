using FuiCs;
using FuiRoutedApp.Shared;

namespace FuiRoutedApp.Settings;

/// <summary>The settings route: FuiCs runs <see cref="CreateApplication"/> when the route loads.</summary>
public static class FuiProgram
{
    public static IApplication CreateApplication() => Application.Create(BuildPage);

    private static Node BuildPage()
    {
        var status = new Text("Notifications are on").FontSize(16.0f);
        var toggle = new Switch("Notifications").IsChecked(true);
        toggle.Changed += change => status.Text = change.IsChecked ? "Notifications are on" : "Notifications are off";
        return Site.Page(
            "Settings",
            toggle,
            FlexBox.Column(status).Margin(0.0f, 12.0f, 0.0f, 0.0f));
    }
}
