using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace MomoVRChatTools
{
    public class SyncVRCParametersToAnimator : EditorWindow
    {
        private VRCExpressionParameters expressionParameters;
        private AnimatorController animatorController;

        List<string> animatorParamtersAdded = new List<string>();
        List<string> animatorParamtersNoLongerUsed = new List<string>();

        bool showAnimatorParamtersAdded = false;

        [MenuItem("Tools/Momo Avatar Toolkit/Sync VRChat Parameters to Animator", false, -20)]
        public static void ShowWindow()
        {
            GetWindow<SyncVRCParametersToAnimator>("Sync VRChat Params");
        }

        private void OnGUI()
        {
            GUILayout.Label("Sync VRC Parameters to Animator", EditorStyles.boldLabel);
            expressionParameters = (VRCExpressionParameters)EditorGUILayout.ObjectField("VRC Parameters Asset", expressionParameters, typeof(VRCExpressionParameters), false);
            animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);

            if (GUILayout.Button("Sync Parameters"))
            {
                if (expressionParameters == null || animatorController == null)
                {
                    EditorUtility.DisplayDialog("Error", "Please assign both VRC Parameters and Animator Controller.", "OK");
                    return;
                }

                AnimatorControllerParameter[] AnimatorParamters = animatorController.parameters;

                animatorParamtersAdded = new List<string>();
                animatorParamtersNoLongerUsed = new List<string>();

                foreach (VRCExpressionParameters.Parameter VRCParameter in expressionParameters.parameters)
                {
                    if (string.IsNullOrEmpty(VRCParameter.name))
                        continue;

                    if (System.Array.Exists(AnimatorParamters, p => p.name == VRCParameter.name))
                        continue; // Skip if already exists

                    AnimatorControllerParameterType type = AnimatorControllerParameterType.Bool;

                    switch (VRCParameter.valueType)
                    {
                        case VRCExpressionParameters.ValueType.Bool:
                            type = AnimatorControllerParameterType.Bool;
                            break;
                        case VRCExpressionParameters.ValueType.Int:
                            type = AnimatorControllerParameterType.Int;
                            break;
                        case VRCExpressionParameters.ValueType.Float:
                            type = AnimatorControllerParameterType.Float;
                            break;
                    }

                    animatorController.AddParameter(VRCParameter.name, type);
                    animatorParamtersAdded.Add(VRCParameter.name);
                }

                for (int i = 0; i < AnimatorParamters.Length; i++)
                {
                    if (string.IsNullOrEmpty(AnimatorParamters[i].name))
                        continue;

                    if (System.Array.Exists(expressionParameters.parameters, p => p.name != AnimatorParamters[i].name))
                    {
                        animatorParamtersNoLongerUsed.Add(AnimatorParamters[i].name);
                    }
                }

                EditorUtility.DisplayDialog("Success", "Parameters synced successfully!", "OK");
            }

            if (animatorParamtersAdded.Count > 0)
            {
                showAnimatorParamtersAdded = EditorGUILayout.Foldout(showAnimatorParamtersAdded, "VRC Parameters Added last sync");
                if (showAnimatorParamtersAdded)
                {
                    for (int i = 0; i < animatorParamtersAdded.Count; i++)
                    {
                        GUILayout.Label(animatorParamtersAdded[i]);
                    }
                }
            }
        }
    }
}