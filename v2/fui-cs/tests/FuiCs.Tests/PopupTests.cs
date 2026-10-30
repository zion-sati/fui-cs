using System.Collections.Generic;
using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class PopupTests
{
    private static (LayoutAbi Abi, Portal Root, PopupPresenter Presenter) MountPresenter(float viewportWidth = 1280.0f, float viewportHeight = 720.0f)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = viewportWidth, ViewportHeight = viewportHeight });
        var root = new Portal();
        var presenter = new PopupPresenter(root, FlexBox.Column());
        Application.Mount(root);
        abi.Calls.Clear();
        return (abi, root, presenter);
    }

    private static (LayoutAbi Abi, Popup Popup) MountPopup(float viewportWidth = 1280.0f, float viewportHeight = 720.0f)
    {
        var abi = TestRuntime.Start(new LayoutAbi { ViewportWidth = viewportWidth, ViewportHeight = viewportHeight });
        var popup = new Popup();
        Application.Mount(new Portal().Child(popup));
        abi.Calls.Clear();
        return (abi, popup);
    }

    [Test]
    public async Task ShowAttachesTheOverlayAndPushesASemanticScope()
    {
        var (abi, root, presenter) = MountPresenter();
        var mark = abi.Created.Count;

        presenter.ShowAtPoint(24.0f, 32.0f, 120.0f, 80.0f);

        var created = abi.CreatedSince(mark);
        await Assert.That(created.Count).IsEqualTo(2);
        var (overlay, surface) = (created[0], created[1]);
        await Assert.That(presenter.IsOpen).IsTrue();
        await Assert.That(presenter.SurfaceX).IsEqualTo(24.0f);
        await Assert.That(presenter.SurfaceY).IsEqualTo(32.0f);
        await Assert.That(abi.HasCall("ui_node_add_child", root.Handle, overlay)).IsTrue();
        await Assert.That(abi.HasCall("ui_node_add_child", overlay, surface)).IsTrue();
        await Assert.That(abi.HasCall("ui_push_semantic_scope", surface)).IsTrue();
    }

    [Test]
    public async Task HideDetachesWithoutDeletingAndShowingAgainReusesTheNodes()
    {
        var (abi, root, presenter) = MountPresenter();
        var mark = abi.Created.Count;
        presenter.ShowAtPoint(12.0f, 16.0f, 120.0f, 80.0f);
        var created = abi.CreatedSince(mark);
        var (overlay, surface) = (created[0], created[1]);
        abi.Calls.Clear();

        presenter.Hide();

        await Assert.That(presenter.IsOpen).IsFalse();
        await Assert.That(abi.HasCall("ui_node_remove_child", root.Handle, overlay)).IsTrue();
        await Assert.That(abi.HasCall("ui_remove_semantic_scope", 1u)).IsTrue();
        await Assert.That(abi.CountOf("ui_delete_node")).IsEqualTo(0);

        mark = abi.Created.Count;
        abi.Calls.Clear();
        presenter.ShowAtPoint(20.0f, 24.0f, 120.0f, 80.0f);

        await Assert.That(presenter.IsOpen).IsTrue();
        await Assert.That(abi.CreatedSince(mark).Count).IsEqualTo(0);
        await Assert.That(abi.HasCall("ui_node_add_child", root.Handle, overlay)).IsTrue();
        await Assert.That(abi.HasCall("ui_push_semantic_scope", surface)).IsTrue();
    }

    [Test]
    public async Task PointsAndAnchoredPlacementsStayInsideTheViewport()
    {
        var (abi, _, presenter) = MountPresenter(320.0f, 220.0f);
        presenter.EdgePadding = 10.0f;
        presenter.AnchorGap = 6.0f;
        var mark = abi.Created.Count;

        presenter.ShowAtPoint(400.0f, 400.0f, 100.0f, 50.0f);
        var surface = abi.CreatedSince(mark)[1];

        await Assert.That(presenter.SurfaceX).IsEqualTo(210.0f);
        await Assert.That(presenter.SurfaceY).IsEqualTo(160.0f);
        await Assert.That(abi.HasCall("ui_set_position", surface, 210.0f, 160.0f)).IsTrue();

        presenter.ShowAnchored(40.0f, 170.0f, 80.0f, 30.0f, 100.0f, 80.0f, PopupPlacement.Auto);

        await Assert.That(presenter.SurfaceX).IsEqualTo(40.0f);
        await Assert.That(presenter.SurfaceY).IsEqualTo(84.0f);

        presenter.ShowAnchored(40.0f, 20.0f, 80.0f, 30.0f, 100.0f, 80.0f, PopupPlacement.Top);

        await Assert.That(presenter.SurfaceY).IsEqualTo(10.0f);
    }

    [Test]
    public async Task TheOverlayCoversTheViewportFromTheRootsOrigin()
    {
        var (abi, _, presenter) = MountPresenter(640.0f, 480.0f);
        var mark = abi.Created.Count;

        presenter.ShowAtPoint(20.0f, 20.0f, 100.0f, 80.0f);
        var overlay = abi.CreatedSince(mark)[0];

        await Assert.That(abi.CountOf("ui_get_bounds") > 0).IsTrue();
        await Assert.That(abi.HasCall("ui_set_position", overlay, -0.0f, -0.0f)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_width", overlay, 640.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_height", overlay, 480.0f, Unit.Pixel)).IsTrue();
    }

    [Test]
    public async Task DisposeHidesAndDeletesTheOverlayTree()
    {
        var (abi, _, presenter) = MountPresenter();
        var mark = abi.Created.Count;
        presenter.ShowAtPoint(10.0f, 10.0f, 100.0f, 60.0f);
        var created = abi.CreatedSince(mark);
        abi.Calls.Clear();

        presenter.Dispose();

        await Assert.That(presenter.IsOpen).IsFalse();
        await Assert.That(abi.HasCall("ui_remove_semantic_scope", 1u)).IsTrue();
        await Assert.That(abi.HasCall("ui_delete_node", created[0])).IsTrue();
        await Assert.That(abi.HasCall("ui_delete_node", created[1])).IsTrue();
    }

    [Test]
    public async Task APopupShowsHidesAndShowsAgainOnTheSameNodes()
    {
        var (abi, popup) = MountPopup();
        var mark = abi.Created.Count;

        popup.ShowAtPoint(18.0f, 24.0f, 160.0f, 96.0f);
        var created = abi.CreatedSince(mark);
        popup.Hide();
        var hidden = !popup.IsOpen;
        mark = abi.Created.Count;
        abi.Calls.Clear();
        popup.ShowAtPoint(20.0f, 28.0f, 160.0f, 96.0f);

        await Assert.That(created.Count).IsEqualTo(2);
        await Assert.That(hidden).IsTrue();
        await Assert.That(popup.IsOpen).IsTrue();
        await Assert.That(abi.CreatedSince(mark).Count).IsEqualTo(0);
        await Assert.That(abi.HasCall("ui_push_semantic_scope", popup.Surface.Handle)).IsTrue();
    }

    [Test]
    public async Task OpenedAndClosedFollowTheVisibleState()
    {
        var (abi, popup) = MountPopup();
        var log = new List<string>();
        popup.OnOpened(() => log.Add("opened")).OnClosed(() => log.Add("closed"));

        popup.ShowAtPoint(24.0f, 32.0f, 140.0f, 80.0f);
        popup.ShowAtPoint(40.0f, 32.0f, 140.0f, 80.0f);
        abi.Bounds[popup.Surface.Parent!.Handle] = new Rect(0.0f, 0.0f, 1280.0f, 720.0f);
        HostInput.Click(popup.Surface.Parent!.Handle, 600.0f, 400.0f);
        popup.Hide();

        await Assert.That(string.Join(",", log)).IsEqualTo("opened,closed");
        await Assert.That(popup.IsOpen).IsFalse();
    }

    [Test]
    public async Task ABackdropClickHidesThePopupOnlyWhenEnabled()
    {
        var (abi, popup) = MountPopup();
        popup.DismissOnBackdropClick = false;
        popup.ShowAtPoint(24.0f, 32.0f, 140.0f, 80.0f);
        var overlay = popup.Surface.Parent!.Handle;
        abi.Bounds[overlay] = new Rect(0.0f, 0.0f, 1280.0f, 720.0f);

        HostInput.Click(overlay, 400.0f, 300.0f);
        var openAfterDisabledClick = popup.IsOpen;
        popup.DismissOnBackdropClick = true;
        HostInput.Click(overlay, 400.0f, 300.0f);

        await Assert.That(openAfterDisabledClick).IsTrue();
        await Assert.That(popup.IsOpen).IsFalse();
    }

    [Test]
    public async Task ClicksOnThePanelBackgroundKeepThePopupOpen()
    {
        var (abi, popup) = MountPopup();
        popup.ShowAtPoint(24.0f, 32.0f, 140.0f, 80.0f);
        abi.Bounds[popup.Surface.Handle] = new Rect(24.0f, 32.0f, 140.0f, 80.0f);
        var overlay = popup.Surface.Parent!.Handle;
        abi.Bounds[overlay] = new Rect(0.0f, 0.0f, 1280.0f, 720.0f);

        HostInput.Click(overlay, 60.0f, 50.0f);
        var openAfterPanelClick = popup.IsOpen;
        HostInput.Click(overlay, 400.0f, 300.0f);

        await Assert.That(openAfterPanelClick).IsTrue();
        await Assert.That(popup.IsOpen).IsFalse();
    }

    [Test]
    public async Task PopupContentGoesOnTheSurface()
    {
        var (abi, popup) = MountPopup();
        var content = new TestNode();

        popup.Child(content);
        popup.ShowAtPoint(16.0f, 22.0f, 140.0f, 80.0f);

        await Assert.That(ReferenceEquals(content.Parent, popup.Surface)).IsTrue();
        await Assert.That(abi.HasCall("ui_node_add_child", popup.Surface.Handle, content.Handle)).IsTrue();
    }

    [Test]
    public async Task ShowingSizesTheSurfaceAndAppliesPlacement()
    {
        var (abi, popup) = MountPopup(320.0f, 220.0f);
        popup.EdgePadding(10.0f).AnchorGap(6.0f).Placement(PopupPlacement.Auto);

        popup.ShowAtPoint(400.0f, 400.0f, 100.0f, 50.0f);
        var surface = popup.Surface.Handle;

        await Assert.That(abi.HasCall("ui_set_width", surface, 100.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_height", surface, 50.0f, Unit.Pixel)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_position", surface, 210.0f, 160.0f)).IsTrue();

        abi.Calls.Clear();
        popup.ShowAnchored(40.0f, 170.0f, 80.0f, 30.0f, 100.0f, 80.0f);

        await Assert.That(abi.HasCall("ui_set_position", surface, 40.0f, 84.0f)).IsTrue();
    }

    [Test]
    public async Task PortalsAreFlaggedAndOptOutOfClipping()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var portal = new Portal();
        var root = FlexBox.Column(portal);

        root.Build();

        await Assert.That(abi.HasCall("ui_set_clip_to_bounds", root.Handle, false)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_clip_to_bounds", portal.Handle, false)).IsTrue();
        await Assert.That(abi.HasCall("ui_set_is_portal", portal.Handle, true)).IsTrue();
    }

    [Test]
    public async Task NegativeSpacingClampsToZeroWithAWarning()
    {
        var (abi, popup) = MountPopup();

        popup.EdgePadding = -4.0f;
        popup.AnchorGap = -1.0f;

        await Assert.That(popup.EdgePadding).IsEqualTo(0.0f);
        await Assert.That(popup.AnchorGap).IsEqualTo(0.0f);
        await Assert.That(abi.CountOf("fui_log")).IsEqualTo(2);
    }
}
