using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphView : GraphView
    {
        private readonly EditorWindow window;

        public MenuGraphView(EditorWindow window)
        {
            this.window = window;

            style.flexGrow = 1;
            this.StretchToParentSize();

            AddBackground();
            AddGraphManipulators();

            nodeCreationRequest = CreateNewMenuNode;
        }

        private void AddGraphManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }

        private void AddBackground()
        {
            GridBackground background = new GridBackground();
            background.StretchToParentSize();
            background.name = "Grid";
            Add(background);
            background.SendToBack();
        }

        private void CreateNewMenuNode(NodeCreationContext context)
        {
            Vector2 mousePosition = context.screenMousePosition - window.position.position;
            Vector2 graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);

            Node node = new Node();
            node.title = "Menu Node";

            node.SetPosition(new Rect(graphMousePosition, new Vector2(200, 200)));

            AddElement(node);
        }
    }
}
