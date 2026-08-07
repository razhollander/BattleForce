using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public interface IMoleControllers
    {
        void InitEntryPoint();
        void CreateMoleAtSpawnPoint(Vector2 spawnPointPosition);
        void SetMoleEmergingFromHole(ushort moleId, Vector2 position, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives);
        void SetGoldenMoleDamaged(ushort moleId, byte remainingLives, byte maxLives);
        void SetMoleHit(ushort moleId);
        void SetMoleInHole(ushort moleId);
        bool TryGetMolePosition(ushort moleId, out Vector2 position);
        void DestroyAll();
    }
}
