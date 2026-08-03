using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public interface IMoleControllers
    {
        void InitEntryPoint();
        void CreateMoleAtSpawnPoint(Vector2 spawnPointPosition);
        void SetMoleOutsideHole(ushort moleId, Vector2 position);
        void SetMoleHit(ushort moleId);
        void SetMoleInHole(ushort moleId);
        Vector2 GetMolePosition(ushort moleId);
        void DestroyAll();
    }
}
