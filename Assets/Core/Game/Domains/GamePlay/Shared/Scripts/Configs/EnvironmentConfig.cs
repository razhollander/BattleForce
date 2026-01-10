using Core.Scripts.Extensions;
using CoreDomain.Scripts.Helpers.SerializableDictionary;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Configs
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Network/Environment Config")]
    public class EnvironmentConfig : ScriptableObject
    {
        [SerializeField]
        SerializableDictionary<int, string> _wallsJson = new SerializableDictionary<int, string>();

        [SerializeField]
        SerializableDictionary<int, EnvironmentLayoutConfig> _environmentJson = new SerializableDictionary<int, EnvironmentLayoutConfig>();

        public WallConfig[] GetWalls(int index)
        {
            if (_environmentJson.ContainsKey(index))
            {
                return _environmentJson[index].Walls;
            }
            return _wallsJson[index].FromJson<WallConfig[]>();
        }
        
        public void SetWalls(WallConfig[] wallConfigs, int index)
        {
            _wallsJson[index] = wallConfigs.ToJson();
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }

        public EnvironmentLayoutConfig GetEnvironmentLayout(int index)
        {
            if (_environmentJson.ContainsKey(index))
            {
                return _environmentJson[index];
            }

            // Fallback for old data if needed, or just return basic layout with walls
            if (_wallsJson.ContainsKey(index))
            {
                var walls = _wallsJson[index].FromJson<WallConfig[]>();
                return new EnvironmentLayoutConfig(walls, new TalentCard[0]);
            }

            return null;
        }

        public void SetEnvironmentLayout(EnvironmentLayoutConfig layout, int index)
        {
            _environmentJson[index] = layout;
#if UNITY_EDITOR
            Core.Scripts.Editor.Utils.EditorUtils.SaveScriptableObject(this);
#endif
        }
    }
    
    public class WallConfig
    {
        public ushort Id;
        public Vector2[] Points;

        public WallConfig(ushort id, Vector2[] points)
        {
            Id = id;
            Points = points;
        }
    }
}