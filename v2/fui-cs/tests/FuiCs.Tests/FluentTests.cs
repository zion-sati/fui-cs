using System.Collections.Generic;
using System.Threading.Tasks;

using FuiCs.Bindings;

using TUnit.Assertions;
using TUnit.Core;

namespace FuiCs.Tests;

public sealed class FluentTests
{
    /// <summary>Redefines the setters subclasses override: a label that implies a role, enabled state and focus.</summary>
    private sealed class LabelNode : Node
    {
        public List<string> Log { get; } = [];

        public override string? SemanticLabel
        {
            get => base.SemanticLabel;
            set
            {
                if (!HasExplicitSemanticRole)
                {
                    DefaultSemanticRole = FuiCs.SemanticRole.StaticText;
                }
                base.SemanticLabel = value;
            }
        }

        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                Log.Add("enabled " + value);
                base.IsEnabled = value;
            }
        }

        public override void Focus()
        {
            Log.Add("focus");
            base.Focus();
        }

        public override ulong Build()
        {
            if (IsBuilt)
            {
                return Handle;
            }
            Handle = Ui.CreateNode(NodeType.Text);
            ApplyNodeMetadata();
            FinishBuild();
            return Handle;
        }
    }

    [Test]
    public async Task ChainsSetTheSameNamedPropertiesAndKeepTheConcreteType()
    {
        TestRuntime.Start();
        var clicks = 0;

        TestNode node = new TestNode()
            .NodeId("chained")
            .Cursor(CursorStyle.Pointer)
            .Enabled(false)
            .Visibility(Visibility.Hidden)
            .Focusable(true, 3)
            .SemanticLabel("Label")
            .SemanticSelected(true)
            .SemanticValueRange(1.0f, 0.0f, 2.0f)
            .AllowDrop()
            .OnPointerClick(_ => clicks++);

        await Assert.That(node.NodeId).IsEqualTo("chained");
        await Assert.That(node.Cursor).IsEqualTo(CursorStyle.Pointer);
        await Assert.That(node.IsEnabled).IsFalse();
        await Assert.That(node.Visibility).IsEqualTo(Visibility.Hidden);
        await Assert.That(node.Focusable).IsTrue();
        await Assert.That(node.TabIndex).IsEqualTo(3);
        await Assert.That(node.SemanticLabel).IsEqualTo("Label");
        await Assert.That(node.SemanticSelected == true).IsTrue();
        await Assert.That(node.SemanticValueRange == new SemanticValueRange(1.0f, 0.0f, 2.0f)).IsTrue();
        await Assert.That(node.AllowDrop).IsTrue();
    }

    [Test]
    public async Task FluentSettersReachSubclassOverrides()
    {
        var abi = TestRuntime.Start();

        var node = new LabelNode().SemanticLabel("Caption").Enabled(false);
        node.Build();
        node.Focus();

        await Assert.That(node.SemanticRole).IsEqualTo(SemanticRole.StaticText);
        await Assert.That(abi.Arguments("ui_set_semantic_role", 1)).IsEqualTo("StaticText");
        await Assert.That(string.Join(",", node.Log)).IsEqualTo("enabled False,focus");
    }

    [Test]
    public async Task FluentEventSubscriptionsAreMulticast()
    {
        var abi = TestRuntime.Start(new LayoutAbi());
        var log = new List<string>();
        var node = new TestNode()
            .OnPointerClick(_ => log.Add("first"))
            .OnPointerClick(_ => log.Add("second"));
        var handle = node.Build();
        abi.Bounds[handle] = new Rect(0.0f, 0.0f, 40.0f, 20.0f);

        HostInput.Click(handle, 10.0f, 10.0f);

        await Assert.That(string.Join(",", log)).IsEqualTo("first,second");
    }
}
