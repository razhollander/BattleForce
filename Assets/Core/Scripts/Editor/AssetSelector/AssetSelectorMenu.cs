// using UnityEditor;
// using UnityEngine;
//
// namespace Core.Scripts.Editor.AssetSelector
// {
//     public class AssetSelectorMenu
//     {
//         private const string SIMULATION_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Simulation/Assets/Configs/SimulationGamePlayConfig.asset";
//         private const string NETWORK_CONFIG_ASSET_PATH = "Assets/Core/Assets/Network/NetworkConfig.asset";
//         private const string PRESENTATION_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Presentation/Assets/Configs/PresentationGamePlayConfig.asset";
//         private const string SHARED_GAMEPLAY_CONFIG_ASSET_PATH = "Assets/Core/Game/Domains/GamePlay/Shared/Assets/SharedGamePlayConfig.asset";
//             
//         [MenuItem("PracticAPI/Select Asset/Network Config", false, 1)]
//         private static void SelectNetworkConfig()
//         {
//             SelectAsset(NETWORK_CONFIG_ASSET_PATH);
//         }
//
//         [MenuItem("PracticAPI/Select Asset/Simulation GamePlay Config", false, 2)]
//         private static void SelectGamePlayConfig()
//         {
//             SelectAsset(SIMULATION_GAMEPLAY_CONFIG_ASSET_PATH);
//         }
//
//         [MenuItem("PracticAPI/Select Asset/Presentation GamePlay Config", false, 3)]
//         private static void SelectPresentationGamePlayConfig()
//         {
//             SelectAsset(PRESENTATION_GAMEPLAY_CONFIG_ASSET_PATH);
//         }
//         
//         [MenuItem("PracticAPI/Select Asset/Shared GamePlay Config", false, 3)]
//         private static void SelectSharedGamePlayConfig()
//         {
//             SelectAsset(SHARED_GAMEPLAY_CONFIG_ASSET_PATH);
//         }
//         
//         private static void SelectAsset(string assetPath)
//         {
//             ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
//
//             if (asset != null)
//             {
//                 Selection.activeObject = asset;
//                 EditorGUIUtility.PingObject(asset);
//             }
//             else
//             {
//                 Debug.LogError($"Asset not found at path: {assetPath}");
//             }
//         }
//     }
// }