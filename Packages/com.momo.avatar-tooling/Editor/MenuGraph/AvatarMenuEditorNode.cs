using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools.Editor
{
    public class AvatarMenuEditorNode : Node
    {
        private AvatarMenuNode avatarMenuNode;
        private List<Port> ports = new List<Port>();

        public AvatarMenuNode AvatarMenuNode {  get { return avatarMenuNode; } }
        public List<Port> Ports { get { return ports; } }

        public AvatarMenuEditorNode(AvatarMenuNode avatarMenuNode)
        {
            this.avatarMenuNode = avatarMenuNode;
            this.AddToClassList("menu-graph-node");

            title = avatarMenuNode.menuName;
            name = avatarMenuNode.GUID;
            SetPosition(avatarMenuNode.Position);

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(PortTypes.Menu));
            inputPort.name = "Input";
            inputContainer.Add(inputPort);
            ports.Add(inputPort);

            foreach (VRCExpressionsMenu.Control control in avatarMenuNode.controls)
            {
                if (control.type != VRCExpressionsMenu.Control.ControlType.SubMenu) continue;

                Port subMenuPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(PortTypes.Menu));
                subMenuPort.portName = control.name;
                outputContainer.Add(subMenuPort);
                ports.Add(subMenuPort);
            }
        }
    }

    public class PortTypes
    {
        public class Menu { }
    }
}
