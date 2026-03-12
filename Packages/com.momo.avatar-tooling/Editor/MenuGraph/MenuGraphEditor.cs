using UnityEditor;
using UnityEngine;

namespace MomoVRChatTools.Editor
{
    [CustomEditor(typeof(MenuGraph))]
    public class MenuGraphEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Click the button to Open the Menu Graph Editor", MessageType.Info);

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button("Open Menu Graph"))
                {
                    MenuGraphEditorWindow.Open(target as MenuGraph);
                }
            }
        }
    }
}
