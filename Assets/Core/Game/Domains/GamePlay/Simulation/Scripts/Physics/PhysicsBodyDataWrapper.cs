namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Physics
{
    public class PhysicsBodyDataWrapper
    {
        public PhysicsBodyData Data;

        public void Reset(ushort id, PhysicsBodyType physicsBodyType)
        {
            Data = new PhysicsBodyData(id, physicsBodyType);
        }
    }
}
