using Sirenix.OdinInspector;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "EnvironmentConfig", menuName = "BF/Network/Environment Config")]
    public class EnvironmentConfig : SerializedScriptableObject
    {
        public WallConfig[] Walls;

        public void SetWalls(WallConfig[] wallConfigs)
        {
            Walls = wallConfigs;
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