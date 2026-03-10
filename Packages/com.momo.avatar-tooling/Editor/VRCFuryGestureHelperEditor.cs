using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace MomoVRChatTools
{
    [CustomEditor(typeof(VRCFuryGestureHelper))]
    public class VRCFuryGestureHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            VRCFuryGestureHelper gestureHelper = (VRCFuryGestureHelper)target;

            EditorGUILayout.Space();

            VRCAvatarDescriptor avatarDescriptor = VRCFuryGestureHelperUtility.FindAvatarDescriptor(gestureHelper);
            List<Component> gestureDriverComponents = VRCFuryGestureHelperUtility.FindGestureDriverComponentsInScope(gestureHelper);

            string avatarDescriptorStatus = avatarDescriptor != null ? "Found" : "Not found";

            string faceMeshStatus = (avatarDescriptor != null && avatarDescriptor.VisemeSkinnedMesh != null) ? avatarDescriptor.VisemeSkinnedMesh.name : "Not found";

            if (gestureHelper.autoApplyOnBuild)
            {
                EditorGUILayout.HelpBox(
                    "On VRChat build/upload, this helper will automatically apply the avatar descriptor face mesh " +
                    "to VRCFury GestureDriver BlendShapeActions inside this object's subtree.",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "This helper is currently set to NOT apply the face mesh automatically to " +
                    "VRCFury GestureDrivers and you MUST click the Apply Now button for this to do anything.",
                    MessageType.Warning
                );
            }
            

            EditorGUILayout.LabelField("Avatar Descriptor", avatarDescriptorStatus);
            EditorGUILayout.LabelField("Face Mesh", faceMeshStatus);
            EditorGUILayout.LabelField("Gesture Drivers In Scope", gestureDriverComponents.Count.ToString());

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button("Apply Now"))
                {
                    ApplyNowWithWarningDialog(gestureHelper, avatarDescriptor, gestureDriverComponents);
                }
            }
        }

        private void ApplyNowWithWarningDialog(VRCFuryGestureHelper gestureHelper, VRCAvatarDescriptor avatarDescriptor, List<Component> gestureDriverComponents)
        {
            if (gestureHelper == null) return;

            if (avatarDescriptor == null)
            {
                EditorUtility.DisplayDialog(
                    "VRCFury Gesture Helper",
                    "No VRCAvatarDescriptor was found on the avatar root.",
                    "OK"
                );
                return;
            }

            if (avatarDescriptor.VisemeSkinnedMesh == null)
            {
                EditorUtility.DisplayDialog(
                    "VRCFury Gesture Helper",
                    "The avatar descriptor does not have a VisemeSkinnedMesh assigned.",
                    "OK"
                );
                return;
            }

            bool userConfirmedOverrideCreation = EditorUtility.DisplayDialog(
                "Apply Now?",
                "This will write the avatar's face mesh into the scoped VRCFury GestureDriver data immediately.\n\n" +
                "If the target object is a prefab instance, this will create prefab overrides, and VRCFury may warn about managed reference overrides.\n\n" +
                "Build/Upload auto-apply is safer for reusable prefabs.\n\n" +
                "Do you want to continue?",
                "Apply Now",
                "Cancel"
            );

            if (!userConfirmedOverrideCreation) return;

            ApplyNow(gestureHelper, avatarDescriptor, gestureDriverComponents);
        }

        private void ApplyNow(VRCFuryGestureHelper gestureHelper, VRCAvatarDescriptor avatarDescriptor, List<Component> gestureDriverComponents)
        {
            Undo.IncrementCurrentGroup();
            int undoGroupIndex = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Apply avatar descriptor face mesh to VRCFury gestures");

            VRCFuryGestureHelperUtility.ApplySummary applySummary =
                VRCFuryGestureHelperUtility.ApplyAvatarFaceMeshToScopedGestureDrivers(
                    gestureHelper,
                    recordUndo: true
                );

            foreach (Component gestureDriverComponent in gestureDriverComponents)
            {
                if (gestureDriverComponent == null) continue;

                EditorUtility.SetDirty(gestureDriverComponent);

                if (PrefabUtility.IsPartOfPrefabInstance(gestureDriverComponent))
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(gestureDriverComponent);
                }
            }

            EditorUtility.SetDirty(gestureHelper);
            EditorUtility.SetDirty(avatarDescriptor);

            if (gestureHelper.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gestureHelper.gameObject.scene);
            }

            Undo.CollapseUndoOperations(undoGroupIndex);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog(
                "VRCFury Gesture Helper",
                $"Updated {applySummary.changedBlendShapeActionCount} BlendShapeAction entr{(applySummary.changedBlendShapeActionCount == 1 ? "y" : "ies")} " +
                $"across {applySummary.changedGestureDriverComponentCount} GestureDriver component{(applySummary.changedGestureDriverComponentCount == 1 ? "" : "s")}.",
                "OK"
            );
        }
    }
}