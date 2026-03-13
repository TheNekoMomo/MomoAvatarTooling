using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    [Serializable]
    public class AvatarMenuNode
    {
        //ID
        [SerializeField] private string guid;

        // Menu itself
        [SerializeField] public string menuName;
        [SerializeField] public List<VRCExpressionsMenu.Control> controls;
        [SerializeField] private VRCExpressionsMenu realExpressionsMenu;

        // Graph
        [SerializeField] public Rect Position;

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
        /// <summary>
        /// Get the VRCExpressionsMenu ScriptableObject that this AvatarMenu is representing
        /// </summary>
        public VRCExpressionsMenu RealExpressionsMenu { get { return realExpressionsMenu; } }

        /// <summary>
        /// Crate a new avatarmenu item
        /// </summary>
        /// <param name="Position">The starting position and size of the graph item</param>
        public AvatarMenuNode(Rect Position, VRCExpressionsMenu realExpressionsMenu)
        {
            guid = Guid.NewGuid().ToString();
            this.Position = Position;
            this.realExpressionsMenu = realExpressionsMenu;
        }
    }
}
