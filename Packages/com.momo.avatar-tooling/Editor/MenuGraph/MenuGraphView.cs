using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace MomoVRChatTools.Editor
{
    public class MenuGraphView : GraphView
    {
        private const string styleSheetPath = "Packages/com.momo.avatar-tooling/Editor/MenuGraph/USS/MenuGraphEditor.uss";

        public MenuGraphView()
        {
            StyleSheet style = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            styleSheets.Add(style);

            GridBackground background = new GridBackground();
            background.name = "Grid";
            Add(background);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
        }
    }
}
