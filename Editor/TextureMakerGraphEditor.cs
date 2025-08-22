using System.IO;
using Unity.GraphToolkit.Editor;
using UnityEditor;
using UnityEngine.UIElements;

namespace Misaki.TextureMaker
{
    [CustomEditor(typeof(DefaultAsset))]
    public class TextureMakerGraphEditor : Editor
    {
        private TextureMakerGraph _graphAsset;
        private AssetImporter _importer;
        private bool _isGraphAsset;

        private void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);

            if (Path.GetExtension(path) == '.' + TextureMakerGraph.EXTENSION)
            {
                _graphAsset = GraphDatabase.LoadGraph<TextureMakerGraph>(path);
                _importer = AssetImporter.GetAtPath(path);
                _isGraphAsset = true;
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            if (!_isGraphAsset)
            {
                return base.CreateInspectorGUI();
            }

            var root = new VisualElement();
            var executeButton = new Button(() =>
            {
                _graphAsset.Execute();
            })
            {
                text = "Execute Graph"
            };

            root.Add(executeButton);

            return root;
        }
    }
}
