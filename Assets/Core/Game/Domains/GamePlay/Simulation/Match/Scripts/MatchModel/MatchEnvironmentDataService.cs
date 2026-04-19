using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions.Linq;
using Core.Scripts.Network;
using Core.Scripts.Utils.CustomCollections;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    /// <summary>
    /// Everything here is pernament in the stage, and its position is determinstic and can't be changed by the user
    /// </summary>
    public class MatchEnvironmentDataService
    {
        private readonly FixedClassUnorderedList<EnvironmentSpringS2C> _springs;
        private readonly FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> _teleportGates;
        private readonly FixedClassUnorderedList<EnvironmentWallS2C> _lavaWalls;
        private readonly FixedClassUnorderedList<EnvironmentWallS2C> _stageBoundaries;
        private readonly FixedClassUnorderedList<EnvironmentWallS2C> _walls;
        private readonly FixedClassUnorderedList<EnvironmentRotatingWheelS2C> _rotatingWheels;
        private readonly FixedClassUnorderedList<MatchEnvironmentFieldBarrierModel> _fieldBarriers;

        public FixedClassUnorderedList<EnvironmentRotatingWheelS2C> RotatingWheels => _rotatingWheels;
        public FixedClassUnorderedList<EnvironmentSpringS2C> Springs => _springs;
        public FixedClassUnorderedList<EnvironmentTeleportGatePairS2C> TeleportGates => _teleportGates;
        public FixedClassUnorderedList<EnvironmentWallS2C> LavaWalls => _lavaWalls;
        public FixedClassUnorderedList<EnvironmentWallS2C> StageBoundaries => _stageBoundaries;
        public FixedClassUnorderedList<EnvironmentWallS2C> Walls => _walls;
        public FixedClassUnorderedList<MatchEnvironmentFieldBarrierModel> FieldBarriers => _fieldBarriers;

        public MatchEnvironmentDataService(NetworkConfig networkConfig)
        {
            _springs = new FixedClassUnorderedList<EnvironmentSpringS2C>(networkConfig.MaxCap.ConcurrentEvironmentSprings, ()=> new EnvironmentSpringS2C());
            _teleportGates = new FixedClassUnorderedList<EnvironmentTeleportGatePairS2C>(networkConfig.MaxCap.ConcurrentEvironmentTeleportPairs, ()=> new EnvironmentTeleportGatePairS2C());
            _lavaWalls = new FixedClassUnorderedList<EnvironmentWallS2C>(networkConfig.MaxCap.ConcurrentEvironmentLavaWalls, ()=> new EnvironmentWallS2C());
            _stageBoundaries = new FixedClassUnorderedList<EnvironmentWallS2C>(networkConfig.MaxCap.ConcurrentEvironmentStageBoundaries, ()=> new EnvironmentWallS2C());
            _walls = new FixedClassUnorderedList<EnvironmentWallS2C>(networkConfig.MaxCap.ConcurrentEvironmentWalls, ()=> new EnvironmentWallS2C());
            _rotatingWheels = new FixedClassUnorderedList<EnvironmentRotatingWheelS2C>(networkConfig.MaxCap.ConcurrentEnvironmentRotatingWheels, ()=> new EnvironmentRotatingWheelS2C(networkConfig.MaxCap.EnvironmentRotatingWheelCap));
            _fieldBarriers = new FixedClassUnorderedList<MatchEnvironmentFieldBarrierModel>(networkConfig.MaxCap.ConcurrentFieldBarriers, () => new MatchEnvironmentFieldBarrierModel());
        }

        public void ClearData()
        {
            _springs.Clear();
            _teleportGates.Clear();
            _lavaWalls.Clear();
            _stageBoundaries.Clear();
            _walls.Clear();
            _fieldBarriers.Clear();

            foreach (var rotatingWheel in _rotatingWheels.AsSpan())
            {
                rotatingWheel.ClearData();
            }
            _rotatingWheels.Clear();
        }
        
        public void AddWall(ushort wallId, Vector2[] Points, Vector2 localPosition, Vector2 worldPosition, float worldRotationDegrees)
        {
            var wall = _walls.AddAndGet();
            wall.Id = wallId;
            wall.SetPoints(Points);
            wall.Transform.WorldRotationDegrees = worldRotationDegrees;
            wall.Transform.LocalPosition = localPosition;
            wall.Transform.WorldPosition = worldPosition;
        }
        
        public void AddLavaWall(ushort lavaWallId, Vector2[] Points, Vector2 localPosition, Vector2 worldPosition, float worldRotationDegrees)
        {
            var lavaWall = _lavaWalls.AddAndGet();
            lavaWall.Id = lavaWallId;
            lavaWall.SetPoints(Points);
            lavaWall.Transform.WorldRotationDegrees = worldRotationDegrees;
            lavaWall.Transform.LocalPosition = localPosition;
            lavaWall.Transform.WorldPosition = worldPosition;
        }
        
        public void AddStageBoundary(ushort stageBoundaryId, Vector2[] Points, Vector2 localPosition, Vector2 worldPosition, float worldRotationDegrees)
        {
            var stageBoundary = _stageBoundaries.AddAndGet();
            stageBoundary.Id = stageBoundaryId;
            stageBoundary.SetPoints(Points);
            stageBoundary.Transform.WorldRotationDegrees = worldRotationDegrees;
            stageBoundary.Transform.LocalPosition = localPosition;
            stageBoundary.Transform.WorldPosition = worldPosition;
        }

        public void AddSpring(ushort springId, Vector2 localPosition, Vector2 worldPosition, float localRotationDegrees, float worldRotationDegrees)
        {
            var spring = _springs.AddAndGet();
            spring.Id = springId;
            spring.Transform.LocalRotationDegrees = localRotationDegrees;
            spring.Transform.WorldRotationDegrees = worldRotationDegrees;
            spring.Transform.LocalPosition = localPosition;
            spring.Transform.WorldPosition = worldPosition;
        }
        
        public EnvironmentRotatingWheelS2C AddRotatingWheel(ushort rotatingWheelId, Vector2 centerPosition, float rotationSpeed)
        {
            var rotatingWheel = _rotatingWheels.AddAndGet();
            rotatingWheel.Id = rotatingWheelId;
            rotatingWheel.RotationSpeed = rotationSpeed;
            rotatingWheel.CenterPosition = centerPosition;
            return rotatingWheel;
        }

        public void AddFieldBarrier(ushort id, ushort teamId, Vector2 position, Vector2 size, Core.Game.Domains.GamePlay.Shared.Scripts.Enums.FieldBarrierShape shape)
        {
            var barrier = _fieldBarriers.AddAndGet();
            barrier.Id = id;
            barrier.TeamId = teamId;
            barrier.Position = position;
            barrier.Size = size;
            barrier.Shape = shape;
        }

        public void RemoveAllFieldBarriers()
        {
            _fieldBarriers.Clear();
        }
        
        public EnvironmentTeleportGatePairS2C GetTeleportGatePairOfGate(ushort teleportGateId)
        {
            foreach (var teleportGatePair in _teleportGates.AsSpan())
            {
                if (teleportGatePair.GateB.Id == teleportGateId || teleportGatePair.GateA.Id == teleportGateId)
                {
                    return teleportGatePair;
                }
            }

            throw new System.Exception("No teleport gate pair found for gate id: " + teleportGateId);
        }

        public EnvironmentSpringS2C GetSpring(ushort springId)
        {
            return _springs.FindWithId(springId);
        }
        
        public EnvironmentTeleportGatePairS2C GetTeleportGatePair(ushort teleportGatePairId)
        {
            return _teleportGates.FindWithId(teleportGatePairId);
        }
        
        public EnvironmentWallS2C GetLavaWall(ushort lavaWallId)
        {
            return _lavaWalls.FindWithId(lavaWallId);
        }

        public EnvironmentWallS2C GetStageBoundary(ushort stageBoundaryId)
        {
            return _stageBoundaries.FindWithId(stageBoundaryId);
        }
        
        public EnvironmentWallS2C GetWall(ushort wallId)
        {
            return _walls.FindWithId(wallId);
        }

        public bool TryGetEnvironmentWall(ushort wallId, out EnvironmentWallS2C wall)
        {
            for (int i = 0; i < _walls.Count; i++)
            {
                if (_walls[i].Id == wallId)
                {
                    wall = _walls[i];
                    return true;
                }
            }

            wall = null;
            return false;
        }

        public MatchEnvironmentFieldBarrierModel GetBarrierForTeam(ushort teamId)
        {
            foreach (var barrier in FieldBarriers.AsSpan())
            {
                if (barrier.TeamId == teamId) return barrier;
            }
            
            LogService.LogError("No field barrier found for team id: " + teamId);
            return null;
        }
        
        public void AddTeleportGatePair(ushort teleportPairId, ushort gateAId, ushort gateBId, Vector2 gateAPosition, float gateANormalRotation, Vector2 gateBPosition,
            float gateBNormalRotation, Vector2 gateAWorldPosition, float gateAWorldRotation, Vector2 gateBWorldPosition, float gateBWorldRotation)

        {
            var teleportGatePair = _teleportGates.AddAndGet();
            teleportGatePair.Id = teleportPairId;
            teleportGatePair.GateA.Transform.LocalRotationDegrees = gateANormalRotation;
            teleportGatePair.GateA.Transform.LocalPosition = gateAPosition;
            teleportGatePair.GateA.Transform.WorldPosition = gateAWorldPosition;
            teleportGatePair.GateA.Transform.WorldRotationDegrees = gateAWorldRotation;
            teleportGatePair.GateA.Id = gateAId;
            teleportGatePair.GateB.Transform.LocalRotationDegrees = gateBNormalRotation;
            teleportGatePair.GateB.Transform.LocalPosition = gateBPosition;
            teleportGatePair.GateB.Transform.WorldPosition = gateBWorldPosition;
            teleportGatePair.GateB.Transform.WorldRotationDegrees = gateBWorldRotation;
            teleportGatePair.GateB.Id = gateBId;
        }
    }
}