using CleverCrow.Fluid.Dialogues.Nodes;
using UnityEditor;
using UnityEngine;

namespace CleverCrow.Fluid.Dialogues.Editors.NodeDisplays {
    [NodeType(typeof(NodeChoiceHubData))]
    public class ChoiceEditor : NodeEditorBase {
        private ChoiceCollection _choices;

        protected override Color NodeColor { get; } = new Color(0.33f, 0.75f, 0.73f);
        protected override float NodeWidth { get; } = 200;

        protected override void OnSetup () {
            _choices = new ChoiceCollection(this, Data as NodeChoiceHubData, Window);
        }

        protected override void OnPrintBody (Event e) {
            serializedObject.Update();

            _choices.Print(e);
            CreateChoice();

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateChoice () {
            if (GUILayout.Button("Add Choice", EditorStyles.miniButton, GUILayout.Width(80))) {
                _choices.Add();
            }
        }

        public override NodeDataBase CreateDataCopy () {
            return _choices.GetParentDataCopy();
        }

        protected override void OnDeleteCleanup () {
            _choices.DeleteAll();
        }
    }
}
