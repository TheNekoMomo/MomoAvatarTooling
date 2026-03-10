using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace MomoVRChatTools
{
    internal class VRCFuryGestureHelperPreprocessor : IVRCSDKPreprocessAvatarCallback
    {
        // Run before VRCFury so the build avatar already has the face mesh assignments in place.
        public int callbackOrder => -10001;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (avatarGameObject == null) return true;

            VRCFuryGestureHelper[] gestureHelpers = avatarGameObject.GetComponentsInChildren<VRCFuryGestureHelper>(true);

            foreach (VRCFuryGestureHelper gestureHelper in gestureHelpers)
            {
                if (gestureHelper == null) continue;
                if (!gestureHelper.autoApplyOnBuild) continue;

                VRCFuryGestureHelperUtility.ApplyAvatarFaceMeshToScopedGestureDrivers(gestureHelper, recordUndo: false);
            }

            return true;
        }
    }
}