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
        private string _assetPath;
        private bool _isGraphAsset;

        private void OnEnable()
        {
            var path = AssetDatabase.GetAssetPath(target);

            if (Path.GetExtension(path) == '.' + TextureMakerGraph.EXTENSION)
            {
                _graphAsset = GraphDatabase.LoadGraph<TextureMakerGraph>(path);
                _assetPath = path;
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
                _graphAsset.Execute(Path.GetFileNameWithoutExtension(_assetPath));
            })
            {
                text = "Execute Graph"
            };

            root.Add(executeButton);

            return root;
        }
    }
}
