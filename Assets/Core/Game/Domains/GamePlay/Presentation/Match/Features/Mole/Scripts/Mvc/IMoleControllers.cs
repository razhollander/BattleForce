using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Mole.Scripts.Mvc
{
    public interface IMoleControllers
    {
        void InitEntryPoint();
        void CreateMoleAtSpawnPoint(ushort moleHoleId, Vector2 spawnPointPosition);
        void SetMoleEmergingFromHole(ushort moleId, ushort moleHoleId, float shakeDurationSeconds, bool isGolden, byte remainingLives, byte maxLives);
        void SetGoldenMoleDamaged(ushort moleId, ushort moleHoleId, byte remainingLives, byte maxLives);
        void SetMoleHit(ushort moleId, ushort moleHoleId);
        void SetMoleExpiring(ushort moleId, ushort moleHoleId, float shakeDurationSeconds);
        void SetAllMolesInHole();
        bool TryGetMoleHolePosition(ushort moleHoleId, out Vector2 position);
        void DestroyAll();
    }
}
