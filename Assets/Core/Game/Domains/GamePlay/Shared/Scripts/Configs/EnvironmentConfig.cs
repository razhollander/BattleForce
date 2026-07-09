using System.Collections.Generic;
using Core.Scripts.Extensions;
using Core.Scripts.Extensions.Linq;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Shared/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        private const int MAX_ID = 255;
        
        [SerializeField]
        SerializableDictionary<int, EnvironmentLayoutConfig> _environmentLayoutConfigs = new SerializableDictionary<int, EnvironmentLayoutConfig>();
#if UNITY_EDITOR
        public SerializableDictionary<int, EnvironmentLayoutConfig> Configs => _environmentLayoutConfigs;
        
#endif

        public List<int> AvailableLayoutIndexes;
        
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

        public void SetStageBoundaries(WallConfig[] wallConfigs, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetStageBoundariesJson(wallConfigs.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetStageBoundariesJson(wallConfigs.ToJson());
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

        public void SetEnvironmentSpikes(S2CModels.EnvironmentSpikeConfig[] environmentSpikes, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetEnvironmentSpikesJson(environmentSpikes.ToJson());
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetEnvironmentSpikesJson(environmentSpikes.ToJson());
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetTeleportGates(S2CModels.EnvironmentTeleportGatePairS2C[] teleportGates, int index)
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

        public void SetPowerUpSpawnPoints(S2CModels.PowerUpSpawnPointConfig[] powerUpSpawnPoints, int index)
        {
            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                var json = JsonConvert.SerializeObject(powerUpSpawnPoints);
                environmentLayout.SetPowerUpSpawnPointsJson(json);
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                var json = JsonConvert.SerializeObject(powerUpSpawnPoints);
                newLayout.SetPowerUpSpawnPointsJson(json);
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;

                System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();
                
                foreach (var kvp in _environmentLayoutConfigs)
                {
                    int index = kvp.Key;
                    var layout = kvp.Value;
                    if (layout == null) continue;

                    CheckConfigArray(layout.GetWalls(), $"Layout {index} Wall", errorBuilder);
                    CheckConfigArray(layout.GetLavaWalls(), $"Layout {index} LavaWall", errorBuilder);
                    CheckConfigArray(layout.GetStageBoundaries(), $"Layout {index} StageBoundary", errorBuilder);
                    CheckConfigArray(layout.GetTalentCards(), $"Layout {index} TalentCard", errorBuilder);
                    CheckConfigArray(layout.GetEnvironmentSprings(), $"Layout {index} EnvironmentSpring", errorBuilder);
                    CheckConfigArray(layout.GetEnvironmentSpikes(), $"Layout {index} EnvironmentSpike", errorBuilder);
                    CheckConfigArray(layout.GetTeleportGates(), $"Layout {index} TeleportGate", errorBuilder);

                    var wheels = layout.GetRotatingWheels();
                    if (wheels != null)
                    {
                        foreach (var w in wheels)
                        {
                            if (w == null) continue;
                            if (w.Id > MAX_ID) errorBuilder.AppendLine($"Layout {index} RotatingWheel ID {w.Id} > {MAX_ID}");

                            CheckConfigArray(w.Walls, $"Layout {index} RotatingWheel {w.Id} Wall", errorBuilder);
                            CheckConfigArray(w.LavaWalls, $"Layout {index} RotatingWheel {w.Id} LavaWall", errorBuilder);
                            CheckConfigArray(w.Springs, $"Layout {index} RotatingWheel {w.Id} Spring", errorBuilder);
                        }
                    }
                }

                if (errorBuilder.Length > 0)
                {
                    string errorMsg = "The following IDs exceed 255:\n" + errorBuilder.ToString();
                    Debug.LogError(errorMsg);
                    UnityEditor.EditorUtility.DisplayDialog("Environment Config ID Error", errorMsg, "OK");
                }
            };
        }

        private void CheckConfigArray(System.Collections.IEnumerable array, string prefix, System.Text.StringBuilder errorBuilder)
        {
            if (array == null) return;

            foreach (var item in array)
            {
                if (item == null) continue;

                var idField = item.GetType().GetField("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (idField != null)
                {
                    object idVal = idField.GetValue(item);
                    if (idVal != null)
                    {
                        try
                        {
                            int numericId = System.Convert.ToInt32(idVal);
                            if (numericId > MAX_ID)
                            {
                                errorBuilder.AppendLine($"{prefix} ID {numericId} > 255");
                            }
                        }
                        catch (System.Exception) { }
                    }
                }

                var idProp = item.GetType().GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (idProp != null)
                {
                    object idVal = idProp.GetValue(item);
                    if (idVal != null)
                    {
                        try
                        {
                            int numericId = System.Convert.ToInt32(idVal);
                            if (numericId > MAX_ID)
                            {
                                errorBuilder.AppendLine($"{prefix} ID {numericId} > 255");
                            }
                        }
                        catch (System.Exception) { }
                    }
                }
            }
        }
#endif
    }
}