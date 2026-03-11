using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace MomoVRChatTools
{
    public class AnimatorTransitionSetter : EditorWindow
    {
        [MenuItem("Assets/Momo Avatar Toolkit/Set All Transitions")]
        private static void SetAllTransitions()
        {
            var controller = Selection.activeObject as AnimatorController;
            if (controller == null)
            {
                EditorUtility.DisplayDialog("No AnimatorController Selected", "Please select an Animator Controller asset in the Project view.", "OK");
                return;
            }

            Undo.RecordObject(controller, "Set All Transitions");

            foreach (var layer in controller.layers)
            {
                var stateMachine = layer.stateMachine;
                ProcessStateMachine(stateMachine);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("All transitions updated.");
        }

        private static void ProcessStateMachine(AnimatorStateMachine stateMachine)
        {
            foreach (var state in stateMachine.states)
            {
                foreach (var transition in state.state.transitions)
                {
                    transition.hasExitTime = false;
                    transition.duration = 0f;
                }
            }

            foreach (var anyTransition in stateMachine.anyStateTransitions)
            {
                anyTransition.hasExitTime = false;
                anyTransition.duration = 0f;
            }

            // Recursively handle sub-state machines
            foreach (var sm in stateMachine.stateMachines)
            {
                ProcessStateMachine(sm.stateMachine);
            }
        }
    }
}