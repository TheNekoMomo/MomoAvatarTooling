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

        [SerializeField] private string currentMenuGraphGUID;
        private MenuGraph currentMenuGraph;
        private MenuGraphView graphView;

        private Dictionary<int, int> depthRowCounts = new();
        private int NumberOfMenusScaned = 0;
        private int NumberOfButtonsScaned = 0;
        private int NumberOfTogglesScaned = 0;
        private int NumberOfSubMenusScaned = 0;
        private int NumberOfTwoAxisPuppetsScaned = 0;
        private int NumberOfFourAxisPuppetsScaned = 0;
        private int NumberOfRadialPuppetsScaned = 0;

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
            EditorApplication.quitting += SaveUserGraphView;

            if (currentMenuGraph == null)
            {
                MenuGraph[] avatarsWithMenuGraph = FindObjectsOfType<MenuGraph>();
                foreach (MenuGraph avatar in avatarsWithMenuGraph)
                {
                    if (avatar.GUID != currentMenuGraphGUID) continue;
                    currentMenuGraph = avatar;
                    currentMenuGraph.OnMenuGraphReset = MenuGraphReset;
                    break;
                }
            }
            
            DrawGUI();
        }
        private void OnDestroy()
        {
            currentMenuGraph.OnMenuGraphReset -= MenuGraphReset;
            SaveUserGraphView();
        }
        private void SaveUserGraphView()
        {
            currentMenuGraph.LoadGraphView = true;
            currentMenuGraph.graphViewPosition = graphView.viewTransform.position;
            currentMenuGraph.graphViewScale = graphView.viewTransform.scale;
            currentMenuGraph.loadBlackBoard = true;
            currentMenuGraph.blackBoardPosition = graphView.blackBoard.GetPosition();
        }

        private void load(MenuGraph target)
        {
            currentMenuGraph = target;
            currentMenuGraphGUID = target.GUID;
            currentMenuGraph.OnMenuGraphReset = MenuGraphReset;

            DrawGUI();
        }
        private void DrawGUI()
        {
            if (currentMenuGraph == null) return;

            rootVisualElement.Clear();
            uxmlAsset.CloneTree(rootVisualElement);

            AssignToolBarButtons();
            AddGraph();

            if(currentMenuGraph.LoadGraphView)
                graphView.UpdateViewTransform(currentMenuGraph.graphViewPosition, currentMenuGraph.graphViewScale);
            if(currentMenuGraph.loadBlackBoard)
                graphView.blackBoard.SetPosition(currentMenuGraph.blackBoardPosition);
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

        private void MenuGraphReset()
        {
            currentMenuGraphGUID = currentMenuGraph.GUID;
            graphView.ClearGraph();
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

            NumberOfMenusScaned = 0;
            NumberOfButtonsScaned = 0;
            NumberOfTogglesScaned = 0;
            NumberOfSubMenusScaned = 0;
            NumberOfTwoAxisPuppetsScaned = 0;
            NumberOfFourAxisPuppetsScaned = 0;
            NumberOfRadialPuppetsScaned = 0;

            depthRowCounts.Clear();

            currentMenuGraph.AvatarMenus.Clear();
            currentMenuGraph.AvatarParamters.Clear();
            currentMenuGraph.Connections.Clear();

            SearchAvatarMenu(avatarDescriptor.expressionsMenu);
            VRCExpressionParameters.Parameter[] parameters = avatarDescriptor.expressionParameters.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                currentMenuGraph.AvatarParamters.Add(MenuGraphParamter.ConvertToMenuGraphParamters(parameters[i]));
            }

            graphView.PopulateGraph();

            string statsMessage =
                $@"Scan Complete

                Menus Scanned: {NumberOfMenusScaned}

                Buttons:            {NumberOfButtonsScaned}
                Toggles:            {NumberOfTogglesScaned}
                SubMenus:           {NumberOfSubMenusScaned}

                Two Axis Puppets:   {NumberOfTwoAxisPuppetsScaned}
                Four Axis Puppets:  {NumberOfFourAxisPuppetsScaned}
                Radial Puppets:     {NumberOfRadialPuppetsScaned}";
            EditorUtility.DisplayDialog("Menu Graph", statsMessage, "OK");
        }
        private void UpdateAvatar()
        {
            Debug.Log($"Updating {currentMenuGraph.name} Avatar..");
        }

        private void SearchAvatarMenu(VRCExpressionsMenu expressionsMenu, AvatarMenuNode parentMenu = null, int parentMenuIndex = 0, int depth = 0)
        {
            NumberOfMenusScaned++;

            // Menu Found
            List<VRCExpressionsMenu.Control> menu = expressionsMenu.controls;
            int submenuPortIndex = 1; // 0 is the input port on the graph node

            // Get the spacing for the node
            if (!depthRowCounts.ContainsKey(depth)) depthRowCounts[depth] = 0;

            float xSpacing = 250f;
            float ySpacing = 220f;

            int row = depthRowCounts[depth];
            depthRowCounts[depth]++;

            Rect position = new Rect(
                new Vector2(depth * xSpacing, row * ySpacing),
                MenuGraphView.NODE_SIZE
            );

            AvatarMenuNode createdAvatarMenu = currentMenuGraph.AddAvatarMenu(position, expressionsMenu);

            if (parentMenu != null)
            {
                // Input = new menu
                // Output = parent menu
                MenuGraphConnectionPort input = new MenuGraphConnectionPort(createdAvatarMenu.GUID, 0);
                MenuGraphConnectionPort output = new MenuGraphConnectionPort(parentMenu.GUID, parentMenuIndex);
                currentMenuGraph.Connections.Add(new MenuGraphConnection(input, output));

                parentMenu.controls[0].subMenu = createdAvatarMenu;
            }

            // Check what this menu has in it
            for (int i = 0; i < menu.Count; i++)
            {
                switch (menu[i].type)
                {
                    case VRCExpressionsMenu.Control.ControlType.Button:
                        NumberOfButtonsScaned++;
                        //Debug.Log($"Found Button {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.Toggle:
                        NumberOfTogglesScaned++;
                        //Debug.Log($"Found Toggle {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.SubMenu:
                        NumberOfSubMenusScaned++;
                        //Debug.Log($"Found Submenu {menu[i].name}");
                        if(menu[i].subMenu != null)
                        {
                            SearchAvatarMenu(menu[i].subMenu, createdAvatarMenu, submenuPortIndex, depth + 1);
                            submenuPortIndex++;
                        }
                        break;
                    case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                        NumberOfTwoAxisPuppetsScaned++;
                        //Debug.Log($"Found TwoAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                        NumberOfFourAxisPuppetsScaned++;
                        //Debug.Log($"Found FourAxisPuppet {menu[i].name}");
                        break;
                    case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                        NumberOfRadialPuppetsScaned++;
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
