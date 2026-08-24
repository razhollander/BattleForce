using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.ScoreGate.Scripts.Mvc
{
    public interface IScoreGatesControllers
    {
        void InitEntryPoint();
        void InitExitPoint();
        void CreateScoreGate(ushort id, Vector2 position, Quaternion rotation, ushort lastScoredTeamId, ushort scoreMultiplier, float mapSizeMultiplier);
        void InterpolateScoreGateTransform(ushort id, Vector2 position, Quaternion rotation);
        void SetTeamColor(ushort id, ushort teamId);
        void SetScoreMultiplier(ushort id, ushort scoreMultiplier);
        void PlayScoreGatePassedAnimation(ushort id);
        bool TryGetScoreGatePosition(ushort id, out Vector2 position);
        void DestroyAll();
    }
}
