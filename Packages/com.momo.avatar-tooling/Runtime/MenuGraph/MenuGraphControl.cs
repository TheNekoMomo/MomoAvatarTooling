using System;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class MenuGraphControl
    {
        public string name = "Control Name";
        public Texture2D icon = null;
        public VRCExpressionsMenu.Control.ControlType type = VRCExpressionsMenu.Control.ControlType.Button;
        public MenuGraphParamter paramter = null;
        public float value = 0;
        public AvatarMenuNode subMenu = null;
    }
}
