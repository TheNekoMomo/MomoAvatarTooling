using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace MomoVRChatTools.Editor
{
    // Custom editor for MenuGraph
    [CustomEditor(typeof(MenuGraph))]
    public class MenuGraphEditor : UnityEditor.Editor
    {
        // Override the Inspector GUI for MenuGraph
        public override void OnInspectorGUI()
        {
            // A little help box to say to click the button
            EditorGUILayout.HelpBox("Click the button to Open the Menu Graph Editor", MessageType.Info);
            // Check that the editor is not in playmode
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                // Add a button to open the graph editor when clicked
                if (GUILayout.Button("Open Menu Graph"))
                {
                    MenuGraphEditorWindow.Open(target as MenuGraph);
                }
            }
        }
    }
}
