using System.Numerics;
using Core.Game.Domains.GamePlay.Shared.Scripts.Configs;
using Core.Game.Domains.GamePlay.Shared.Scripts.S2CModels;
using Core.Scripts.Extensions.Linq;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Game.Domains.GamePlay.Simulation.Match.Scripts.MatchModel
{
    public class MatchEnvironmentDataService
    {
        public Vector2 EnvironmentHalfSize { get; private set; }
        public TalentCardS2C[] TalentCards { get; private set; }
        public EnvironmentSpringS2C[] EnvironmentSprings { get; private set; }
        public EnvironmentTeleportGatePairS2C[] TeleportGates { get; private set; }
        public WallConfig[] LavaWallConfigs { get; private set; }
        public WallConfig[] WallConfigs { get; private set; }
        public EnvironmentRotatingWheelConfig[] RotatingWheelConfigs { get; private set; }
        
        public EnvironmentSpringS2C GetSpring(ushort springId)
        {
            return EnvironmentSprings.FindWithId(springId);
        }
        
        public EnvironmentTeleportGatePairS2C GetTeleportGatePair(ushort teleportGatePairId)
        {
            return TeleportGates.FindWithId(teleportGatePairId);
        }
        
        public EnvironmentTeleportGatePairS2C GetTeleportGatePairOfGate(ushort teleportGateId)
        {
            for (int i = 0; i < TeleportGates.Length; i++)
            {
                var teleportGatePair = TeleportGates[i];

                if (teleportGatePair.GateBId == teleportGateId || teleportGatePair.GateAId == teleportGateId)
                {
                    return teleportGatePair;
                }
            }

            throw new System.Exception("No teleport gate pair found for gate id: " + teleportGateId);
        }
        
        private readonly SharedGamePlayConfig _sharedGamePlayConfig;
        
        public MatchEnvironmentDataService(SharedGamePlayConfig sharedGamePlayConfig)
        {
            _sharedGamePlayConfig = sharedGamePlayConfig;
        }

        public void InitEntryPoint(int environmentLayoutIndex)
        {
            WallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetWalls();
            LavaWallConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetLavaWalls();
            TalentCards = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetTalentCards();
            EnvironmentSprings = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetEnvironmentSprings();
            TeleportGates = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetTeleportGates();
            RotatingWheelConfigs = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetRotatingWheels();
            EnvironmentHalfSize = _sharedGamePlayConfig.Environment.GetEnvironmentLayout(environmentLayoutIndex).GetEnvironmentHalfSize();
        }
    }
}