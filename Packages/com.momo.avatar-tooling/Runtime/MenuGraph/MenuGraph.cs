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
        public VRCAvatarDescriptor AvatarDescriptor { get { return avatarDescriptor; } }

        // This is how to get the max number of VRCExpressionsMenu.Control there can be in any one array.
        //VRCExpressionsMenu.MAX_CONTROLS;

        public List<VRCExpressionParameters.Parameter> avatarParameters = new List<VRCExpressionParameters.Parameter>();
        public Dictionary<string, List<VRCExpressionsMenu.Control>> avatarMenus = new Dictionary<string, List<VRCExpressionsMenu.Control>>();

        private void Reset()
        {
            avatarParameters = new List<VRCExpressionParameters.Parameter>();
            avatarMenus = new Dictionary<string, List<VRCExpressionsMenu.Control>>();
        }
    }
}
