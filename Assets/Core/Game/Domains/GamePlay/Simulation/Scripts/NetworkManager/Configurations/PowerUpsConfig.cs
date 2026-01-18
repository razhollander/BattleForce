using Core.Game.Domains.GamePlay.Shared.S2CModels;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.NetworkManager.Configurations
{
    [CreateAssetMenu(fileName = "PowerUpsConfig", menuName = "Core/Game/GamePlay/Simulation/Configurations/PowerUpsConfig")]
    public class PowerUpsConfig : ScriptableObject
    {
        public float SpawnInterval = 5f;
        public int MaxPowerUps = 5;
        public float MoveSpeed = 5f;
        public float Radius = 0.5f;
        public PowerUpType PowerUpType = PowerUpType.Turn;
    }
}
