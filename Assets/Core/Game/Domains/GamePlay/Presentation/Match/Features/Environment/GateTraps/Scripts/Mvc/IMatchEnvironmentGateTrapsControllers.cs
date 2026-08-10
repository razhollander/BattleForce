namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.Environment.GateTraps.Scripts.Mvc
{
    public interface IMatchEnvironmentGateTrapsControllers
    {
        void InitEntryPoint();
        void CreateGateTrap(ushort gateTrapId);
        void UpdateGateTrapViews();
        void DestroyAll();
    }
}
