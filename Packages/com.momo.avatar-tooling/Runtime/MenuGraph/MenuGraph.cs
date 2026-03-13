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

        public AvatarMenu AddAvatarMenu(List<VRCExpressionsMenu.Control> controls, Rect position, string menuName = "New Menu")
        {
            AvatarMenu menu = new AvatarMenu(position);
            menu.menuName = menuName;
            menu.controls = controls;

            avatarMenus.Add(menu);

            return menu;
        }
    }

    [Serializable]
    public class AvatarMenu
    {
        [SerializeField] public string menuName;
        [SerializeField] public List<VRCExpressionsMenu.Control> controls;

        [SerializeField] public Rect Position;

        [SerializeField] private string guid;
        public string GUID
        {
            get
            {
                if (string.IsNullOrEmpty(guid)) guid = Guid.NewGuid().ToString();
                return guid;
            }
        }

        public AvatarMenu(Rect Position)
        {
            guid = Guid.NewGuid().ToString();
            this.Position = Position;
        }
    }
}
