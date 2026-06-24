using Core.Game.Domains.GamePlay.Shared.S2CModels;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class PowerUpsConfig
    {
        public float SpawnMinSecondsInterval = 5f;
        public float SpawnMaxSecondsInterval = 10f;
        public int MaxConcurrentPowerUpBalls = 5;
        public float MoveSpeed = 5f;
        public float Radius = 1.269f;
        public PowerUpType[] ObtainablePowerUps = { PowerUpType.SonicSlap, PowerUpType.GalacticPull, PowerUpType.Nuke };
        public float GalacticPullDurationSeconds = 5f;
        public float GalacticPullForce = 8f;
        public float NukeForce = 15f;
        public float NukeMinSpinAmount = 180f;
        public float NukeMaxSpinAmount = 720f;
    }
}
