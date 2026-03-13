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

            AvatarMenu newMenu = menuGraph.AddAvatarMenu(new List<VRCExpressionsMenu.Control>(), new Rect(graphMousePosition, NODE_SIZE));
            AddNewNode(newMenu);
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
            foreach (GraphElement element in graphViewChange.movedElements)
            {
                Node node = element as Node;
                if (node == null) continue;

                string guid = node.name;
                if (string.IsNullOrEmpty(guid)) continue;

                AvatarMenu avatarMenu = menuGraph.AvatarMenus.First(menu => menu.GUID == guid);
                if (avatarMenu == null) continue;

                Undo.RecordObject(menuGraph, "Moved menu node");
                avatarMenu.Position = node.GetPosition();
            }
        }
        private void RemovedElements(GraphViewChange graphViewChange)
        {
            List<Node> nodes = graphViewChange.elementsToRemove.OfType<Node>().ToList();

            if(nodes.Count != 0) Undo.RecordObject(menuGraph, "Removed menu nodes");
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                string guid = nodes[i].name;
                if(string.IsNullOrEmpty(guid)) continue;

                AvatarMenu avatarMenu = menuGraph.AvatarMenus.First(menu => menu.GUID == guid);
                if (avatarMenu == null) continue;

                menuGraph.AvatarMenus.Remove(avatarMenu);
            }
        }

        public void AddNewNode(AvatarMenu avatarMenu)
        {
            Node node = new Node();

            node.title = avatarMenu.menuName;
            node.SetPosition(avatarMenu.Position);
            node.name = avatarMenu.GUID;

            Port inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PortTypes.Menu));
            inputPort.name = "Input";
            node.inputContainer.Add(inputPort);

            foreach (VRCExpressionsMenu.Control control in avatarMenu.controls)
            {
                if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu) continue;

                Port subMenuPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.Menu));
                subMenuPort.portName = control.name;
                node.outputContainer.Add(subMenuPort);
            }

            AddElement(node);
        }

        public class PortTypes
        {
            public class Menu { }
        }
    }
}
