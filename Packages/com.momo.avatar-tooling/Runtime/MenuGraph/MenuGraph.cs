using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace MomoVRChatTools
{
    [RequireComponent(typeof(VRCAvatarDescriptor)), AddComponentMenu("MomoVRChatTools/MenuGraph")]
    public class MenuGraph : MonoBehaviour, IEditorOnly
    {
        private VRCAvatarDescriptor avatarDescriptor;

        // Storing the Parameters and Menus here so they can be edited without touching the scriptableObjects
        // This is how to get the max number of VRCExpressionsMenu.Control there can be in any one array.
        //VRCExpressionsMenu.MAX_CONTROLS;
        [SerializeField] private List<VRCExpressionParameters.Parameter> avatarParameters = new List<VRCExpressionParameters.Parameter>();
        [SerializeField] private List<AvatarMenuNode> avatarMenus = new List<AvatarMenuNode>();
        public List<VRCExpressionParameters.Parameter> AvatarParameters {  get { return avatarParameters; } }
        public List<AvatarMenuNode> AvatarMenus {  get { return avatarMenus; } }

        [SerializeField] private List<MenuGraphConnection> connections = new List<MenuGraphConnection>();
        public List<MenuGraphConnection> Connections { get { return connections; } }

        private void Reset()
        {
            avatarParameters = new List<VRCExpressionParameters.Parameter>();
            avatarMenus = new List<AvatarMenuNode>();
            connections = new List<MenuGraphConnection>();
        }
        public VRCAvatarDescriptor GetAvatarDescriptor()
        {
            if (avatarDescriptor != null) return avatarDescriptor;
            avatarDescriptor = GetComponent<VRCAvatarDescriptor>();
            return avatarDescriptor;
        }

        public AvatarMenuNode AddAvatarMenu(List<VRCExpressionsMenu.Control> controls, Rect position, string menuName = "New Menu", VRCExpressionsMenu realExpressionsMenu = null)
        {
            AvatarMenuNode menu = new AvatarMenuNode(position, realExpressionsMenu);
            menu.menuName = menuName;
            menu.controls = controls;

            avatarMenus.Add(menu);

            return menu;
        }
    }
}
