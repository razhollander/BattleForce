using CoreDomain.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace CoreDomain.Scripts.Editor.ProjectManual
{
    [InitializeOnLoad]
    public class ProjectManualWindow : EditorWindow
    {
        private const string DONT_SHOW_AGAIN_KEY = "DontShowAgain";
        private const string PDF_PATH = "Assets/ProjectManual.pdf";
        private static readonly Vector2 WINDOW_SIZE = new Vector2(400, 220);
        
        private static bool _isDontShowAgain;

        static ProjectManualWindow()
        {
            EditorApplication.delayCall += TryOpenWindowOnStartup;
        }
        
        private static void TryOpenWindowOnStartup()
        {
            if (EditorPrefs.HasKey(DONT_SHOW_AGAIN_KEY) && EditorPrefs.GetBool(DONT_SHOW_AGAIN_KEY))
            {
                return;
            }

            var windows = Resources.FindObjectsOfTypeAll<ProjectManualWindow>();
            var isWindowAlreadyOpen = windows is {Length: > 0};
            if (!isWindowAlreadyOpen)
            {
                ShowWindow();
            }
        }

        [MenuItem("PracticAPI/Project Manual..")]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectManualWindow>();
            window.titleContent = new GUIContent("Project Manual");
            _isDontShowAgain = EditorPrefs.GetBool(DONT_SHOW_AGAIN_KEY);
            window.Show();
        }

        private void OnGUI()
        {
            DrawHeaderGUI();
            GUILayout.Space(10);
            DrawOpenProjectManualGUI();
            GUILayout.Space(10);
            DrawAutoOpenGUI();
            ResizeToFitContents();
        }

        private static void DrawHeaderGUI()
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.6f, 0.8f) }
            };
    
            GUILayout.Label("🎉 Thanks for your support! 🎉", headerStyle);
            GUILayout.Space(10);
            GUILayout.Label("Welcome to the Project Manual!\nWe hope you'll find this sample project useful. Enjoy!", EditorStyles.wordWrappedLabel);
        }

        private static void DrawOpenProjectManualGUI()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("📖 Project Manual PDF", EditorStyles.boldLabel);
    
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Open Project Manual -->", GUILayout.Height(40)))
            {
                IOUtils.OpenFile(PDF_PATH);
            }
            GUI.backgroundColor = Color.white;

            GUILayout.EndVertical();
        }

        private static void DrawAutoOpenGUI()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("⚙️ Settings", EditorStyles.boldLabel);
    
            var previousIsDontShowAgain = _isDontShowAgain;
            _isDontShowAgain = GUILayout.Toggle(_isDontShowAgain, "Dont Show Again This Window?");
            GUILayout.Label("Can always be found under the tab PracticAPI/Project Manual", EditorStyles.helpBox);
            
            bool didDontShowAgainChanged = previousIsDontShowAgain != _isDontShowAgain;

            if (didDontShowAgainChanged)
            {
                EditorPrefs.SetBool(DONT_SHOW_AGAIN_KEY, _isDontShowAgain);
            }

            GUILayout.EndVertical();
        }
        
        private void ResizeToFitContents()
        {
            minSize = WINDOW_SIZE;
            maxSize = WINDOW_SIZE;
        }
    }
}