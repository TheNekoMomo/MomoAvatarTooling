using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;
using static VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu;

namespace MomoVRChatTools.Editor
{
    public class AvatarMenuEditorNode : Node
    {
        private MenuGraph menuGraph;
        // Catched AvatarMenuNode for later use
        private AvatarMenuNode avatarMenuNode;
        // List of all ports on this node
        private List<Port> ports = new List<Port>();

        private Dictionary<string, Port> MenucontrolPortLookup = new Dictionary<string, Port>();

        /// <summary>
        /// Returns the AvatarMenuNode this node represents
        /// </summary>
        public AvatarMenuNode AvatarMenuNode {  get { return avatarMenuNode; } }
        /// <summary>
        /// Return a list of all ports on this node, 0 = the Input
        /// </summary>
        public List<Port> Ports { get { return ports; } }

        public AvatarMenuEditorNode(AvatarMenuNode avatarMenuNode, MenuGraph menuGraph)
        {
            this.menuGraph = menuGraph;
            // Catch the avatarMenuNode so it can be used later on
            this.avatarMenuNode = avatarMenuNode;

            // Crate a editable title
            TextField titleField = new TextField()
            {
                value = avatarMenuNode.menuName
            };
            titleField.RegisterValueChangedCallback(evt =>
            {
                avatarMenuNode.menuName = evt.newValue;
            });
            titleContainer.Add(titleField);

            // Set the name of the visual element so it can be found based on the GUID
            name = avatarMenuNode.GUID;
            // Set the start position of the node
            SetPosition(avatarMenuNode.Position);

            // Add a input port so that any menu can go to it
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PortTypes.Menu));
            inputPort.name = "Input";
            inputContainer.Add(inputPort);
            ports.Add(inputPort);

            UpdateNodeFields();
        }

        public void UpdateNodeFields()
        {
            extensionContainer.Clear();
            outputContainer.Clear();
            ports.RemoveAll(port => port.direction == Direction.Output);
            MenucontrolPortLookup.Clear();

            Button addButton = new Button(() =>
            {
                if (avatarMenuNode.controls.Count >= 8) return;
                avatarMenuNode.controls.Add(new MenuGraphControl());
                UpdateNodeFields();
            });
            addButton.text = "Add";
            addButton.tooltip = "Add a new field";
            extensionContainer.Add(addButton);

            for (int i = 0; i < avatarMenuNode.controls.Count; i++)
            {
                MenuGraphControl control = avatarMenuNode.controls[i];

                VisualElement row = new VisualElement();
                VisualElement nameRow = new VisualElement();
                VisualElement dataRow = new VisualElement();

                nameRow.style.flexDirection = FlexDirection.Row;
                dataRow.style.flexDirection = FlexDirection.Row;

                row.Add(nameRow);
                row.Add(dataRow);

                nameRow.Add(CreateNameField(control));
                nameRow.Add(CreateRemoveButton(control));

                CreateDataRow(control, dataRow);

                extensionContainer.Add(row);
                VisualElement space = new VisualElement();
                space.style.height = 10;
                extensionContainer.Add(space);
            }

            RefreshExpandedState();
        }

        private TextField CreateNameField(MenuGraphControl control)
        {
            TextField controlName = new TextField();
            controlName.value = control.name;
            controlName.style.minWidth = 80;
            controlName.style.flexGrow = 1;
            controlName.RegisterValueChangedCallback(evt =>
            {
                string newName = "New Control";
                newName = string.IsNullOrEmpty(evt.newValue) ? newName : evt.newValue;
                newName = newName.Trim();
                control.name = newName;

                if (control.type == Control.ControlType.SubMenu)
                {
                    Port submenuPort = MenucontrolPortLookup[control.GUID];
                    if (submenuPort == null)
                    {
                        Debug.LogError($"Could not find port for {control.GUID} control in lookup table");
                        return;
                    }
                    submenuPort.portName = newName;
                }
            });

            return controlName;
        }
        private Button CreateRemoveButton(MenuGraphControl control)
        {
            Button removeButton = new Button(() =>
            {
                avatarMenuNode.controls.Remove(control);
                UpdateNodeFields();
            });
            removeButton.text = "X";
            removeButton.style.width = 22;

            return removeButton;
        }

        // Will need to rebuild when a new Parameter is Added.. may be best to move the dataRow to a catched value
        private void CreateDataRow(MenuGraphControl control, VisualElement dataRow)
        {
            dataRow.Clear();

            List<string> controlTypeStringList = Enum.GetNames(typeof(Control.ControlType)).ToList();
            int controlTypeStringIndex = controlTypeStringList.IndexOf(control.type.ToString());

            PopupField<string> controlTypePopupField = new PopupField<string>(controlTypeStringList, controlTypeStringIndex);
            controlTypePopupField.style.width = 90;
            controlTypePopupField.RegisterValueChangedCallback(evt =>
            {
                if (!Enum.TryParse(evt.newValue, out Control.ControlType newControlType)) return;

                if (control.type == Control.ControlType.SubMenu)
                {
                    // Submenu removed
                    // need to remove the port off the lookup table
                    // need to rebuild all the edges
                    // need to remove all connections
                    // maybe use Port.DisconnectAll();
                    // still need to remove the connections from the menuGraph connections List
                    // I can get the Port from the lookup table and from that get the index in ports for that Port
                    // I know the node GUID
                    // Remove all connections for that list based on if the GUID matches and the port index match.. if both do then remove that connection
                }
                else if (newControlType == Control.ControlType.SubMenu)
                {
                    // Submenu made
                    // Already calling for the data row to be Created again.. so dont need to call for a port to be made
                    // what needs to be done when a new submenu is made other then rebuilding the data row? nothing?
                }
                control.type = newControlType;
                CreateDataRow(control, dataRow);
            });

            dataRow.Add(controlTypePopupField);

            if (control.type == Control.ControlType.SubMenu)
            {
                Port subMenuPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.Menu));
                subMenuPort.portName = control.name;
                outputContainer.Add(subMenuPort);
                ports.Add(subMenuPort);
                MenucontrolPortLookup.Add(control.GUID, subMenuPort);

                RefreshPorts();
            }
            else
            {
                VisualElement valueVisualElement = new VisualElement();

                List<string> parameterNames = GetParameterFieldOptions();
                int selectedParameterIndex = parameterNames.IndexOf(control.paramterName);

                PopupField<string> parameterField = new PopupField<string>(parameterNames, selectedParameterIndex);
                parameterField.style.width = 100;
                parameterField.RegisterValueChangedCallback(evt =>
                {
                    control.paramterName = evt.newValue;
                    CreateValueField(control, valueVisualElement);
                });
                parameterField.RegisterCallback<MouseOverEvent>(evt =>
                {
                    List<string> parameterNames = GetParameterFieldOptions();
                    parameterField.choices.Clear();
                    parameterField.choices.AddRange(parameterNames);

                    if (parameterNames.Contains(control.paramterName))
                    {
                        parameterField.value = control.paramterName;
                    }
                    else if (parameterNames.Count > 0)
                    {
                        parameterField.value = parameterNames[0];
                        control.paramterName = parameterNames[0];
                    }
                });
                dataRow.Add(parameterField);

                CreateValueField(control, valueVisualElement);

                dataRow.Add(valueVisualElement);
            }
        }
        private void CreateValueField(MenuGraphControl control, VisualElement valueVisualElement)
        {
            valueVisualElement.Clear();

            MenuGraphParamter menuGraphParamter = menuGraph.AvatarParamters.GetMenuGraphParamterByName(control.paramterName);
            if (menuGraphParamter != null)
            {
                switch (menuGraphParamter.valueType)
                {
                    case VRCExpressionParameters.ValueType.Int:
                        IntegerField intField = new IntegerField();
                        intField.value = (int)control.value;
                        intField.style.width = 60;
                        intField.RegisterValueChangedCallback(evt =>
                        {
                            control.value = Mathf.Clamp(evt.newValue, 0, 255);
                        });
                        valueVisualElement.Add(intField);
                        break;

                    case VRCExpressionParameters.ValueType.Float:
                        FloatField floatField = new FloatField();
                        floatField.value = control.value;
                        floatField.style.width = 60;
                        floatField.RegisterValueChangedCallback(evt =>
                        {
                            control.value = evt.newValue;
                        });
                        valueVisualElement.Add(floatField);
                        break;

                    case VRCExpressionParameters.ValueType.Bool:
                        Toggle toggle = new Toggle();
                        toggle.value = control.value >= 1;
                        toggle.RegisterValueChangedCallback(evt =>
                        {
                            control.value = evt.newValue ? 1 : 0;
                        });
                        valueVisualElement.Add(toggle);
                        break;

                    default:
                        Debug.LogError("Unknown Parameter ValueType");
                        break;
                }
            }
        }

        private List<string> GetParameterFieldOptions()
        {
            List<string> parameterNames = new List<string>();
            foreach (MenuGraphParamter paramter in menuGraph.AvatarParamters)
            {
                parameterNames.Add(paramter.name);
            }

            if (parameterNames.Count == 0) parameterNames.Add("[None]");

            return parameterNames;
        }

        // When the Node is selected in the graph trigger this event
        public override void OnSelected()
        {
            // Run the normal OnSelected code
            base.OnSelected();
            // Check if we have a scriptable object for this menu already made, if so set it to the active object selected.
            if (avatarMenuNode.RealExpressionsMenu != null)
            {
                Selection.activeObject = avatarMenuNode.RealExpressionsMenu;
            }
        }
    }

    // Port types used for the ports on a node
    public class PortTypes
    {
        public class Menu { }
    }
}