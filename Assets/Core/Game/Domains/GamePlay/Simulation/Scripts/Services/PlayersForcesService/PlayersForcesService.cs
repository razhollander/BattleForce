using System.Collections.Generic;
using System.Numerics;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Services.PlayersForcesService
{
    public class PlayersForcesService : IPlayersForcesService
    {
        private class ForceData
        {
            public Vector2 Direction; // This represents Velocity vector contributed by this force
            public float Acceleration; // Friction/Decay for linear velocity
            public float SpinPower;
            public float SpinAcceleration;
        }

        private readonly Dictionary<ushort, List<ForceData>> _forcesPerPlayer = new Dictionary<ushort, List<ForceData>>();

        public void AddForce(ushort playerId, Vector2 forcePower, float acceleration, float spinPower, float spinAcceleration)
        {
            if (!_forcesPerPlayer.ContainsKey(playerId))
            {
                _forcesPerPlayer[playerId] = new List<ForceData>();
            }

            _forcesPerPlayer[playerId].Add(new ForceData
            {
                Direction = forcePower,
                Acceleration = acceleration,
                SpinPower = spinPower,
                SpinAcceleration = spinAcceleration
            });
        }

        public Vector2 CalculatePlayerVelocity(ushort playerId)
        {
            if (!_forcesPerPlayer.TryGetValue(playerId, out var forces))
            {
                return Vector2.Zero;
            }

            Vector2 totalVelocity = Vector2.Zero;
            foreach (var force in forces)
            {
                totalVelocity += force.Direction;
            }
            return totalVelocity;
        }

        public float CalculatePlayerSpin(ushort playerId)
        {
            if (!_forcesPerPlayer.TryGetValue(playerId, out var forces))
            {
                return 0f;
            }

            float totalSpin = 0f;
            foreach (var force in forces)
            {
                totalSpin += force.SpinPower;
            }
            return totalSpin;
        }

        public void Tick(float deltaTime)
        {
            foreach (var kvp in _forcesPerPlayer)
            {
                var forces = kvp.Value;
                for (int i = forces.Count - 1; i >= 0; i--)
                {
                    var force = forces[i];

                    // Linear decay
                    float currentMagnitude = force.Direction.Length();
                    if (currentMagnitude > 0)
                    {
                        float newMagnitude = currentMagnitude - (force.Acceleration * deltaTime);
                        if (newMagnitude <= 0)
                        {
                            force.Direction = Vector2.Zero;
                        }
                        else
                        {
                            force.Direction = Vector2.Normalize(force.Direction) * newMagnitude;
                        }
                    }

                    // Spin decay
                    if (System.Math.Abs(force.SpinPower) > 0)
                    {
                        float spinSign = System.Math.Sign(force.SpinPower);
                        float newSpin = System.Math.Abs(force.SpinPower) - (force.SpinAcceleration * deltaTime);
                        if (newSpin <= 0)
                        {
                            force.SpinPower = 0;
                        }
                        else
                        {
                            force.SpinPower = newSpin * spinSign;
                        }
                    }

                    // Remove if both exhausted
                    if (force.Direction == Vector2.Zero && force.SpinPower == 0)
                    {
                        forces.RemoveAt(i);
                    }
                }
            }
        }

        public void Clear(ushort playerId)
        {
            if (_forcesPerPlayer.ContainsKey(playerId))
            {
                _forcesPerPlayer[playerId].Clear();
            }
        }
    }
}
