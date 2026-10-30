using System.Globalization;

using FuiCs;

namespace FuiCs.Browser;

/// <summary>
/// The Stage 3 route: buttons, checkboxes, a switch, radios, a slider and progress bar, a popup, a dialog and a
/// context menu, with a readout line that every control updates.
/// </summary>
internal static class GalleryPage
{
    private sealed class State
    {
        public int Clicks;
        public bool? Agree = false;
        public bool Wifi;
        public string Size = "none";
        public float Volume = 30.0f;
        public string Dialog = "none";
        public string Menu = "none";
        public bool PopupOpen;

        public string Describe() => string.Create(
            CultureInfo.InvariantCulture,
            $"clicks={Clicks} agree={(Agree is { } agree ? agree.ToString() : "Mixed")} wifi={(Wifi ? "on" : "off")} size={Size} volume={Volume} dialog={Dialog} menu={Menu} popup={(PopupOpen ? "open" : "closed")}");
    }

    public static Node Build()
    {
        var state = new State();
        var readout = new Text(state.Describe()).FontSize(13.0f).NodeId("readout");
        void Update() => readout.Text = state.Describe();

        var primary = new Button("Primary").NodeId("primary").OnClick(_ =>
        {
            state.Clicks++;
            Update();
        });
        var tinted = new Button("Tinted").Colors(new ButtonColors { Background = LayoutPage.Green, BackgroundHover = 0x15803DFF });
        var disabled = new Button("Disabled").Enabled(false);

        var agree = new Checkbox("Agree to terms").NodeId("agree").IsThreeState().OnChanged(e =>
        {
            state.Agree = e.IsChecked;
            Update();
        });
        var wifi = new Switch("Wi-Fi").NodeId("wifi").OnChanged(e =>
        {
            state.Wifi = e.IsChecked;
            Update();
        });
        var size = new RadioGroup().NodeId("size").Option("s", "Small").Option("m", "Medium").Option("l", "Large")
            .OnChanged(e =>
            {
                state.Size = e.SelectedValue ?? "none";
                Update();
            });
        size.FlexDirection = FlexDirection.Row;
        foreach (var radio in size.Radios)
        {
            radio.NodeId("size-" + radio.Value);
        }

        var progress = new ProgressBar(30.0f).Length(200.0f).NodeId("progress");
        var volume = new Slider(30.0f).Length(200.0f).NodeId("volume").OnChanged(e =>
        {
            state.Volume = e.Value;
            progress.Value = e.Value;
            Update();
        });

        var dialog = new Dialog("Save changes?", "Your edits are kept in this tab until you save them.")
            .OnAccepted(() =>
            {
                state.Dialog = "accepted";
                Update();
            })
            .OnCancelled(() =>
            {
                state.Dialog = "cancelled";
                Update();
            });
        dialog.AcceptButton.NodeId("dialog-ok");
        var openDialog = new Button("Open dialog").NodeId("open-dialog").OnClick(_ => dialog.Show());

        var popup = new Popup { BackdropColor = 0x0F172A20 }
            .OnOpened(() =>
            {
                state.PopupOpen = true;
                Update();
            })
            .OnClosed(() =>
            {
                state.PopupOpen = false;
                Update();
            });
        popup.Surface.NodeId("popup-surface").Padding(12.0f).Background(Theme.Active.Value.Colors.Surface).CornerRadius(8.0f);
        popup.Child(new Text("Popups hold any content.").FontSize(14.0f));
        var openPopup = new Button("Open popup").NodeId("open-popup");
        openPopup.OnClick(_ =>
        {
            var bounds = openPopup.GetBounds();
            popup.ShowAnchored(bounds.X, bounds.Y, bounds.Width, bounds.Height, 220.0f, 60.0f);
        });

        var menu = new ContextMenu().Items(
            new MenuItem("Rename", () => Choose("renamed")) { ShortcutLabel = "F2" },
            new MenuItem("Duplicate", () => Choose("duplicated")),
            MenuItem.Separator,
            new MenuItem("Delete", () => Choose("deleted")));
        void Choose(string action)
        {
            state.Menu = action;
            Update();
        }
        var tile = new FlexBox()
            .NodeId("tile")
            .Width(200.0f)
            .Height(48.0f)
            .Padding(12.0f)
            .CornerRadius(8.0f)
            .Background(LayoutPage.Grey)
            .Child(new Text("Right-click this tile").FontSize(14.0f))
            .OnContextMenuOpening(e => menu.Show(null, e.X, e.Y));

        var card = FlexBox.Column(
                new Text("Stage 3 controls").FontSize(24.0f).FontWeight(FontWeight.Bold).NodeId("gallery-title"),
                Gap(12.0f),
                FlexBox.Row(primary, Gap(12.0f), tinted, Gap(12.0f), disabled).AlignItems(AlignItems.Center),
                Gap(12.0f),
                FlexBox.Row(agree, Gap(16.0f), wifi).AlignItems(AlignItems.Center),
                Gap(8.0f),
                size,
                Gap(8.0f),
                FlexBox.Row(volume, Gap(12.0f), progress).AlignItems(AlignItems.Center),
                Gap(12.0f),
                FlexBox.Row(openPopup, Gap(12.0f), openDialog, Gap(12.0f), tile).AlignItems(AlignItems.Center),
                Gap(16.0f),
                readout)
            .NodeId("gallery-card")
            .Padding(20.0f);

        return FlexBox.Column(card, popup, dialog, menu).Padding(12.0f).AlignItems(AlignItems.Start);
    }

    private static FlexBox Gap(float size) => new FlexBox().Width(size).Height(size);
}
