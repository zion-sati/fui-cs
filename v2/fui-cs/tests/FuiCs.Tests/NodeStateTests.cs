using System.Threading.Tasks;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class NodeStateTests
{
    [Test]
    public async Task DisablingAParentDisablesDescendantsUntilItIsEnabledAgain()
    {
        var abi = TestRuntime.Start();
        var parent = new TestNode();
        var child = new TestNode();
        child.MakeInteractive();
        parent.AddChild(child);
        parent.Build();
        abi.Calls.Clear();

        parent.IsEnabled = false;

        await Assert.That(child.IsEnabled).IsFalse();
        await Assert.That(abi.Arguments("ui_set_interactive", 0)).IsEqualTo("2");
        await Assert.That(abi.Arguments("ui_set_interactive", 1)).IsEqualTo("False");

        parent.IsEnabled = true;

        await Assert.That(child.IsEnabled).IsTrue();
        await Assert.That(abi.Arguments("ui_set_interactive", 1)).IsEqualTo("False,True");
        await Assert.That(string.Join(",", child.EnabledChanges)).IsEqualTo("False,True");
    }

    [Test]
    public async Task AChildRemovedFromADisabledParentIsEnabledAgain()
    {
        TestRuntime.Start();
        var parent = new TestNode { IsEnabled = false };
        var child = new TestNode();

        parent.AddChild(child);
        var whileAttached = child.IsEnabled;
        parent.RemoveChild(child);

        await Assert.That(whileAttached).IsFalse();
        await Assert.That(child.IsEnabled).IsTrue();
    }

    [Test]
    public async Task CollapsingAParentHidesDescendantsWithoutChangingTheirOwnVisibility()
    {
        var abi = TestRuntime.Start();
        var parent = new TestNode();
        var child = new TestNode();
        parent.AddChild(child);
        parent.Build();
        abi.Calls.Clear();

        parent.Visibility = Visibility.Collapsed;

        await Assert.That(child.IsVisible).IsFalse();
        await Assert.That(child.Visibility).IsEqualTo(Visibility.Normal);
        await Assert.That(abi.Arguments("ui_set_visibility", 0)).IsEqualTo("1,2");
        await Assert.That(string.Join(",", child.VisibilityChanges)).IsEqualTo("Collapsed");
    }

    [Test]
    public async Task FocusableNodesAreFocusableOnlyWhileEnabled()
    {
        var abi = TestRuntime.Start();
        var node = new TestNode { Focusable = true, TabIndex = 2 };
        node.Build();

        await Assert.That(abi.Arguments("ui_set_focusable", 1)).IsEqualTo("True");
        await Assert.That(abi.Arguments("ui_set_focusable", 2)).IsEqualTo("2");

        abi.Calls.Clear();
        node.IsEnabled = false;

        await Assert.That(abi.Arguments("ui_set_focusable", 1)).IsEqualTo("False");
    }

    [Test]
    public async Task DefaultSemanticsApplyUntilExplicitValuesAreSet()
    {
        var abi = TestRuntime.Start();
        var node = new TestNode();
        node.SetDefaultRole(SemanticRole.Button);
        node.SetDefaultLabel("Default");
        node.Build();

        await Assert.That(abi.Arguments("ui_set_semantic_role", 1)).IsEqualTo("Button");
        await Assert.That(abi.Arguments("ui_set_semantic_label", 1)).IsEqualTo("Default");

        abi.Calls.Clear();
        node.SemanticRole = SemanticRole.Link;
        node.SetDefaultRole(SemanticRole.Checkbox);

        await Assert.That(node.SemanticRole).IsEqualTo(SemanticRole.Link);
        await Assert.That(abi.Arguments("ui_set_semantic_role", 1)).IsEqualTo("Link");
    }

    [Test]
    public async Task NullableSemanticStatesSendWhetherTheyAreSet()
    {
        var abi = TestRuntime.Start();
        var node = new TestNode();
        node.Build();
        abi.Calls.Clear();

        node.SemanticValueRange = new SemanticValueRange(5.0f, 0.0f, 10.0f);
        node.SemanticValueRange = null;
        node.SemanticSelected = true;
        node.SemanticSelected = null;

        await Assert.That(abi.Arguments("ui_set_semantic_value_range", 1)).IsEqualTo("True,False");
        await Assert.That(abi.Arguments("ui_set_semantic_value_range", 2)).IsEqualTo("5,0");
        await Assert.That(abi.Arguments("ui_set_semantic_selected", 1)).IsEqualTo("True,False");
        await Assert.That(abi.Arguments("ui_set_semantic_selected", 2)).IsEqualTo("True,False");
    }

    [Test]
    public async Task SemanticDisabledCanTrackEnabledState()
    {
        var abi = TestRuntime.Start();
        var node = new TestNode();
        node.TrackSemanticDisabled();
        node.Build();
        abi.Calls.Clear();

        node.IsEnabled = false;

        await Assert.That(node.SemanticDisabled == true).IsTrue();
        await Assert.That(abi.Arguments("ui_set_semantic_disabled", 2)).IsEqualTo("True");

        node.SemanticDisabled = null;
        abi.Calls.Clear();
        node.IsEnabled = true;

        await Assert.That(abi.CountOf("ui_set_semantic_disabled")).IsEqualTo(0);
    }
}
