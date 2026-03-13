using UnityEditor;
using UnityEngine;

namespace MomoVRChatTools.Editor
{
    // Custom editor for MenuGraph
    [CustomEditor(typeof(MenuGraph))]
    public class MenuGraphEditor : UnityEditor.Editor
    {
        private const string BugReportUrl = "https://github.com/TheNekoMomo/MomoAvatarTooling/issues/new";

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

            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This code is still in alpha. If you find any bugs, please report them.", MessageType.Warning);
            if (GUILayout.Button("Report a Bug")) Application.OpenURL(BugReportUrl);
        }
    }
}
