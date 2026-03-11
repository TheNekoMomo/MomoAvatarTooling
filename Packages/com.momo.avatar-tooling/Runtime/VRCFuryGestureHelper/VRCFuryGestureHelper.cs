using UnityEngine;
using VRC.SDKBase;

namespace MomoVRChatTools
{
    [DisallowMultipleComponent]
    public class VRCFuryGestureHelper : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Search this object and its children. This lets you limit the scan scope instead of always scanning the whole avatar root.")]
        public bool includeInactiveChildren = true;

        [Tooltip("If enabled, the helper will try to apply the avatar face mesh automatically during VRChat build/upload.")]
        public bool autoApplyOnBuild = true;

        /// <summary>
        /// Called by Unity when the component is first added or reset in the inspector.
        /// We do not need to store the avatar descriptor, but this is a good place
        /// to do sanity checks later if you want them.
        /// </summary>
        private void Reset()
        {
            // Intentionally left simple.
            // We resolve the avatar descriptor dynamically from the avatar root when needed.
        }
    }
}