using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset uxmlAsset;

        private MenuGraph currentMenuGraph;
        private MenuGraphView graphView;

        public static void Open(MenuGraph target)
        {
            MenuGraphEditorWindow[] windows = Resources.FindObjectsOfTypeAll<MenuGraphEditorWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i].currentMenuGraph == target)
                {
                    windows[i].Focus();
                    return;
                }
            }

            MenuGraphEditorWindow window = CreateWindow<MenuGraphEditorWindow>(typeof(MenuGraphEditorWindow), typeof(SceneView));
            window.titleContent = new GUIContent($"{target.name}", EditorGUIUtility.ObjectContent(null, typeof(MenuGraph)).image);
            window.load(target);
        }

        private void load(MenuGraph target)
        {
            currentMenuGraph = target;

            uxmlAsset.CloneTree(rootVisualElement);

            AssignToolBarButtons();
            AddGraph();
        }

        private void AssignToolBarButtons()
        {
            Toolbar toolbar = rootVisualElement.Q<Toolbar>();
            Button scanAvatar = toolbar.Q<Button>("ScanAvatar");
            scanAvatar.clicked += ScanAvatar;

            Button updateAvatar = toolbar.Q<Button>("UpdateAvatar");
            updateAvatar.clicked += UpdateAvatar;
        }
        private void ScanAvatar()
        {
            Debug.Log($"Scaning {currentMenuGraph.name} Avatar..");
        }
        private void UpdateAvatar()
        {
            Debug.Log($"Updating {currentMenuGraph.name} Avatar..");
        }

        private void AddGraph()
        {
            VisualElement mainArea = rootVisualElement.Q("MainArea");
            VisualElement GraphArea = mainArea.Q("GraphRoot");

            graphView = new MenuGraphView(this);
            GraphArea.Add(graphView);
        }
    }
}
