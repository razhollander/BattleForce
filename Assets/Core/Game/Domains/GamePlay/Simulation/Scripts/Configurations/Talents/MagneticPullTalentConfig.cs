using System;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Simulation.Scripts.Configurations.Talents
{
    [Serializable]
    public class MagneticPullTalentConfig
    {
        public float FieldWidth = 3f;
        public float FieldHeight = 6f;
        public float OffsetFromPlayer = 2f;
        public float PushForce = 15f;
    }
}
