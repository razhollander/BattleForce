using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.Enums;
using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using Newtonsoft.Json;
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

        // The DeathMatch layout pool. Kept under its original name so existing assets keep working.
        public List<int> AvailableLayoutIndexes;
        public List<int> WhacAMoleLayoutIndexes;
        public List<int> GatePassLayoutIndexes;

        public EnvironmentLayoutConfig GetEnvironmentLayout(int index)
        {
            return _environmentLayoutConfigs[index];
        }

        public List<int> GetLayoutIndexesForStageType(StageType stageType)
        {
            switch (stageType)
            {
                case StageType.WhacAMole: return WhacAMoleLayoutIndexes;
                case StageType.GatePass: return GatePassLayoutIndexes;
                default: return AvailableLayoutIndexes;
            }
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

        public void SetMoleSpawnPoints(S2CModels.MoleSpawnPointConfig[] moleSpawnPoints, int index)
        {
            var json = JsonConvert.SerializeObject(moleSpawnPoints);

            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetMoleSpawnPointsJson(json);
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetMoleSpawnPointsJson(json);
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetScoreGates(S2CModels.ScoreGateConfig[] scoreGates, int index)
        {
            var json = JsonConvert.SerializeObject(scoreGates);

            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetScoreGatesJson(json);
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetScoreGatesJson(json);
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetGateTraps(EnvironmentGateTrapConfig[] gateTraps, int index)
        {
            var json = JsonConvert.SerializeObject(gateTraps);

            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetGateTrapsJson(json);
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetGateTrapsJson(json);
                _environmentLayoutConfigs[index] = newLayout;
            }
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public void SetCameraBoundaries(S2CModels.CameraBoundariesConfig cameraBoundaries, int index)
        {
            var json = JsonConvert.SerializeObject(cameraBoundaries);

            if (_environmentLayoutConfigs.TryGetValue(index, out var environmentLayout))
            {
                environmentLayout.SetCameraBoundariesJson(json);
            }
            else
            {
                var newLayout = new EnvironmentLayoutConfig("", "");
                newLayout.SetCameraBoundariesJson(json);
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
                    CheckConfigArray(layout.GetGateTraps(), $"Layout {index} GateTrap", errorBuilder);

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

                    CheckWallIdsAreUnique(layout, index, errorBuilder);
                }

                if (errorBuilder.Length > 0)
                {
                    string errorMsg = "The following environment config problems were found:\n" + errorBuilder.ToString();
                    Debug.LogError(errorMsg);
                    UnityEditor.EditorUtility.DisplayDialog("Environment Config Error", errorMsg, "OK");
                }
            };
        }

        // Plain walls, a wheel's walls and a gate trap's wall all end up in the same list and the same physics id space,
        // so a duplicate makes CopyWallStateToBody drive both bodies onto one transform instead of failing loudly.
        private void CheckWallIdsAreUnique(EnvironmentLayoutConfig layout, int index, System.Text.StringBuilder errorBuilder)
        {
            var usedWallIds = new HashSet<ushort>();

            var walls = layout.GetWalls();
            if (walls != null)
            {
                foreach (var wall in walls)
                {
                    if (wall != null && !usedWallIds.Add(wall.Id))
                    {
                        errorBuilder.AppendLine($"Layout {index} Wall ID {wall.Id} is used more than once");
                    }
                }
            }

            var wheels = layout.GetRotatingWheels();
            if (wheels != null)
            {
                foreach (var wheel in wheels)
                {
                    if (wheel?.Walls == null) continue;

                    foreach (var wheelWall in wheel.Walls)
                    {
                        if (wheelWall != null && !usedWallIds.Add(wheelWall.Id))
                        {
                            errorBuilder.AppendLine($"Layout {index} RotatingWheel {wheel.Id} Wall ID {wheelWall.Id} is used more than once");
                        }
                    }
                }
            }

            var gateTraps = layout.GetGateTraps();
            if (gateTraps != null)
            {
                foreach (var gateTrap in gateTraps)
                {
                    if (gateTrap != null && !usedWallIds.Add(gateTrap.WallId))
                    {
                        errorBuilder.AppendLine($"Layout {index} GateTrap {gateTrap.Id} WallId {gateTrap.WallId} is already used by another wall");
                    }
                }
            }
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