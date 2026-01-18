using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Network/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField]
        SerializableDictionary<int, EnvironmentLayoutConfig> _environmentLayoutConfigs = new SerializableDictionary<int, EnvironmentLayoutConfig>();

        public EnvironmentLayoutConfig GetEnvironmentLayout(int index)
        {
            return _environmentLayoutConfigs[index];
        }
        
        public void SetWalls(WallConfig[] wallConfigs, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetWallsJson(wallConfigs.ToJson());
            }
            else
            {
                _environmentLayoutConfigs[index] = new EnvironmentLayoutConfig(wallConfigs.ToJson(), "");
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetLavaWalls(WallConfig[] wallConfigs, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetLavaWallsJson(wallConfigs.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetLavaWallsJson(wallConfigs.ToJson());
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }
    }
}