using Core.Scripts.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Network/Environment Config")]
    public class EnvironmentConfig : SerializedScriptableObject
    {
        [SerializeField] string _wallsJson;

        public WallConfig[] GetWalls()
        {
            return _wallsJson.FromJson<WallConfig[]>();
        }
        
        public void SetWalls(WallConfig[] wallConfigs)
        {
            _wallsJson = wallConfigs.ToJson();
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