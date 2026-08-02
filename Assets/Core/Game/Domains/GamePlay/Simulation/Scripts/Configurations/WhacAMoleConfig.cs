namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class WhacAMoleConfig
    {
        public float StageDurationSeconds = 60f;
        public float MoleSpawnIntervalSeconds = 1.5f;
        public int MaxConcurrentMoles = 8;
        public float MoleRadius = 0.8f;
        public float MoleLifetimeSeconds = 4f; // zero or negative means moles never expire on their own
        public int ScorePerMoleHit = 1;
        public int GemsForWinningTeam = 1;
    }
}
