using System.Collections.Generic;
using CleverCrow.Fluid.Dialogues.Actions;
using CleverCrow.Fluid.Dialogues.Conditions;

namespace CleverCrow.Fluid.Dialogues.Nodes {
    public class NodeLink : NodeBase {
        public override bool IsValid => _children[0]?.IsValid ?? false;

        public NodeLink (
            INode child,
            List<ICondition> conditions,
            List<IAction> enterActions,
            List<IAction> exitActions) :
            base(new List<INode>{child}, conditions, enterActions, exitActions) {
        }
    }
}
