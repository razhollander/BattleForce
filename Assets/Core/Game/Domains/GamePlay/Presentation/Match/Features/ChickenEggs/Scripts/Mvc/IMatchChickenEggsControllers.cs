using System.Threading;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ChickenEggs.Scripts.Mvc
{
    public interface IMatchChickenEggsControllers
    {
        void InitEntryPoint();
        void CreateEgg(ushort eggId, Vector2 position);
        void DestroyAll();
        Awaitable BreakAndDestroyEgg(ushort eggId, CancellationTokenSource cancellationTokenSource);
    }
}
