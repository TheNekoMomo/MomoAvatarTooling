using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphView : GraphView
    {
        // Catch the EditorWindow and MenuGraph for later use
        private readonly EditorWindow window;
        private readonly MenuGraph menuGraph;
        // Pre set the node size so it is consistently set
        public static readonly Vector2 NODE_SIZE = new Vector2(200, 200);

        [SerializeField] private List<AvatarMenuEditorNode> graphNodes = new List<AvatarMenuEditorNode>();

        // Create the GraphView
        public MenuGraphView(EditorWindow window, MenuGraph menuGraph)
        {
            this.window = window;
            this.menuGraph = menuGraph;

            style.flexGrow = 1;
            this.StretchToParentSize();

            AddBackground();
            AddGraphManipulators();

            nodeCreationRequest += UserRequestedNewMenuNode;
            graphViewChanged += OnGraphViewChanged;
        }
        private void AddBackground()
        {
            GridBackground background = new GridBackground();
            background.StretchToParentSize();
            background.name = "Grid";
            Add(background);
            background.SendToBack();
        }
        private void AddGraphManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        private void UserRequestedNewMenuNode(NodeCreationContext context)
        {
            Vector2 mousePosition = context.screenMousePosition - window.position.position;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            AvatarMenuNode newMenu = menuGraph.AddAvatarMenu(new List<VRCExpressionsMenu.Control>(), new Rect(graphMousePosition, NODE_SIZE));
            AddNodeToGraph(newMenu);
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                MovedElements(graphViewChange);
            }
            if(graphViewChange.elementsToRemove != null)
            {
                RemovedElements(graphViewChange);
            }

            return graphViewChange;
        }
        private void MovedElements(GraphViewChange graphViewChange)
        {
            List<AvatarMenuEditorNode> nodes = graphViewChange.elementsToRemove.OfType<AvatarMenuEditorNode>().ToList();
            if (nodes.Count != 0)
            {
                Undo.RecordObject(menuGraph, "Moved menu node");

                for (int i = 0; i < nodes.Count; i++)
                {
                    string guid = nodes[i].name;

                    AvatarMenuNode avatarMenu = GetAvatarMenuNodeFromGUID(guid);
                    if (avatarMenu == null) continue;


                    avatarMenu.Position = nodes[i].GetPosition();
                }
            }
        }
        private void RemovedElements(GraphViewChange graphViewChange)
        {
            List<AvatarMenuEditorNode> nodes = graphViewChange.elementsToRemove.OfType<AvatarMenuEditorNode>().ToList();
            if(nodes.Count != 0)
            {
                Undo.RecordObject(menuGraph, "Removed menu nodes");

                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    string guid = nodes[i].name;
                    AvatarMenuNode avatarMenu = GetAvatarMenuNodeFromGUID(guid);
                    if(avatarMenu == null) continue;

                    menuGraph.AvatarMenus.Remove(avatarMenu);
                    graphNodes.Remove(nodes[i]);
                }
            }
        }
        private AvatarMenuNode GetAvatarMenuNodeFromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            AvatarMenuNode avatarMenu = menuGraph.AvatarMenus.First(menu => menu.GUID == guid);
            return avatarMenu;
        }

        public void AddNodeToGraph(AvatarMenuNode avatarMenu)
        {
            AvatarMenuEditorNode node = new AvatarMenuEditorNode(avatarMenu);

            AddElement(node);
            graphNodes.Add(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> allPorts = new List<Port>();
            List<Port> ports = new List<Port>();

            foreach (AvatarMenuEditorNode node in graphNodes)
            {
                allPorts.AddRange(node.Ports);
            }

            foreach (Port p in allPorts)
            {
                if (p == startPort) continue;
                if (p.node == startPort.node) continue;
                if (p.direction == startPort.direction) continue;
                if(p.portType == startPort.portType) ports.Add(p);
            }

            return ports;
        }
    }
}
