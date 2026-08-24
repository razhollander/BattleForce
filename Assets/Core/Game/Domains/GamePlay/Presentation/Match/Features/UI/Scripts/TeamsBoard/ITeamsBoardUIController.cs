namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public interface ITeamsBoardUIController
    {
        void UpdateTeamGems(ushort teamId, int gems);
        void UpdateTeamBolts(ushort teamId, int teamBolts);
        void UpdateTeamMolesKilled(ushort teamId, int molesKilled);
        void UpdateTeamGatePassScore(ushort teamId, int gatePassScore);
        void SetIsMolesKilledShown(bool isShown);
        void SetIsGatePassScoreShown(bool isShown);
        void CreateTeamBoard(ushort teamId, int teamGems, int teamBolts);
        void DestroyAll();
    }
}
