using System.Collections.Generic;
using System.Linq;
using CleverCrow.Fluid.Dialogues.Choices;
using CleverCrow.Fluid.Dialogues.Nodes;
using UnityEditor;
using UnityEngine;

namespace CleverCrow.Fluid.Dialogues.Editors.NodeDisplays {
    public class ChoiceCollection {
        private readonly DialogueWindow _window;
        private readonly NodeEditorBase _node;
        private readonly NodeDataChoiceBase _data;

        private readonly List<ChoiceData> _graveyard = new List<ChoiceData>();
        private readonly List<Connection> _connections = new List<Connection>();
        private readonly List<SerializedObject> _serializedObjects = new List<SerializedObject>();

        private bool IsChoiceMemoryLeak => _data.choices.Count != _connections.Count;

        public ChoiceCollection (NodeEditorBase node, NodeDataChoiceBase data, DialogueWindow window) {
            _window = window;
            _node = node;
            _data = data;

            RebuildChoices();
        }

        public void Add () {
            Undo.SetCurrentGroupName("Add choice");
            Undo.RecordObject(_data, "Add choice");

            var choice = ScriptableObject.CreateInstance<ChoiceData>();
            choice.name = "Choice";
            choice.Setup();
            _data.choices.Add(choice);

            AssetDatabase.AddObjectToAsset(choice, _data);
            AssetDatabase.SaveAssets();

            AddConnectionDisplay(choice);

            Undo.RegisterCreatedObjectUndo(choice, "Add choice");
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        public void Print (Event e) {
            MemoryLeakCleaner();
            PrintChoices(e);
            CleanGraveyard();
        }

        public void DeleteAll () {
            foreach (var choice in _data.choices.ToList()) {
                Undo.DestroyObjectImmediate(choice);
            }
        }

        public NodeDataBase GetParentDataCopy () {
            var copy = _data.GetCopy() as NodeDataChoiceBase;
            foreach (var choice in copy.choices) {
                choice.name = "Choice";
                choice.Setup();

                AssetDatabase.AddObjectToAsset(choice, _window.Graph);
                AssetDatabase.SaveAssets();
                Undo.RegisterCreatedObjectUndo(choice, "Duplicate choice");
            }

            return copy;
        }

        private void PrintChoices (Event e) {
            for (var i = 0; i < _data.choices.Count; i++) {
                GUILayout.BeginHorizontal();

                var choice = _data.choices[i];
                var connection = _connections[i];
                var serializedObject = _serializedObjects[i];

                serializedObject.Update();
                if (GUILayout.Button("Edit", EditorStyles.miniButton)) Selection.activeObject = choice;
                if (GUILayout.Button("-", EditorStyles.miniButton)) _graveyard.Add(choice);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("text"), GUIContent.none);
                serializedObject.ApplyModifiedProperties();

                GUILayout.EndHorizontal();

                // Only draw on repaint events to prevent crashing display position
                if (e.type == EventType.Repaint) {
                    var area = GUILayoutUtility.GetLastRect();
                    var pos = _node.ContentArea.position;
                    pos.x += _node.ContentArea.width;
                    pos.y += area.y;
                    connection.SetPosition(pos);
                }
            }
        }

        private void AddConnectionDisplay (ChoiceData choice) {
            _node.Out[0].Hide = true;
            _node.CreateConnection(ConnectionType.Out, choice);
            _connections.Add(_node.Out[_node.Out.Count - 1]);
            _serializedObjects.Add(new SerializedObject(choice));
        }

        private void RebuildChoices () {
            _node.Out[0].Hide = false;
            _connections.ForEach(_node.RemoveConnection);
            _connections.Clear();
            _serializedObjects.Clear();
            foreach (var choice in _data.choices) {
                AddConnectionDisplay(choice);
            }
        }

        private void MemoryLeakCleaner () {
            if (IsChoiceMemoryLeak) {
                RebuildChoices();
            }
        }

        private void DeleteChoice (ChoiceData choice) {
            var choiceIndex = _data.choices.IndexOf(choice);
            var connection = _connections[choiceIndex];
            var serializedObject = _serializedObjects[choiceIndex];

            Undo.SetCurrentGroupName($"Delete {choice.name}");
            Undo.RecordObject(_data, $"Delete {choice.name}");

            connection.Links.ClearAllLinks();
            _data.choices.Remove(choice);
            _node.RemoveConnection(connection);
            _connections.Remove(connection);
            _serializedObjects.Remove(serializedObject);

            Undo.DestroyObjectImmediate(choice);
            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        }

        private void CleanGraveyard () {
            if (_graveyard.Count <= 0) return;

            foreach (var choice in _graveyard) {
                DeleteChoice(choice);
            }

            _node.Out[0].Hide = _connections.Count != 0;
            _graveyard.Clear();
        }
    }
}
