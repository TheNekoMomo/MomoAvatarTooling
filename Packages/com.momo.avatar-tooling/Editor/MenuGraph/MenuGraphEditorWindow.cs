using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphEditorWindow : EditorWindow
    {
        private static VRCAvatarDescriptor selectedAvatar;

        private MenuGraphView graphView;

        [MenuItem("Tools/Momo Avatar Toolkit/Avatar Menu Graph")]
        private static void OpenWindow()
        {
            MenuGraphEditorWindow window = GetWindow<MenuGraphEditorWindow>("Avatar Menu Graph", true, typeof(SceneView));
            window.minSize = new Vector2(900, 500);
        }

        private void CreateGUI()
        {
            // Clare the Window
            rootVisualElement.Clear();

            Toolbar toolbar = new Toolbar();

            var avatarField = new ObjectField("Avatar Descriptor")
            {
                objectType = typeof(VRCAvatarDescriptor),
                value = selectedAvatar,
                allowSceneObjects = true
            };
            avatarField.RegisterValueChangedCallback(OnAvatarChanged);
            toolbar.Add(avatarField);

            Button scanButton = new Button(ScanSelectedAvatar) { text = "Scan Avatar" };
            toolbar.Add(scanButton);

            Button createButton = new Button() { text = "Create Menus" };
            toolbar.Add(createButton);

            rootVisualElement.Add(toolbar);

            graphView = new MenuGraphView();
            rootVisualElement.Add(graphView);
        }

        private void OnAvatarChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            selectedAvatar = evt.newValue as VRCAvatarDescriptor;
        }

        private void ScanSelectedAvatar()
        {
            if (selectedAvatar == null) return;
        }
    }
}
