using System;
using System.Collections.Generic;

namespace Core.Game.Domains.GamePlay.Shared.Scripts.Playback
{
    [Serializable]
    public class DebugTickData
    {
        public List<DebugRecordedPacket> Packets = new List<DebugRecordedPacket>();
    }
}