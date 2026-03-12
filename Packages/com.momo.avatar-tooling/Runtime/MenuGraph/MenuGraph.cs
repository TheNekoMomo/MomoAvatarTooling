using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDKBase;

namespace MomoVRChatTools
{
    public class MenuGraph : MonoBehaviour, IEditorOnly
    {
        private VRCAvatarDescriptor avatarDescriptor;

        // Storing the Parameters and Menus here so they can be edited without touching the scriptableObjects
        // This is how to get the max number of VRCExpressionsMenu.Control there can be in any one array.
        //VRCExpressionsMenu.MAX_CONTROLS;
        private List<VRCExpressionParameters.Parameter> avatarParameters = new List<VRCExpressionParameters.Parameter>();
        private List<KeyValuePair<string, List<VRCExpressionsMenu.Control>>> avatarMenus = new List<KeyValuePair<string, List<VRCExpressionsMenu.Control>>>();
        public List<VRCExpressionParameters.Parameter> AvatarParameters {  get { return avatarParameters; } }
        public List<KeyValuePair<string, List<VRCExpressionsMenu.Control>>> AvatarMenus {  get { return avatarMenus; } }

        private void Reset()
        {
            avatarParameters = new List<VRCExpressionParameters.Parameter>();
            avatarMenus = new List<KeyValuePair<string, List<VRCExpressionsMenu.Control>>>();
        }
        public VRCAvatarDescriptor GetAvatarDescriptor()
        {
            if (avatarDescriptor != null) return avatarDescriptor;
            avatarDescriptor = GetComponent<VRCAvatarDescriptor>();
            return avatarDescriptor;
        }

        public void AddAvatarMenu(out string menuName, out List<VRCExpressionsMenu.Control> menuOptions)
        {
            menuName = "New Menu";
            menuOptions = new List<VRCExpressionsMenu.Control>();

            KeyValuePair<string, List<VRCExpressionsMenu.Control>> NewMenu = KeyValuePair.Create(menuName, menuOptions);
            avatarMenus.Add(NewMenu);
        }
    }
}
