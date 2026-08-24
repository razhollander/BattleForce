namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations
{
    [System.Serializable]
    public class WhacAMoleConfig
    {
        public float StageDurationSeconds = 60f;
        public float MinMoleSpawnIntervalSeconds = 1f;
        public float MaxMoleSpawnIntervalSeconds = 2f;
        public int MaxConcurrentMoles = 8;
        public float MoleRadius = 0.8f;
        public float MoleHoleReuseCooldownSeconds = 1f;
        public float MinMoleLifetimeSeconds = 3f;
        public float MaxMoleLifetimeSeconds = 5f; // zero or negative max means moles never expire on their own
        public ushort ScorePerMoleKilled = 1;
        public int MinMolesUntilGoldenMole = 3;
        public int MaxMolesUntilGoldenMole = 6;
        public int GoldenMoleLives = 3;
        public ushort GoldenMoleScoreOnKill = 3;
    }
}
