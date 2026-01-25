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
        private const string PhysicsDebugDrawDefineSymbol = "PHYSICS_DEBUG_DRAW_ENABLED";
        private bool _areInfoLogsEnabled;
        private bool _areErrorLogsEnabled;
        private bool _simulateNetworkLatency;
        private bool _arePhysicsDebugDrawEnabled;

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
            _arePhysicsDebugDrawEnabled = EditorUtils.IsSymbolEnabled(PhysicsDebugDrawDefineSymbol);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Toggle Compile Define", EditorStyles.boldLabel);

            _areInfoLogsEnabled = EditorGUILayout.Toggle("Are Info Logs Enabled", _areInfoLogsEnabled);
            _areErrorLogsEnabled = EditorGUILayout.Toggle("Are Error Logs Enabled", _areErrorLogsEnabled);
            _arePhysicsDebugDrawEnabled = EditorGUILayout.Toggle("Are Physics Debug Draw Enabled", _arePhysicsDebugDrawEnabled);
            if (EditorGUILayout.LinkButton("Refresh"))
            {
                TryRefreshDefineSymbols();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Settings", EditorStyles.boldLabel);
            var isPlayback = Core.Scripts.Utils.PlayerPrefsSettings.IsPlaybackEnabled;
            var newIsPlayback = EditorGUILayout.Toggle("Play last recorded match", isPlayback);
            if (newIsPlayback != isPlayback)
            {
                Core.Scripts.Utils.PlayerPrefsSettings.IsPlaybackEnabled = newIsPlayback;
            }
            
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
            var areCurrentPhysicsDebugDrawEnabled = EditorUtils.IsSymbolEnabled(PhysicsDebugDrawDefineSymbol);
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
            
            if (areCurrentPhysicsDebugDrawEnabled != _arePhysicsDebugDrawEnabled)
            {
                if (_arePhysicsDebugDrawEnabled)
                {
                    definesToAddList.Add(PhysicsDebugDrawDefineSymbol);
                }
                else
                {
                    definesToRemoveList.Add(PhysicsDebugDrawDefineSymbol);
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