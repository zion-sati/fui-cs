namespace FuiCs;

public static class Smoke
{
    private const uint FontRegular = 1;
    private const uint PanelText = 0xE2E8F0FF;
    private const float Spacing = 32.0f;

    public static INode CreateSmokeTree()
    {
        return Fui.Column()
            .Padding(24.0f)
            .Child(
                Fui.Row()
                    .Child(
                        Fui.Text("left")
                            .Font(FontRegular, 28.0f)
                            .TextColor(PanelText))
                    .Child(
                        Fui.FlexBox()
                            .Width(Spacing)
                            .Height(1.0f))
                    .Child(
                        Fui.Text("right")
                            .Font(FontRegular, 28.0f)
                            .TextColor(PanelText)))
            .Child(Fui.FlexBox().Height(24.0f))
            .Child(
                Fui.FlexBox()
                    .Width(120.0f)
                    .Height(96.0f)
                    .BgColor(0x006CFFFF));
    }
}
