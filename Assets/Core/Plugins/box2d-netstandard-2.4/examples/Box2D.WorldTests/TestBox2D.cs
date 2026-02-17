using System;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using Box2D.WorldTests;
using UnityEngine;

namespace Core.Plugins.box2d_netstandard_2._4.examples.Box2D.WorldTests
{
    public class TestBox2D : MonoBehaviour
    {
        private World _world;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            //_world = CarWorld.CreateWorld(out var bodies, out var joints);
            //_world = CollisionTestWorld.CreateWorld();
            //var cotan = new 
            //_world.SetContactListener(new contactl);
            // var testDebugDrawer = new TestDebugDrawer();
            // testDebugDrawer.AppendFlags(DrawFlags.Aabb);
            // testDebugDrawer.AppendFlags(DrawFlags.Joint);
            // testDebugDrawer.AppendFlags(DrawFlags.Pair);
            // testDebugDrawer.AppendFlags(DrawFlags.Shape);
            // testDebugDrawer.AppendFlags(DrawFlags.CenterOfMass);
            // _world.SetDebugDraw(testDebugDrawer);
        }

        // Update is called once per frame
        void FixedUpdate()
        {
           // _world.Step(Time.fixedDeltaTime, 8,8);
        }

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR && DEBUG_DRAW_ENABLED
            _world?.DrawDebugData();
#endif
        }
    }
}
