using System.Text;
using Core.Game.Domains.GamePlay.Presentation.Scripts.ScriptableObjects;
using Core.Scripts.Network;
using CoreDomain.Scripts.Services.Logger.Base;
using UnityEngine;

namespace Core.Game.Domains.GamePlay.Presentation.Scripts.DataService
{
    /// <summary>
    /// Measures how evenly server state actually reaches this client, to tell network jitter apart from local frame
    /// hitching - the two look identical on screen but have nothing to do with each other.
    ///
    /// Note what is and is not measurable here. LiteNetLib runs with UnsyncedEvents off, so packets are handed over
    /// inside PollEvents on the main thread rather than at socket-receive time: every packet that landed during the
    /// last fixed update is delivered in the same batch, which makes true wire inter-arrival timing unrecoverable from
    /// this side. What that batching does expose is the number that actually matters - how many state packets each poll
    /// had to work with. A healthy connection delivers exactly one per poll. Polls with none are the stalls the view
    /// renders as a pause, and polls with several are the bursts that follow them.
    /// </summary>
    public class NetworkDiagnosticsService : INetworkDiagnosticsService
    {
        private const float REPORT_INTERVAL_IN_SECONDS = 5f;
        private const string REPORT_PREFIX = "[NetDiag]";

        private readonly bool _isLoggingEnabled;
        private readonly float _tickDeltaTime;
        private readonly IInterpolationDecayService _interpolationDecayService;
        private readonly StringBuilder _reportBuilder = new StringBuilder(512);

        private Window _window;
        private int _packetsInCurrentPoll;
        private int _lastPingInMilliseconds;
        private float _windowStartTime;

        public string LastReportText { get; private set; } = REPORT_PREFIX + " collecting...";

        public NetworkDiagnosticsService(PresentationGamePlayConfig gamePlayConfig, NetworkConfig networkConfig, IInterpolationDecayService interpolationDecayService)
        {
            _isLoggingEnabled = gamePlayConfig.IsNetworkDiagnosticsLoggingEnabled;
            _tickDeltaTime = networkConfig.DeltaTime;
            _interpolationDecayService = interpolationDecayService;
            _windowStartTime = Time.realtimeSinceStartup;
        }

        public void OnFullTickPacketReceived()
        {
            _packetsInCurrentPoll++;
            _window.PacketsReceived++;
        }

        public void OnStateProcessed(int ticksAdvancedSinceLastProcessedState)
        {
            _window.StatesProcessed++;

            if (ticksAdvancedSinceLastProcessedState > _window.MaxTicksAdvanced)
            {
                _window.MaxTicksAdvanced = ticksAdvancedSinceLastProcessedState;
            }

            switch (ticksAdvancedSinceLastProcessedState)
            {
                case 1: _window.TickGap1++; break;
                case 2: _window.TickGap2++; break;
                case 3: _window.TickGap3++; break;
                case >= 4 and <= 7: _window.TickGap4To7++; break;
                default: _window.TickGap8Plus++; break;
            }
        }

        public void OnPollCompleted(int pingInMilliseconds)
        {
            _window.Polls++;
            _lastPingInMilliseconds = pingInMilliseconds;

            if (_packetsInCurrentPoll == 0)
            {
                _window.PollsWithNoPacket++;
            }
            else if (_packetsInCurrentPoll > 1)
            {
                _window.PollsWithMultiplePackets++;
            }

            if (_packetsInCurrentPoll > _window.MaxPacketsInPoll)
            {
                _window.MaxPacketsInPoll = _packetsInCurrentPoll;
            }

            _packetsInCurrentPoll = 0;

            var decay = _interpolationDecayService.CurrentDecay;
            var isFirstDecaySampleOfWindow = _window.Polls == 1;

            if (isFirstDecaySampleOfWindow || decay < _window.MinDecay)
            {
                _window.MinDecay = decay;
            }

            TryReport();
        }

        public void OnFrameRendered(float frameDeltaTimeInSeconds)
        {
            _window.Frames++;

            if (frameDeltaTimeInSeconds > _window.WorstFrameTimeInSeconds)
            {
                _window.WorstFrameTimeInSeconds = frameDeltaTimeInSeconds;
            }

            // A frame that took longer than two ticks means the renderer missed a beat on its own, with or without the
            // network - that is the signature of local hitching rather than jitter.
            if (frameDeltaTimeInSeconds > _tickDeltaTime * 2f)
            {
                _window.FramesOverTwoTicks++;
            }
        }

        private void TryReport()
        {
            var now = Time.realtimeSinceStartup;
            var elapsedSeconds = now - _windowStartTime;
            if (elapsedSeconds < REPORT_INTERVAL_IN_SECONDS)
            {
                return;
            }

            LastReportText = BuildReport(elapsedSeconds);

            if (_isLoggingEnabled)
            {
                // Deliberately logged as an error so it survives into the player log file at default log settings,
                // where a friend reproducing the stutter can pull it out and send it over.
                LogService.LogError(LastReportText);
            }

            _window = default;
            _windowStartTime = now;
        }

        private string BuildReport(float elapsedSeconds)
        {
            var packetsPerSecond = _window.PacketsReceived / elapsedSeconds;
            var emptyPollPercent = _window.Polls > 0 ? _window.PollsWithNoPacket * 100f / _window.Polls : 0f;
            var discardedPackets = _window.PacketsReceived - _window.StatesProcessed;

            _reportBuilder.Clear();
            _reportBuilder.Append(REPORT_PREFIX);
            _reportBuilder.Append(" window ").Append(elapsedSeconds.ToString("0.0")).Append("s");
            _reportBuilder.Append(" | ping ").Append(_lastPingInMilliseconds).Append("ms");
            _reportBuilder.Append(" | packets ").Append(_window.PacketsReceived)
                .Append(" (").Append(packetsPerSecond.ToString("0.0")).Append("/s, expected ~").Append((1f / _tickDeltaTime).ToString("0")).Append("/s)");
            _reportBuilder.Append(" | polls ").Append(_window.Polls)
                .Append(": empty ").Append(_window.PollsWithNoPacket).Append(" (").Append(emptyPollPercent.ToString("0.0")).Append("%)")
                .Append(", multi ").Append(_window.PollsWithMultiplePackets)
                .Append(", max ").Append(_window.MaxPacketsInPoll);
            _reportBuilder.Append(" | tickGaps 1:").Append(_window.TickGap1)
                .Append(" 2:").Append(_window.TickGap2)
                .Append(" 3:").Append(_window.TickGap3)
                .Append(" 4-7:").Append(_window.TickGap4To7)
                .Append(" 8+:").Append(_window.TickGap8Plus)
                .Append(" max ").Append(_window.MaxTicksAdvanced);
            _reportBuilder.Append(" | discarded ").Append(discardedPackets);
            _reportBuilder.Append(" | decay now ").Append(_interpolationDecayService.CurrentDecay.ToString("0.0"))
                .Append(" min ").Append(_window.MinDecay.ToString("0.0"));
            _reportBuilder.Append(" | frames ").Append(_window.Frames)
                .Append(", worst ").Append((_window.WorstFrameTimeInSeconds * 1000f).ToString("0.0")).Append("ms")
                .Append(", over2ticks ").Append(_window.FramesOverTwoTicks);

            return _reportBuilder.ToString();
        }

        private struct Window
        {
            public int PacketsReceived;
            public int StatesProcessed;
            public int Polls;
            public int PollsWithNoPacket;
            public int PollsWithMultiplePackets;
            public int MaxPacketsInPoll;
            public int TickGap1;
            public int TickGap2;
            public int TickGap3;
            public int TickGap4To7;
            public int TickGap8Plus;
            public int MaxTicksAdvanced;
            public int Frames;
            public float WorstFrameTimeInSeconds;
            public int FramesOverTwoTicks;
            public float MinDecay;
        }
    }
}
