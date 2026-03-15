using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class AvatarMenuNode
    {
        [SerializeField] private string guid;
        /// <summary>
        /// Get the GUID for this menu graph item
        /// </summary>
        public string GUID
        {
            get
            {
                // check that it still has a guid if not get it a new one.
                if (string.IsNullOrEmpty(guid)) guid = Guid.NewGuid().ToString();
                return guid;
            }
        }

        [SerializeField] public string menuName;
        [SerializeReference] public List<MenuGraphControl> controls = new List<MenuGraphControl>();
        [SerializeField] private VRCExpressionsMenu realExpressionsMenu;
        /// <summary>
        /// Get the VRCExpressionsMenu ScriptableObject that this AvatarMenu is representing
        /// </summary>
        public VRCExpressionsMenu RealExpressionsMenu { get { return realExpressionsMenu; } }

        [SerializeField] public Rect Position;

        /// <summary>
        /// Crate a new avatarmenu item
        /// </summary>
        /// <param name="Position">The starting position and size of the graph item</param>
        public AvatarMenuNode(Rect Position, VRCExpressionsMenu realExpressionsMenu)
        {
            guid = Guid.NewGuid().ToString();

            menuName = realExpressionsMenu.name;

            List<VRCExpressionsMenu.Control> VRCControls = realExpressionsMenu.controls;
            foreach (VRCExpressionsMenu.Control control in VRCControls)
            {
                MenuGraphControl menuGraphControl = new MenuGraphControl();
                menuGraphControl.name = control.name;
                menuGraphControl.icon = control.icon;
                menuGraphControl.type = control.type;
                menuGraphControl.paramter = new MenuGraphParamter();
                menuGraphControl.value = control.value;
                menuGraphControl.subMenu = null;

                controls.Add(menuGraphControl);
            }

            this.realExpressionsMenu = realExpressionsMenu;

            this.Position = Position;
        }
        public AvatarMenuNode(Rect Position, string menuName)
        {
            guid = Guid.NewGuid().ToString();

            this.menuName = menuName;
            realExpressionsMenu = null;

            this.Position = Position;
        }
    }
}
