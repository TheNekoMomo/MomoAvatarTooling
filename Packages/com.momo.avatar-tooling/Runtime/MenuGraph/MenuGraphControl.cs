using System;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class MenuGraphControl
    {
        public string name;
        public Texture2D icon;
        public VRCExpressionsMenu.Control.ControlType type;
        public MenuGraphParamter paramter;
        public float value;
        public AvatarMenuNode subMenu;
    }
}
