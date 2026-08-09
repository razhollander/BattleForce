namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.PlayersTouchingSpikesTracker
{
    public readonly struct PlayerTouchingSpikeToDamageData
    {
        public readonly ushort PlayerId;
        public readonly ushort SpikeId;

        public PlayerTouchingSpikeToDamageData(ushort playerId, ushort spikeId)
        {
            PlayerId = playerId;
            SpikeId = spikeId;
        }
    }
}