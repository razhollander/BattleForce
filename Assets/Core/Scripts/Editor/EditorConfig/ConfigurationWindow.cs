using System.Collections.Generic;
using Core.Scripts.Editor.Utils;
using NUnit.Framework;
using UnityEditor;

namespace Core.Scripts.Editor.EditorConfig
{
    public class ConfigurationWindow : EditorWindow
    {
        private const string InfoLogsDefineSymbol = "INFO_LOGS_ENABLED";
        private const string ErrorLogsDefineSymbol = "ERROR_LOGS_ENABLED";
        private const string DebugDrawDefineSymbol = "DEBUG_DRAW_ENABLED";
        private bool _areInfoLogsEnabled;
        private bool _areErrorLogsEnabled;
        private bool _simulateNetworkLatency;
        private bool _areDebugDrawEnabled;

        [MenuItem("PracticAPI/Config")]
        public static void ShowWindow()
        {
            var window = GetWindow<ConfigurationWindow>("Configuration");
            window.Init();
        }

        public void Init()
        {
            _areInfoLogsEnabled = EditorUtils.IsSymbolEnabled(InfoLogsDefineSymbol);
            _areErrorLogsEnabled = EditorUtils.IsSymbolEnabled(ErrorLogsDefineSymbol);
            _areDebugDrawEnabled = EditorUtils.IsSymbolEnabled(DebugDrawDefineSymbol);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Toggle Compile Define", EditorStyles.boldLabel);

            _areInfoLogsEnabled = EditorGUILayout.Toggle("Are Info Logs Enabled", _areInfoLogsEnabled);
            _areErrorLogsEnabled = EditorGUILayout.Toggle("Are Error Logs Enabled", _areErrorLogsEnabled);
            _areDebugDrawEnabled = EditorGUILayout.Toggle("Are Debug Draw Enabled", _areDebugDrawEnabled);
            if (EditorGUILayout.LinkButton("Refresh"))
            {
                TryRefreshDefineSymbols();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Settings", EditorStyles.boldLabel);
            
            var shouldSkipMatchMaking = Core.Scripts.Utils.PlayerPrefsSettings.ShouldSkipMatchMaking;
            var newShouldSkipMatchMaking = EditorGUILayout.Toggle("Should skip match making", shouldSkipMatchMaking);
            if (shouldSkipMatchMaking != newShouldSkipMatchMaking)
            {
                Core.Scripts.Utils.PlayerPrefsSettings.ShouldSkipMatchMaking = newShouldSkipMatchMaking;
            }
        }

        private void TryRefreshDefineSymbols()
        {
            var areCurrentInfoLogsEnabled = EditorUtils.IsSymbolEnabled(InfoLogsDefineSymbol);
            var areCurrentErrorLogsEnabled = EditorUtils.IsSymbolEnabled(ErrorLogsDefineSymbol);
            var areCurrentDebugDrawEnabled = EditorUtils.IsSymbolEnabled(DebugDrawDefineSymbol);
            var definesToRemoveList = new List<string>();
            var definesToAddList = new List<string>();
            if (areCurrentInfoLogsEnabled != _areInfoLogsEnabled)
            {
                if (_areInfoLogsEnabled)
                {
                    definesToAddList.Add(InfoLogsDefineSymbol);
                }
                else
                {
                    definesToRemoveList.Add(InfoLogsDefineSymbol);
                }
            }
                
            if (areCurrentErrorLogsEnabled != _areErrorLogsEnabled)
            {
                if (_areErrorLogsEnabled)
                {
                    definesToAddList.Add(ErrorLogsDefineSymbol);
                }
                else
                {
                    definesToRemoveList.Add(ErrorLogsDefineSymbol);
                }
            }
            
            if (areCurrentDebugDrawEnabled != _areDebugDrawEnabled)
            {
                if (_areDebugDrawEnabled)
                {
                    definesToAddList.Add(DebugDrawDefineSymbol);
                }
                else
                {
                    definesToRemoveList.Add(DebugDrawDefineSymbol);
                }
            }

            var didMakeAnyChange = definesToAddList.Count > 0 || definesToRemoveList.Count > 0;
            if (didMakeAnyChange)
            {
                EditorUtils.UpdateCompileDefines(definesToAddList, definesToRemoveList);
            }
        }
    }
}