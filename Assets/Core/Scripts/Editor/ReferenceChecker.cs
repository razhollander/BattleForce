using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Scripts.Editor
{
    public class ReferenceChecker : EditorWindow
    {
        [MenuItem("Assets/Check ALL Object References", false)]
        private static void CheckReferences()
        {
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError("No asset selected!");
                return;
            }

            var allAssets = AssetDatabase.FindAssets("", new[] { "Assets" });
            var referencingAssets = new List<Object>();

            foreach (var guid in allAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var assetDependencies = AssetDatabase.GetDependencies(path, true);
                if (Array.Exists(assetDependencies, dependency => dependency == assetPath))
                {
                    referencingAssets.Add(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            }

            Selection.objects = referencingAssets.ToArray();
            referencingAssets.Clear(); 
            ShowWindow();
        }
        
        private static void ShowWindow()
        {
            GetWindow<ReferenceChecker>("Selected Assets");
        }

        private void OnGUI()
        {
            GUILayout.Label("Selected Assets", EditorStyles.boldLabel);

            foreach (var obj in Selection.objects)
            {
                EditorGUILayout.ObjectField(obj, typeof(Object), false);
            }
        }
    }
}