namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public interface ITeamsBoardUIController
    {
        void UpdateTeamGems(ushort teamId, int gems);
        void UpdateTeamBolts(ushort teamId, int teamBolts);
        void UpdateTeamMolesHit(ushort teamId, int molesHit);
        void SetIsMolesHitShown(bool isShown);
        void CreateTeamBoard(ushort teamId, int teamGems, int teamBolts);
        void DestroyAll();
    }
}
