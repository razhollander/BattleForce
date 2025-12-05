using Core.Scripts.Editor.Utils;
using UnityEditor;

namespace Core.Scripts.Editor.EditorConfig
{
    public class ConfigurationWindow : EditorWindow
    {
        private const string LogsDefineSymbol = "Logs";
        private bool _areLogsEnabled;
        private bool _simulateNetworkLatency;

        [MenuItem("PracticAPI/Config")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigurationWindow>("Configuration");
            window.Init();
        }

        public void Init()
        {
            _areLogsEnabled = EditorUtils.IsSymbolEnabled(LogsDefineSymbol);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Toggle Compile Define", EditorStyles.boldLabel);

            var isEnabled = EditorGUILayout.Toggle("Are LogsEnabled", _areLogsEnabled);
            if (isEnabled == _areLogsEnabled)
            {
                return;
            }
            
            _areLogsEnabled = isEnabled;
            if (_areLogsEnabled)
            {
                EditorUtils.AddCompileDefine(LogsDefineSymbol);
            }
            else
            {
                EditorUtils.RemoveCompileDefine(LogsDefineSymbol);
            }
        }
    }
}