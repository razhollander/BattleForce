namespace Core.Game.Domains.GamePlay.Presentation.Match.Features.WaterGunStream.Scripts.Mvc
{
    public interface IWaterGunStreamControllers
    {
        void InitEntryPoint();
        void Spawn(ushort playerId);
        void Despawn(ushort playerId);
        void Tick(ushort playerId, UnityEngine.Vector2 aimDirection, float angularVelocity);
    }
}
