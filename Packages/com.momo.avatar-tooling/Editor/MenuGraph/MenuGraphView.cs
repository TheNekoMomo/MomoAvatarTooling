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

        public Blackboard blackBoard;

        // Create the GraphView
        public MenuGraphView(MenuGraphEditorWindow window, MenuGraph menuGraph)
        {
            this.window = window;
            this.menuGraph = menuGraph;

            style.flexGrow = 1;
            this.StretchToParentSize();

            AddBackground();
            AddGraphManipulators();

            AddBlackBoard();

            PopulateGraph();

            AddMiniMap();

            nodeCreationRequest += UserRequestedNewMenuNode;
            graphViewChanged = OnGraphViewChanged;
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
        private void AddBlackBoard()
        {
            blackBoard = new Blackboard(this);

            blackBoard.title = "Parameters";
            blackBoard.subTitle = "Parameters";

            blackBoard.addItemRequested = bb =>
            {
                MenuGraphParamter paramter = new MenuGraphParamter();
                menuGraph.AvatarParamters.Add(paramter);
                AddParameterToBlackboard(paramter);

                UpdateAllNodesDataRow();
            };
            blackBoard.editTextRequested += OnBlackboardEdit;

            blackBoard.scrollable = true;

            blackBoard.RegisterCallback<ValidateCommandEvent>(OnDeleteKey);

            Add(blackBoard);
        }

        private void OnDeleteKey(ValidateCommandEvent evt)
        {
            if (evt.commandName != "SoftDelete") return;

            BlackboardField blackboardField = selection.OfType<BlackboardField>().FirstOrDefault();
            if (blackboardField.userData is not MenuGraphParamter paramter) return;
            BlackboardRow blackboardRow = blackboardField.GetFirstAncestorOfType<BlackboardRow>();
            if (blackboardRow == null) return;

            RemoveParameter(paramter, blackboardRow);
        }

        private void AddParameterToBlackboard(MenuGraphParamter paramter)
        {
            // Create the BlackboardField
            BlackboardField field = new BlackboardField
            {
                text = paramter.name,
                typeText = paramter.valueType.ToString(),
                userData = paramter
            };
            // Create a new VisualElement to hold all of the data from the paramter
            VisualElement propertyView = new VisualElement();
            // Create a VisualElement to hold just the Default Value
            VisualElement ValueField = new VisualElement();
            // Add the Field for the current Default Value Type Int | Float | Bool
            AddValueField(paramter, ValueField);
            // Create and add a a EnumField for a drop down to change the Default Value Type
            EnumField typeField = new EnumField(paramter.valueType);
            typeField.RegisterValueChangedCallback(evt =>
            {
                // When the ValueType changes update the paramter to save it
                paramter.valueType = (VRCExpressionParameters.ValueType)evt.newValue;
                // Update the BlackboardField value type text
                field.typeText = paramter.valueType.ToString();
                // update the blackBoard subTitle as its based on the ValueType
                blackBoard.subTitle = blackBoard.subTitle = $"Total Parmater Cost: {menuGraph.AvatarParamters.CalculateTotalCostOfParamters()}";
                // Clare and then add back the ValueField
                ValueField.Clear();
                AddValueField(paramter, ValueField);
                UpdateAllNodesDataRow();
            });
            propertyView.Add(typeField);
            // Add the ValueField to the propertyView now so that it shows up after the ValueType drop down
            propertyView.Add(ValueField);
            // Create and add the toggles for if the value is Saved and or Network Synced
            Toggle savedField = new Toggle("Saved")
            {
                value = paramter.saved
            };
            savedField.RegisterValueChangedCallback(evt =>
            {
                paramter.saved = evt.newValue;
            });
            propertyView.Add(savedField);
            Toggle networkSyncedField = new Toggle("Network Synced")
            {
                value = paramter.networkSynced
            };
            networkSyncedField.RegisterValueChangedCallback(evt =>
            {
                paramter.networkSynced = evt.newValue;
            });
            propertyView.Add(networkSyncedField);
            // Create the BlackboardRow that holds the field and propertyView
            BlackboardRow row = new BlackboardRow(field, propertyView);
            // Add a Contextual menu drop down when right clicking on the BlackboardRow to remove the Parameter
            row.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                evt.menu.AppendAction("Remove", action => RemoveParameter(paramter, row), DropdownMenuAction.AlwaysEnabled);
            }));
            // Add the BlackboardRow to the blackBoard
            blackBoard.Add(row);
            // Set the blackBoard subTitle
            blackBoard.subTitle = $"Total Parmater Cost: {menuGraph.AvatarParamters.CalculateTotalCostOfParamters()}";
        }
        private static void AddValueField(MenuGraphParamter paramter, VisualElement propertyView)
        {
            switch (paramter.valueType)
            {
                case VRCExpressionParameters.ValueType.Int:
                    IntegerField intField = new IntegerField("Default Value")
                    {
                        value = (int)paramter.defaultValue
                    };
                    intField.RegisterValueChangedCallback(evt =>
                    {
                        paramter.defaultValue = evt.newValue;
                    });
                    propertyView.Add(intField);
                    break;
                case VRCExpressionParameters.ValueType.Float:
                    FloatField floatField = new FloatField("Default Value")
                    {
                        value = paramter.defaultValue
                    };
                    floatField.RegisterValueChangedCallback(evt =>
                    {
                        paramter.defaultValue = evt.newValue;
                    });
                    propertyView.Add(floatField);
                    break;
                case VRCExpressionParameters.ValueType.Bool:
                    Toggle boolField = new Toggle("Default Value")
                    {
                        value = paramter.defaultValue >= 1
                    };
                    boolField.RegisterValueChangedCallback(evt =>
                    {
                        paramter.defaultValue = evt.newValue ? 1 : 0;
                    });
                    propertyView.Add(boolField);
                    break;
                default:
                    Debug.LogError("Paramter Added with unknown, Type: " + paramter.valueType);
                    break;
            }
        }
        private void RemoveParameter(MenuGraphParamter paramter, BlackboardRow row)
        {
            row.RemoveFromHierarchy();
            menuGraph.AvatarParamters.Remove(paramter);

            UpdateAllNodesDataRow();
        }

        private void UpdateAllNodesDataRow()
        {
            foreach (AvatarMenuEditorNode node in graphNodes)
            {
                node.UpdateNodeFields();
            }

            RedrawConnections();
        }

        private void OnBlackboardEdit(Blackboard blackboard, VisualElement element, string newValue)
        {
            if(element is BlackboardField field && field.userData is MenuGraphParamter paramter)
            {
                if (string.IsNullOrEmpty(newValue)) newValue = "New Paramter";
                newValue = newValue.Trim();
                paramter.name = newValue;

                field.text = newValue;
                EditorUtility.SetDirty(menuGraph);
            }
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
            if (nodes.Count != 0)
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
            Debug.Log($"Edges to remove {edgesToRemove.Count}");
            if (edgesToRemove.Count != 0)
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
            Debug.Log($"EdgeCreated {graphViewChange.edgesToCreate.Count}");
            for (int i = 0; i < graphViewChange.edgesToCreate.Count; i++)
            {
                AvatarMenuEditorNode inputNode = (AvatarMenuEditorNode)graphViewChange.edgesToCreate[i].input.node;
                int inputIndex = inputNode.Ports.IndexOf(graphViewChange.edgesToCreate[i].input);

                AvatarMenuEditorNode outputNode = (AvatarMenuEditorNode)graphViewChange.edgesToCreate[i].output.node;
                int outputIndex = outputNode.Ports.IndexOf(graphViewChange.edgesToCreate[i].output);

                MenuGraphConnection connection = new MenuGraphConnection(inputNode.AvatarMenuNode.GUID, inputIndex, outputNode.AvatarMenuNode.GUID, outputIndex);

                menuGraph.Connections.Add(connection);
                editorConnectionDictionary.Add(graphViewChange.edgesToCreate[i], connection);
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

            foreach (MenuGraphParamter paramter in menuGraph.AvatarParamters)
            {
                AddParameterToBlackboard(paramter);
            }

            DrawConnections();
        }
        public void ClearGraph()
        {
            graphViewChanged -= OnGraphViewChanged;

            List<GraphElement> elementsToRemove = graphElements.ToList();
            DeleteElements(elementsToRemove);

            blackBoard.Clear();

            graphNodes.Clear();
            editorConnectionDictionary.Clear();

            graphViewChanged = OnGraphViewChanged;
        }
        private void RedrawConnections()
        {
            graphViewChanged -= OnGraphViewChanged;

            List<Edge> edges = new List<Edge>();
            for (int i = editorConnectionDictionary.Count - 1; i >= 0; i--)
            {
                KeyValuePair<Edge, MenuGraphConnection> connection = editorConnectionDictionary.ElementAt(i);
                edges.Add(connection.Key);
                editorConnectionDictionary.Remove(connection.Key);
            }
            DeleteElements(edges);

            DrawConnections();

            graphViewChanged = OnGraphViewChanged;
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
        public void RemoveEdge(Edge edge)
        {
            graphViewChanged -= OnGraphViewChanged;

            // Get the MenuGraphConnection data based on the edge
            if (editorConnectionDictionary.TryGetValue(edge, out MenuGraphConnection connectionToRemove))
            {
                // Remove the MenuGraphConnection from both the Dictionary and List
                menuGraph.Connections.Remove(connectionToRemove);
                editorConnectionDictionary.Remove(edge);
                // Loop over all the Connections in menuGraph and check if is the same node GUID for the output ports as the one to be removed
                // Then Check if the index is less then the index of the port being removed, if it is lower it by one
                for (int i = 0; i < menuGraph.Connections.Count; i++)
                {
                    if (menuGraph.Connections[i].outputPort.nodeGUID != connectionToRemove.outputPort.nodeGUID) continue;
                    if (menuGraph.Connections[i].outputPort.portIndex < connectionToRemove.outputPort.portIndex) continue;

                    menuGraph.Connections[i].outputPort.portIndex -= 1;
                    if (menuGraph.Connections[i].outputPort.portIndex > 0)
                    {
                        menuGraph.Connections[i].outputPort.portIndex = 0;
                    }
                }
            }
            if (edge.input != null) edge.input.Disconnect(edge);
            if (edge.output != null) edge.output.Disconnect(edge);

            RemoveElement(edge);

            graphViewChanged = OnGraphViewChanged;
        }
        private AvatarMenuEditorNode GetEditorNode(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            AvatarMenuEditorNode node = graphNodes.FirstOrDefault(editorNode => editorNode.AvatarMenuNode.GUID == guid);
            return node;
        }

        private void AddNodeToGraph(AvatarMenuNode avatarMenu)
        {
            AvatarMenuEditorNode node = new AvatarMenuEditorNode(avatarMenu, menuGraph, this);
            AddElement(node);
            graphNodes.Add(node);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> allPorts = new List<Port>();
            List<Port> compatiblePorts = new List<Port>();

            foreach (AvatarMenuEditorNode node in graphNodes)
            {
                allPorts.AddRange(node.Ports);
            }

            foreach (Port port in allPorts)
            {
                if (startPort.connections.Count() > 0) continue;
                if (port.connections.Count() > 0) continue;
                if (port == startPort) continue;
                if (port.node == startPort.node) continue;
                if (port.direction == startPort.direction) continue;
                if (port.portType == startPort.portType) compatiblePorts.Add(port);
            }

            return compatiblePorts;
        }
    }
}