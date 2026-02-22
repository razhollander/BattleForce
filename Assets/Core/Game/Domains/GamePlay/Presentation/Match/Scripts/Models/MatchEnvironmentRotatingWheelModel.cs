using System.Collections.Generic;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Scripts.Extensions;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Match.Scripts.Models
{
    public class MatchEnvironmentRotatingWheelModel
    {
        public ushort Id { get; private set; }
        public Vector2 CenterPosition { get; private set; }
        public float RotationSpeed { get; private set; }
        public float CurrentRotation { get; set; }

        public List<ushort> WallIds { get; private set; } = new List<ushort>();
        public List<ushort> LavaWallIds { get; private set; } = new List<ushort>();
        public List<ushort> SpringIds { get; private set; } = new List<ushort>();
        public EnvironmentRotatingWheelConfig Config { get; private set; }

        public MatchEnvironmentRotatingWheelModel(EnvironmentRotatingWheelConfig config)
        {
            Config = config;
            Id = config.Id;
            CenterPosition = config.CenterPosition.ToUnityVector2();
            RotationSpeed = config.RotationSpeed;
            CurrentRotation = 0f;

            if (config.Walls != null)
            {
                foreach (var wall in config.Walls) WallIds.Add(wall.Id);
            }
            if (config.LavaWalls != null)
            {
                foreach (var wall in config.LavaWalls) LavaWallIds.Add(wall.Id);
            }
            if (config.Springs != null)
            {
                foreach (var spring in config.Springs) SpringIds.Add(spring.Id);
            }
        }
    }
}
