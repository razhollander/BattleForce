namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloorTracker
{
    public interface IPlayersOnTeamFloorTrackerService
    {
        void SetPlayerTeam(ushort playerId, ushort teamId);
        ushort GetPlayerTeam(ushort playerId);
        void RemovePlayer(ushort playerId);
    }
}