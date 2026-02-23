using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public interface IEnvironmentSpringControllers
    {
        void InitEntryPoint();
        void CreateSpring(ushort springId);
        void DestroyAll();
        void PlaySpringBounceAnimation(ushort springId);
        void UpdateSpringTransform(ushort springId);
    }
}
