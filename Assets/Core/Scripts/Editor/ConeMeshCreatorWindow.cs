using UnityEditor;
using UnityEngine;

namespace Core.Scripts.Editor
{
    /// <summary>
    /// Editor window that generates a 2D cone (sector / pie-slice) mesh via <see cref="MeshUtils.CreateConeMesh"/>
    /// and saves it as a .asset file in the project.
    /// </summary>
    public class ConeMeshCreatorWindow : EditorWindow
    {
        private float _radius = 8.5f;
        private float _openingAngle = 22.5f;
        private int _arcSegments = 16;
        private int _radialSegments = 8;
        private float _z;
        private string _assetName = "ConeMesh";

        [MenuItem("Tools/Mesh/Cone Mesh Creator")]
        private static void Open()
        {
            var window = GetWindow<ConeMeshCreatorWindow>(true, "Cone Mesh Creator");
            window.minSize = new Vector2(320, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Cone Parameters", EditorStyles.boldLabel);

            _radius = EditorGUILayout.FloatField("Radius", _radius);
            _openingAngle = EditorGUILayout.FloatField("Opening Angle (deg)", _openingAngle);
            _arcSegments = Mathf.Max(1, EditorGUILayout.IntField("Arc Segments", _arcSegments));
            _radialSegments = Mathf.Max(1, EditorGUILayout.IntField("Radial Segments", _radialSegments));
            _z = EditorGUILayout.FloatField("Z Offset", _z);

            EditorGUILayout.Space();
            _assetName = EditorGUILayout.TextField("Asset Name", _assetName);

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Mesh Asset", GUILayout.Height(30)))
                CreateAsset();
        }

        private void CreateAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Cone Mesh",
                string.IsNullOrWhiteSpace(_assetName) ? "ConeMesh" : _assetName,
                "asset",
                "Choose where to save the generated cone mesh.");

            if (string.IsNullOrEmpty(path))
                return;

            var mesh = MeshUtils.CreateConeMesh(_radius, _openingAngle, _arcSegments, _radialSegments, _z);
            mesh.name = System.IO.Path.GetFileNameWithoutExtension(path);

            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(mesh);
            Selection.activeObject = mesh;
        }
    }
}
