using System;
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
        private readonly MenuGraphEditorWindow window;
        private readonly MenuGraph menuGraph;
        // Pre set the node size so it is consistently set
        public static readonly Vector2 NODE_SIZE = new Vector2(200, 200);
        private readonly Vector2 miniMapSize = new Vector2(220, 160);

        [SerializeField] private List<AvatarMenuEditorNode> graphNodes = new List<AvatarMenuEditorNode>();
        [SerializeField] private Dictionary<Edge, MenuGraphConnection> editorConnectionDictionary = new Dictionary<Edge, MenuGraphConnection>();

        // Create the GraphView
        public MenuGraphView(MenuGraphEditorWindow window, MenuGraph menuGraph)
        {
            this.window = window;
            this.menuGraph = menuGraph;

            style.flexGrow = 1;
            this.StretchToParentSize();

            AddBackground();
            AddMiniMap();
            AddGraphManipulators();

            //var blackBoard = new Blackboard();
            //Add(blackBoard);

            PopulateGraph();

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
        private void AddMiniMap()
        {
            MiniMap miniMap = new MiniMap
            {
                anchored = true,
                windowed = false
            };

            miniMap.SetPosition(new Rect(10, 30, miniMapSize.x, miniMapSize.y));
            Add(miniMap);

            RegisterCallback<GeometryChangedEvent>(evt =>
            {
                float width = miniMapSize.x;
                float height = miniMapSize.y;

                miniMap.SetPosition(new Rect(
                    10,
                    contentRect.height - height - 10,
                    width,
                    height));
            });
        }
        private void AddGraphManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            this.AddManipulator(new ContentZoomer());
        }

        // GraphView Delegates
        private void UserRequestedNewMenuNode(NodeCreationContext context)
        {
            Vector2 mousePosition = context.screenMousePosition - window.position.position;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            AvatarMenuNode newMenu = menuGraph.AddAvatarMenu(new Rect(graphMousePosition, NODE_SIZE));
            AddNodeToGraph(newMenu);
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                MovedNode(graphViewChange);
            }
            if(graphViewChange.elementsToRemove != null)
            {
                RemovedNode(graphViewChange);
                RemoveConnection(graphViewChange);
            }
            if(graphViewChange.edgesToCreate != null)
            {
                EdgeCreated(graphViewChange);
            }

            return graphViewChange;
        }

        // Used when the GraphView has an update
        private void MovedNode(GraphViewChange graphViewChange)
        {
            List<AvatarMenuEditorNode> nodes = graphViewChange.movedElements.OfType<AvatarMenuEditorNode>().ToList();
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
        private void RemovedNode(GraphViewChange graphViewChange)
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
        private void RemoveConnection(GraphViewChange graphViewChange)
        {
            List<Edge> edgesToRemove = graphViewChange.elementsToRemove.OfType<Edge>().ToList();
            if(edgesToRemove.Count != 0)
            {
                Undo.RecordObject(menuGraph, "Removed Connection");

                for (int i = edgesToRemove.Count - 1; i >= 0; i--)
                {
                    if (editorConnectionDictionary.TryGetValue(edgesToRemove[i], out MenuGraphConnection connection))
                    {
                        menuGraph.Connections.Remove(connection);
                        editorConnectionDictionary.Remove(edgesToRemove[i]);
                    }
                }
            }
        }
        private void EdgeCreated(GraphViewChange graphViewChange)
        {
            Undo.RecordObject(menuGraph, "Added Connections");
            foreach (Edge edge in graphViewChange.edgesToCreate)
            {
                AvatarMenuEditorNode inputNode = (AvatarMenuEditorNode)edge.input.node;
                int inputIndex = inputNode.Ports.IndexOf(edge.input);

                AvatarMenuEditorNode outputNode = (AvatarMenuEditorNode)edge.output.node;
                int outputIndex = outputNode.Ports.IndexOf(edge.output);

                MenuGraphConnection connection = new MenuGraphConnection(inputNode.AvatarMenuNode.GUID, inputIndex, outputNode.AvatarMenuNode.GUID, outputIndex);
                menuGraph.Connections.Add(connection);
            }
        }
        private AvatarMenuNode GetAvatarMenuNodeFromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            AvatarMenuNode avatarMenu = menuGraph.AvatarMenus.FirstOrDefault(menu => menu.GUID == guid);
            return avatarMenu;
        }

        public void PopulateGraph()
        {
            if (menuGraph.AvatarMenus.Count == 0) return;

            ClearGraph();

            foreach (AvatarMenuNode avatarMenu in menuGraph.AvatarMenus)
            {
                AddNodeToGraph(avatarMenu);
            }

            DrawConnections();
        }
        public void ClearGraph()
        {
            graphViewChanged -= OnGraphViewChanged;

            List<GraphElement> elementsToRemove = graphElements.ToList();
            DeleteElements(elementsToRemove);

            graphNodes.Clear();
            editorConnectionDictionary.Clear();

            graphViewChanged += OnGraphViewChanged;
        }

        private void DrawConnections()
        {
            if(menuGraph.Connections.Count == 0) return;
            foreach (MenuGraphConnection connection in menuGraph.Connections)
            {
                AvatarMenuEditorNode inputNode = GetEditorNode(connection.inputPort.nodeGUID);
                AvatarMenuEditorNode outputNode = GetEditorNode(connection.outputPort.nodeGUID);

                if (inputNode == null || outputNode == null) continue;

                if (connection.inputPort.portIndex < 0 || connection.inputPort.portIndex >= inputNode.Ports.Count) continue;
                if (connection.outputPort.portIndex < 0 || connection.outputPort.portIndex >= outputNode.Ports.Count) continue;

                Port inputPort = inputNode.Ports[connection.inputPort.portIndex];
                Port outputPort = outputNode.Ports[connection.outputPort.portIndex];

                if (inputPort.direction != Direction.Input) continue;
                if (outputPort.direction != Direction.Output) continue;

                Edge edge = outputPort.ConnectTo(inputPort);

                AddElement(edge);
                editorConnectionDictionary.Add(edge, connection);
            }
        }
        private AvatarMenuEditorNode GetEditorNode(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            AvatarMenuEditorNode node = graphNodes.FirstOrDefault(editorNode => editorNode.AvatarMenuNode.GUID == guid);
            return node;
        }

        private void AddNodeToGraph(AvatarMenuNode avatarMenu)
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
                if (p.portType == startPort.portType) ports.Add(p);
            }

            return ports;
        }
    }
}
