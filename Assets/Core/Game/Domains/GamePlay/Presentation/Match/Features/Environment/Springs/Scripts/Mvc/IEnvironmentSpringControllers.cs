namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.Springs.Scripts.Mvc
{
    public interface IEnvironmentSpringControllers
    {
        void InitEntryPoint();
        void CreateSpring(ushort springId, UnityEngine.Transform parent = null);
        void DestroyAll();
        void PlaySpringBounceAnimation(ushort springId);
    }
}
