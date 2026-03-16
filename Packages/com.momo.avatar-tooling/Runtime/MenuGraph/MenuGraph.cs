using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace MomoVRChatTools
{
    [RequireComponent(typeof(VRCAvatarDescriptor)), AddComponentMenu("MomoVRChatTools/MenuGraph")]
    public class MenuGraph : MonoBehaviour, IEditorOnly
    {
        [SerializeField] private string guid = "";
        public string GUID {  get { return guid; } }

        // The avatars Descriptor
        private VRCAvatarDescriptor avatarDescriptor;

        // All of the menus on the avatar
        [SerializeField] private List<AvatarMenuNode> avatarMenus = new List<AvatarMenuNode>();
        public List<AvatarMenuNode> AvatarMenus {  get { return avatarMenus; } }

        // List of all the Paramters for the avatar
        [SerializeField] private List<MenuGraphParamter> avatarParamters = new List<MenuGraphParamter>();
        public List<MenuGraphParamter> AvatarParamters { get { return avatarParamters; } }

        // All the connections inside the nodes between the menus
        [SerializeField] private List<MenuGraphConnection> connections = new List<MenuGraphConnection>();
        public List<MenuGraphConnection> Connections { get { return connections; } }

        [SerializeField] public bool loadBlackBoard = false;
        [SerializeField] public Rect blackBoardPosition;
        [SerializeField] public bool LoadGraphView = false;
        [SerializeField] public Vector3 graphViewPosition;
        [SerializeField] public Vector3 graphViewScale;

        public Action OnMenuGraphReset = null;

        private void Reset()
        {
            //avatarParameters = new List<VRCExpressionParameters.Parameter>();
            avatarMenus = new List<AvatarMenuNode>();
            connections = new List<MenuGraphConnection>();

            guid = Guid.NewGuid().ToString();

            OnMenuGraphReset?.Invoke();
        }

        public VRCAvatarDescriptor GetAvatarDescriptor()
        {
            if (avatarDescriptor != null) return avatarDescriptor;
            avatarDescriptor = GetComponent<VRCAvatarDescriptor>();
            return avatarDescriptor;
        }

        public AvatarMenuNode AddAvatarMenu(Rect position, VRCExpressionsMenu realExpressionsMenu)
        {
            AvatarMenuNode menu = new AvatarMenuNode(position, realExpressionsMenu);
            avatarMenus.Add(menu);
            return menu;
        }
        public AvatarMenuNode AddAvatarMenu(Rect position, string menuName = "New Menu")
        {
            AvatarMenuNode menu = new AvatarMenuNode(position, menuName);
            avatarMenus.Add(menu);
            return menu;
        }
    }
}
