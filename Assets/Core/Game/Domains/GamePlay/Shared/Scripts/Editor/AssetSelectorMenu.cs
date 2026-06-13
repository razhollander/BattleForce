using UnityEditor;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Editor
{
    public class AssetSelectorMenu
    {
        private const string SIMULATION_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Simulation/Assets/Configs/SimulationGamePlayConfig.asset";
        private const string NETWORK_CONFIG_ASSET_PATH = "Assets/Core/Assets/Network/NetworkConfig.asset";
        private const string PRESENTATION_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Presentation/Assets/Configs/PresentationGamePlayConfig.asset";
        private const string SHARED_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Shared/Assets/SharedGamePlayConfig.asset";
            
        [MenuItem("PracticAPI/Select Asset/Network Config", false, 1)]
        private static void SelectNetworkConfig()
        {
            SelectAssetAtPath(NETWORK_CONFIG_ASSET_PATH);
        }

        [MenuItem("PracticAPI/Select Asset/Simulation GamePlay Config", false, 2)]
        private static void SelectGamePlayConfig()
        {
            SelectAssetAtPath(SIMULATION_GAMEPLAY_CONFIG_ASSET_PATH);
        }

        [MenuItem("PracticAPI/Select Asset/Presentation GamePlay Config", false, 3)]
        private static void SelectPresentationGamePlayConfig()
        {
            SelectAssetAtPath(PRESENTATION_GAMEPLAY_CONFIG_ASSET_PATH);
        }
        
        [MenuItem("PracticAPI/Select Asset/Shared GamePlay Config", false, 4)]
        private static void SelectSharedGamePlayConfig()
        {
            SelectAssetAtPath(SHARED_GAMEPLAY_CONFIG_ASSET_PATH);
        }
        
        [MenuItem("PracticAPI/Select Asset/Environment Config", false, 5)]
        private static void SelectEnvironmentConfig()
        {
            var sharedGamePlayConfig = AssetDatabase.LoadAssetAtPath<SharedGamePlayConfig>(SHARED_GAMEPLAY_CONFIG_ASSET_PATH);
            SelectAsset(sharedGamePlayConfig.Environment);
        }
        
        private static void SelectAssetAtPath(string assetPath)
        {
            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (asset != null)
            {
                SelectAsset(asset);
            }
            else
            {
                Debug.LogError($"Asset not found at path: {assetPath}");
            }
        }

        private static void SelectAsset(ScriptableObject asset)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }
}