using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace MomoVRChatTools
{
    [RequireComponent(typeof(VRCAvatarDescriptor))]
    public class MenuGraph : MonoBehaviour, IEditorOnly
    {
        private VRCAvatarDescriptor avatarDescriptor;

        // Storing the Parameters and Menus here so they can be edited without touching the scriptableObjects
        // This is how to get the max number of VRCExpressionsMenu.Control there can be in any one array.
        //VRCExpressionsMenu.MAX_CONTROLS;
        [SerializeField] private List<VRCExpressionParameters.Parameter> avatarParameters = new List<VRCExpressionParameters.Parameter>();
        [SerializeField] private List<AvatarMenu> avatarMenus = new List<AvatarMenu>();
        public List<VRCExpressionParameters.Parameter> AvatarParameters {  get { return avatarParameters; } }
        public List<AvatarMenu> AvatarMenus {  get { return avatarMenus; } }

        private void Reset()
        {
            avatarParameters = new List<VRCExpressionParameters.Parameter>();
            avatarMenus = new List<AvatarMenu>();
        }
        public VRCAvatarDescriptor GetAvatarDescriptor()
        {
            if (avatarDescriptor != null) return avatarDescriptor;
            avatarDescriptor = GetComponent<VRCAvatarDescriptor>();
            return avatarDescriptor;
        }

        public AvatarMenu AddAvatarMenu(List<VRCExpressionsMenu.Control> controls, Rect position, string menuName = "New Menu", VRCExpressionsMenu realExpressionsMenu = null)
        {
            AvatarMenu menu = new AvatarMenu(position, realExpressionsMenu);
            menu.menuName = menuName;
            menu.controls = controls;

            avatarMenus.Add(menu);

            return menu;
        }
    }

    [Serializable]
    public class AvatarMenu
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
        public VRCExpressionsMenu RealExpressionsMenu { get {  return realExpressionsMenu; } }

        /// <summary>
        /// Crate a new avatarmenu item
        /// </summary>
        /// <param name="Position">The starting position and size of the graph item</param>
        public AvatarMenu(Rect Position, VRCExpressionsMenu realExpressionsMenu)
        {
            guid = Guid.NewGuid().ToString();
            this.Position = Position;
            this.realExpressionsMenu = realExpressionsMenu;
        }
    }
}
