using Core.Game.Domains.GamePlay.Shared;

namespace Core.Game.Domains.GamePlay.Presentation.Features.Player.Scripts
{
    public interface IPlayerControllers
    {
        void InitEntryPoint();
        void CreatePlayer(int playerId);
    }
}