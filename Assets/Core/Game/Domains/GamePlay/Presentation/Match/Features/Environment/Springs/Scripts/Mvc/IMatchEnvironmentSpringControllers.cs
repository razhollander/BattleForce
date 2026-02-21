using Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc;
using Core.Game.Domains.GamePlay.Presentation.Scripts.Models;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public interface IMatchEnvironmentSpringControllers
    {
        void InitEntryPoint();
        void CreateSpring(ushort springId);
        MatchEnvironmentSpringController GetSpring(ushort springId);
        void DestroyAll();
        void PlaySpringBounceAnimation(ushort springId);
    }
}
