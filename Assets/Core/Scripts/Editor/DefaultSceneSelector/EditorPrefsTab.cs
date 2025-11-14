using UnityEditor;

namespace CoreDomain.Scripts.Editor.DefaultSceneSelector
{
    public static class EditorPrefsTab
    {
        [MenuItem("Edit/Clear All PlayerEditorPrefs")]
        public static void DeleteAllEditorPrefs()
        {
            if (EditorUtility.DisplayDialog(
                    "Clear All EditorPrefs?",
                    "This will delete ALL EditorPrefs for Unity on this machine.\n\nAre you sure?",
                    "Yes, delete all",
                    "Cancel"))
            {
                EditorPrefs.DeleteAll();
            }
        }
    }
}