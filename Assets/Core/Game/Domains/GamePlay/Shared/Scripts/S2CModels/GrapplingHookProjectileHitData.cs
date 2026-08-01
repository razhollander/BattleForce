using System.Numerics;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels
{
    // Describes what the grappling hook attached to. Only IsHookAttached is serialized to the client; the rest are
    // server-only and let the hook follow the attached entity's transform (walls/frigid blocks/rocks can move & rotate).
    public struct GrapplingHookProjectileHitData
    {
        public bool IsHookAttached;
        public GrapplingHookHitType HitType;
        public ushort AttachedEntityId;
        public Vector2 AttachedLocalPosition;
    }
}
