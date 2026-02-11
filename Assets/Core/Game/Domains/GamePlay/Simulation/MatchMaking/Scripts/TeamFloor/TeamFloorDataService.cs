using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Simulation.MatchMaking.Scripts.TeamFloor
{
    public interface ITeamFloorDataService
    {
        Dictionary<ushort, ushort> FloorIdToTeamId { get; }
    }

    public class TeamFloorDataService : ITeamFloorDataService
    {
        public Dictionary<ushort, ushort> FloorIdToTeamId { get; private set; }

        public TeamFloorDataService()
        {
            FloorIdToTeamId = new Dictionary<ushort, ushort>();
        }
    }
}