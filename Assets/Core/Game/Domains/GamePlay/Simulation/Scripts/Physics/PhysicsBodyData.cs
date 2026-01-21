namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public struct PhysicsBodyData
    {
        public readonly ushort Id;
        public readonly PhysicsBodyType PhysicsBodyType;

        public PhysicsBodyData(ushort id, PhysicsBodyType physicsBodyType)
        {
            Id = id;
            PhysicsBodyType = physicsBodyType;
        }
    }
}