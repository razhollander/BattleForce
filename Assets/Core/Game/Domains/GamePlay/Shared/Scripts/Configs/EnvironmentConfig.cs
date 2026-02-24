using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Shared/Environment Config")]
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

        public void SetEnvironmentSprings(S2CModels.EnvironmentSpringConfig[] environmentSprings, int index)
        {
             if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetEnvironmentSpringsJson(environmentSprings.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetEnvironmentSpringsJson(environmentSprings.ToJson());
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetTeleportGates(Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels.EnvironmentTeleportGatePairS2C[] teleportGates, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetTeleportGatesJson(teleportGates.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetTeleportGatesJson(teleportGates.ToJson());
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetFieldBarriers(EnvironmentFieldBarrierConfig[] fieldBarriers, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetFieldBarriersJson(fieldBarriers.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetFieldBarriersJson(fieldBarriers.ToJson());
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }
    }
}