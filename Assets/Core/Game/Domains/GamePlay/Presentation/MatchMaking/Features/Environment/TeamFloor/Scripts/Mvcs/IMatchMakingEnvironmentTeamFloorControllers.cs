namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.TeamFloor.Scripts.Mvcs
{
    public interface IMatchMakingEnvironmentTeamFloorControllers
    {
        void InitEntryPoint();
        void AnimateFloorBounce(ushort teamId);
    }
}