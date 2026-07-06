namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.GalacticPullStar.Scripts.Mvc
{
    public interface IGalacticPullStarEffectControllers
    {
        void InitEntryPoint();
        void InitExitPoint();
        void ShowStar(ushort fieldId, ushort casterTeamId);
        void HideStarForceField(ushort starId);
        void DestroyAll();
    }
}
