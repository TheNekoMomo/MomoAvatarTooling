using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace MomoVRChatTools
{
    internal static class VRCFuryGestureHelperUtility
    {
        // Type names used by reflection so we do not need direct references
        // to VRCFury internal classes.
        private const string VrcfuryComponentTypeName = "VF.Model.VRCFury";
        private const string GestureDriverTypeName = "VF.Model.Feature.GestureDriver";
        private const string BlendShapeActionTypeName = "VF.Model.StateAction.BlendShapeAction";

        internal struct ApplySummary
        {
            public int gestureDriverComponentCount;
            public int changedGestureDriverComponentCount;
            public int changedBlendShapeActionCount;
        }

        /// <summary>
        /// Finds the avatar root for the helper by using transform.root.
        /// This matches your intended workflow where the helper can live on a subtree,
        /// while still resolving the descriptor from the avatar root.
        /// </summary>
        internal static GameObject GetAvatarRootObject(VRCFuryGestureHelper gestureHelper)
        {
            if (gestureHelper == null) return null;
            return gestureHelper.transform.root != null
                ? gestureHelper.transform.root.gameObject
                : gestureHelper.gameObject;
        }

        /// <summary>
        /// Finds the avatar descriptor on the avatar root.
        /// We do not expose this in the inspector. It is resolved automatically.
        /// </summary>
        internal static VRCAvatarDescriptor FindAvatarDescriptor(VRCFuryGestureHelper gestureHelper)
        {
            GameObject avatarRootObject = GetAvatarRootObject(gestureHelper);
            if (avatarRootObject == null) return null;

            return avatarRootObject.GetComponent<VRCAvatarDescriptor>();
        }

        /// <summary>
        /// Finds all VRCFury components under the helper's subtree that are specifically
        /// GestureDriver features.
        /// </summary>
        internal static List<Component> FindGestureDriverComponentsInScope(VRCFuryGestureHelper gestureHelper)
        {
            List<Component> gestureDriverComponents = new List<Component>();

            if (gestureHelper == null) return gestureDriverComponents;

            Component[] scopedComponents = gestureHelper.gameObject.GetComponentsInChildren<Component>(
                gestureHelper.includeInactiveChildren
            );

            foreach (Component scopedComponent in scopedComponents)
            {
                if (scopedComponent == null) continue;

                Type scopedComponentType = scopedComponent.GetType();
                if (scopedComponentType.FullName != VrcfuryComponentTypeName) continue;

                FieldInfo contentFieldInfo = scopedComponentType.GetField(
                    "content",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (contentFieldInfo == null) continue;

                object contentValue = contentFieldInfo.GetValue(scopedComponent);
                if (contentValue == null) continue;

                if (contentValue.GetType().FullName != GestureDriverTypeName) continue;

                gestureDriverComponents.Add(scopedComponent);
            }

            return gestureDriverComponents;
        }

        /// <summary>
        /// Applies the avatar descriptor's VisemeSkinnedMesh to all BlendShapeAction entries
        /// inside GestureDriver features that are inside the helper's subtree.
        /// </summary>
        internal static ApplySummary ApplyAvatarFaceMeshToScopedGestureDrivers(
            VRCFuryGestureHelper gestureHelper,
            bool recordUndo
        )
        {
            ApplySummary applySummary = new ApplySummary();

            if (gestureHelper == null) return applySummary;

            VRCAvatarDescriptor avatarDescriptor = FindAvatarDescriptor(gestureHelper);
            if (avatarDescriptor == null) return applySummary;

            SkinnedMeshRenderer avatarFaceMeshRenderer = avatarDescriptor.VisemeSkinnedMesh;
            if (avatarFaceMeshRenderer == null) return applySummary;

            List<Component> gestureDriverComponents = FindGestureDriverComponentsInScope(gestureHelper);
            applySummary.gestureDriverComponentCount = gestureDriverComponents.Count;

            foreach (Component gestureDriverComponent in gestureDriverComponents)
            {
                bool changedThisGestureDriver = ApplyAvatarFaceMeshToGestureDriverComponent(
                    gestureDriverComponent,
                    avatarFaceMeshRenderer,
                    ref applySummary.changedBlendShapeActionCount,
                    recordUndo
                );

                if (changedThisGestureDriver)
                {
                    applySummary.changedGestureDriverComponentCount++;
                }
            }

            return applySummary;
        }

        /// <summary>
        /// Applies the face mesh to one VRCFury GestureDriver component.
        /// Only BlendShapeAction entries are changed.
        /// </summary>
        private static bool ApplyAvatarFaceMeshToGestureDriverComponent(
            Component gestureDriverComponent,
            SkinnedMeshRenderer avatarFaceMeshRenderer,
            ref int changedBlendShapeActionCount,
            bool recordUndo
        )
        {
            if (gestureDriverComponent == null) return false;
            if (avatarFaceMeshRenderer == null) return false;

            Type gestureDriverUnityComponentType = gestureDriverComponent.GetType();

            FieldInfo contentFieldInfo = gestureDriverUnityComponentType.GetField(
                "content",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (contentFieldInfo == null) return false;

            object gestureDriverContentObject = contentFieldInfo.GetValue(gestureDriverComponent);
            if (gestureDriverContentObject == null) return false;

            if (gestureDriverContentObject.GetType().FullName != GestureDriverTypeName) return false;

            FieldInfo gesturesFieldInfo = gestureDriverContentObject.GetType().GetField(
                "gestures",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (gesturesFieldInfo == null) return false;

            object gesturesCollectionObject = gesturesFieldInfo.GetValue(gestureDriverContentObject);
            if (!(gesturesCollectionObject is IEnumerable gesturesCollection)) return false;

            bool changedAnythingInThisGestureDriver = false;

            if (recordUndo)
            {
                Undo.RecordObject(gestureDriverComponent, "Apply avatar descriptor face mesh to VRCFury gestures");
            }

            foreach (object gestureEntry in gesturesCollection)
            {
                if (gestureEntry == null) continue;

                FieldInfo stateFieldInfo = gestureEntry.GetType().GetField(
                    "state",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (stateFieldInfo == null) continue;

                object stateObject = stateFieldInfo.GetValue(gestureEntry);
                if (stateObject == null) continue;

                FieldInfo actionsFieldInfo = stateObject.GetType().GetField(
                    "actions",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (actionsFieldInfo == null) continue;

                object actionsCollectionObject = actionsFieldInfo.GetValue(stateObject);
                if (!(actionsCollectionObject is IEnumerable actionsCollection)) continue;

                foreach (object actionObject in actionsCollection)
                {
                    if (actionObject == null) continue;
                    if (actionObject.GetType().FullName != BlendShapeActionTypeName) continue;

                    Type blendShapeActionType = actionObject.GetType();

                    FieldInfo rendererFieldInfo = blendShapeActionType.GetField(
                        "renderer",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                    FieldInfo allRenderersFieldInfo = blendShapeActionType.GetField(
                        "allRenderers",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                    );

                    if (rendererFieldInfo == null || allRenderersFieldInfo == null) continue;

                    bool rendererIsAlreadyCorrect = Equals(
                        rendererFieldInfo.GetValue(actionObject),
                        avatarFaceMeshRenderer
                    );

                    bool allRenderersIsAlreadyFalse = Equals(
                        allRenderersFieldInfo.GetValue(actionObject),
                        false
                    );

                    if (rendererIsAlreadyCorrect && allRenderersIsAlreadyFalse) continue;

                    rendererFieldInfo.SetValue(actionObject, avatarFaceMeshRenderer);
                    allRenderersFieldInfo.SetValue(actionObject, false);

                    changedAnythingInThisGestureDriver = true;
                    changedBlendShapeActionCount++;
                }
            }

            return changedAnythingInThisGestureDriver;
        }
    }
}