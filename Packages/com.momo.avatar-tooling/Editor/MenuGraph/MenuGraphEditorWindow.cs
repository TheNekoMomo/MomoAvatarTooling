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

        private Dictionary<int, int> depthRowCounts = new();

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

            rootVisualElement.Clear();
            uxmlAsset.CloneTree(rootVisualElement);

            AssignToolBarButtons();
            AddGraph();
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

            depthRowCounts.Clear();

            currentMenuGraph.AvatarMenus.Clear();
            currentMenuGraph.Connections.Clear();

            SearchAvatarMenu(avatarDescriptor.expressionsMenu);

            graphView.PopulateGraph();
        }
        private void UpdateAvatar()
        {
            Debug.Log($"Updating {currentMenuGraph.name} Avatar..");
        }

        private void SearchAvatarMenu(VRCExpressionsMenu expressionsMenu, AvatarMenuNode parentMenu = null, int parentMenuIndex = 0, int depth = 0)
        {
            // Menu Found
            string name = expressionsMenu.name;
            List<VRCExpressionsMenu.Control> menu = expressionsMenu.controls;
            int submenuPortIndex = 1; // 0 is the input port on the graph node

            // Get the spacing for the node
            if (!depthRowCounts.ContainsKey(depth)) depthRowCounts[depth] = 0;

            float xSpacing = 250f;
            float ySpacing = 120f;

            int row = depthRowCounts[depth];
            depthRowCounts[depth]++;

            Rect position = new Rect(
                new Vector2(depth * xSpacing, row * ySpacing),
                MenuGraphView.NODE_SIZE
            );

            AvatarMenuNode createdAvatarMenu = currentMenuGraph.AddAvatarMenu
            (
                menu,
                position, 
                realExpressionsMenu: expressionsMenu, 
                menuName: name
            );

            if (parentMenu != null)
            {
                // Input = new menu
                // Output = parent menu
                MenuGraphConnectionPort input = new MenuGraphConnectionPort(createdAvatarMenu.GUID, 0);
                MenuGraphConnectionPort output = new MenuGraphConnectionPort(parentMenu.GUID, parentMenuIndex);
                currentMenuGraph.Connections.Add(new MenuGraphConnection(input, output));
            }

            // Check what this menu has in it
            for (int i = 0; i < menu.Count; i++)
            {
                switch (menu[i].type)
                {
                    case VRCExpressionsMenu.Control.ControlType.Button:
                        //Debug.Log($"Found Button {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.Toggle:
                        //Debug.Log($"Found Toggle {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.SubMenu:
                        //Debug.Log($"Found Submenu {menu[i].name}");
                        if(menu[i].subMenu != null)
                        {
                            SearchAvatarMenu(menu[i].subMenu, createdAvatarMenu, submenuPortIndex, depth + 1);
                            submenuPortIndex++;
                        }
                        break;
                    case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                        //Debug.Log($"Found TwoAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                        //Debug.Log($"Found FourAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                        //Debug.Log($"Found RadialPuppet {menu[i].name}");
                        break;
                    default:
                        //Debug.LogError("Found unknow ControlType : " + menu[i].name);
                        break;
                }
            }
        }

        
    }
}
