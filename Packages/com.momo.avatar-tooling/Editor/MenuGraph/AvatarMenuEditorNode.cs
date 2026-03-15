using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools.Editor
{
    public class AvatarMenuEditorNode : Node
    {
        // Catched AvatarMenuNode for later use
        private AvatarMenuNode avatarMenuNode;
        // List of all ports on this node
        private List<Port> ports = new List<Port>();

        /// <summary>
        /// Returns the AvatarMenuNode this node represents
        /// </summary>
        public AvatarMenuNode AvatarMenuNode {  get { return avatarMenuNode; } }
        /// <summary>
        /// Return a list of all ports on this node, 0 = the Input
        /// </summary>
        public List<Port> Ports { get { return ports; } }

        public AvatarMenuEditorNode(AvatarMenuNode avatarMenuNode)
        {
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

            // Loop over all the controls so that can add a editable element for it
            foreach (var control in avatarMenuNode.controls)
            {
                switch (control.type)
                {
                    case VRCExpressionsMenu.Control.ControlType.Button:
                        break;

                    case VRCExpressionsMenu.Control.ControlType.Toggle:
                        break;

                    case VRCExpressionsMenu.Control.ControlType.SubMenu:
                        Port subMenuPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.Menu));
                        subMenuPort.portName = control.name;
                        outputContainer.Add(subMenuPort);
                        ports.Add(subMenuPort);
                        break;

                    case VRCExpressionsMenu.Control.ControlType.TwoAxisPuppet:
                        break;

                    case VRCExpressionsMenu.Control.ControlType.FourAxisPuppet:
                        break;

                    case VRCExpressionsMenu.Control.ControlType.RadialPuppet:
                        break;

                    default:
                        break;
                }
            }

            // Update the Node (safety)
            RefreshPorts();
            RefreshExpandedState();
        }

        private void AddIntField(VRCExpressionsMenu.Control control)
        {
            IntegerField valueField = new IntegerField(control.name);
            valueField.value = (int)control.value;
            valueField.AddToClassList("unity-base-field__aligned");

            valueField.RegisterValueChangedCallback(evt =>
            {
                control.value = Mathf.Clamp(evt.newValue, 0, 255);
            });
            extensionContainer.Add(valueField);
        }
        private void AddFloatField(VRCExpressionsMenu.Control control)
        {
            FloatField valueField = new FloatField(control.name);
            valueField.value = control.value;
            valueField.AddToClassList("unity-base-field__aligned");

            valueField.RegisterValueChangedCallback(evt =>
            {
                control.value = Mathf.Clamp(evt.newValue, -1, 1);
            });
            extensionContainer.Add(valueField);
        }
        private void AddBoolField(VRCExpressionsMenu.Control control)
        {
            Toggle valueField = new Toggle(control.name);
            valueField.value = control.value == 1;
            valueField.AddToClassList("unity-base-field__aligned");

            valueField.RegisterValueChangedCallback(evt =>
            {
                control.value = evt.newValue ? 1 : 0;
            });
            extensionContainer.Add(valueField);
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
