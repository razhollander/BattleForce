using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.MatchMaking.Features.Environment.Walls.Scripts.Mvcs
{
    public interface IMatchMakingEnvironmentWallsControllers
    {
        void InitEntryPoint();
        void CreateWall(ushort wallId);
        void AnimateWall(int wallId);
    }
}