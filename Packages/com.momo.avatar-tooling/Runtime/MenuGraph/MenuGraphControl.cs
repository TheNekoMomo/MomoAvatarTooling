using System;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class MenuGraphControl
    {
        [SerializeField] private string guid = "";
        public string GUID {  get { return guid; } }
        public string name = "Control Name";
        public Texture2D icon = null;
        public VRCExpressionsMenu.Control.ControlType type = VRCExpressionsMenu.Control.ControlType.Button;
        public string paramterName = "";
        public float value = 0;
        public AvatarMenuNode subMenu = null;

        public MenuGraphControl()
        {
            guid = Guid.NewGuid().ToString();
        }
    }
}
