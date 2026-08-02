using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Inputs
{
    public interface IPlayersMouseDataService
    {
        void SetPlayerMouseData(ushort playerId, bool isUsingMouseAim, Vector2 mouseWorldPosition);
        PlayerMouseData GetPlayerMouseData(ushort playerId);
    }

    public struct PlayerMouseData
    {
        public bool IsUsingMouseAim;
        public Vector2 MouseWorldPosition;
    }
}
