namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.UI.Scripts.TeamsBoard
{
    public interface ITeamsBoardUIController
    {
        void InitEntryPoint();
        void UpdateTeamGems(ushort teamId, int gems);
        void UpdateTeamBolts(ushort teamId, int bolts);
    }
}
