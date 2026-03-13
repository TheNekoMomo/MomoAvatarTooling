using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphEditorWindow : EditorWindow
    {
        // The uxml template the window is build off
        [SerializeField] private VisualTreeAsset uxmlAsset;

        [SerializeField] private MenuGraph currentMenuGraph;
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
        private void OnEnable()
        {
            DrawGUI();
        }

        private void load(MenuGraph target)
        {
            currentMenuGraph = target;

            DrawGUI();
        }
        private void DrawGUI()
        {
            if (currentMenuGraph == null) return;

            uxmlAsset.CloneTree(rootVisualElement);

            AssignToolBarButtons();
            AddGraph();

            PopulateGraph();
        }


        // Adding funtions to the uxml Template
        private void AssignToolBarButtons()
        {
            Toolbar toolbar = rootVisualElement.Q<Toolbar>();
            Button scanAvatar = toolbar.Q<Button>("ScanAvatar");
            scanAvatar.clicked += ScanAvatar;

            Button updateAvatar = toolbar.Q<Button>("UpdateAvatar");
            updateAvatar.clicked += UpdateAvatar;
        }
        private void AddGraph()
        {
            VisualElement mainArea = rootVisualElement.Q("MainArea");
            VisualElement GraphArea = mainArea.Q("GraphRoot");

            graphView = new MenuGraphView(this, currentMenuGraph);
            GraphArea.Add(graphView);
        }

        // Toolbar Buttons
        private void ScanAvatar()
        {
            if (currentMenuGraph == null)
            {
                EditorUtility.DisplayDialog("Menu Graph", "Something went wrong, could not find the Menu Graph component.", "Ok");
                return;
            }
            VRCAvatarDescriptor avatarDescriptor = currentMenuGraph.GetAvatarDescriptor();
            if (avatarDescriptor == null)
            {
                EditorUtility.DisplayDialog("Menu Graph", "Something went wrong, could not find the Avatar Descriptor.", "Ok");
                return;
            }
            if (avatarDescriptor.expressionsMenu == null)
            {
                EditorUtility.DisplayDialog("Menu Graph", "This avatar does not have any expression menus setup right now.", "Ok");
                return;
            }
            Debug.Log($"Scaning {currentMenuGraph.name} Avatar..");

            currentMenuGraph.AvatarMenus.Clear();
            SearchAvatarMenu(avatarDescriptor.expressionsMenu);
            PopulateGraph();
        }
        private void UpdateAvatar()
        {
            Debug.Log($"Updating {currentMenuGraph.name} Avatar..");
        }


        private void SearchAvatarMenu(VRCExpressionsMenu expressionsMenu)
        {
            // Menu Found
            string name = expressionsMenu.name;
            List<VRCExpressionsMenu.Control> menu = expressionsMenu.controls;

            currentMenuGraph.AddAvatarMenu(menu, new Rect(Vector2.zero, MenuGraphView.NODE_SIZE), realExpressionsMenu: expressionsMenu, menuName: name);

            // Check what this menu has in it
            for (int i = 0; i < menu.Count; i++)
            {
                switch (menu[i].type)
                {
                    case VRCExpressionsMenu.Control.ControlType.Button:
                        Debug.Log($"Found Button {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.Toggle:
                        Debug.Log($"Found Toggle {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.SubMenu:
                        // Found a Sub-Menu Check that
                        Debug.Log($"Found Submenu {menu[i].name}");
                        SearchAvatarMenu(menu[i].subMenu);
                        break;
                    case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                        Debug.Log($"Found TwoAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                        Debug.Log($"Found FourAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                        Debug.Log($"Found RadialPuppet {menu[i].name}");
                        break;
                    default:
                        Debug.LogError("Found unknow ControlType : " + menu[i].name);
                        break;
                }
            }
        }

        private void PopulateGraph()
        {
            if (currentMenuGraph == null) return;
            if (currentMenuGraph.AvatarMenus.Count == 0) return;

            foreach (AvatarMenu avatarMenu in currentMenuGraph.AvatarMenus)
            {
                graphView.AddNewNode(avatarMenu);
            }
        }
    }
}
