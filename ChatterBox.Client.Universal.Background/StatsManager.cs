using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using webrtc_winrt_api;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Threading;

namespace ChatterBox.Client.Universal.Background
{
    public sealed class StatsManager
    {
        public StatsManager()
        {
            _telemetry = new TelemetryClient();
            _telemetry.Context.Operation.Id = Guid.NewGuid().ToString();
        }

        RTCPeerConnection _peerConnection;
        TelemetryClient _telemetry;
        Timer _metricsTimer;
        AudioVideoMetricsCollector _metricsCollector;
        public void Initialize(RTCPeerConnection pc)
        {
            if (pc != null)
            {
                _peerConnection = pc;
                _peerConnection.OnRTCStatsReportsReady += PeerConnection_OnRTCStatsReportsReady;
            }
            else
            {
                Debug.WriteLine("StatsManager: Cannot initialize peer connection by null pointer");
            }
        }

        public void Reset()
        {
            if (_peerConnection != null)
            {
                _peerConnection.ToggleRTCStats(false);
                _peerConnection = null;
            }
            if(_metricsTimer != null) {
                _metricsTimer.Dispose();
            }
        }

        private bool _isStatsCollectionEnabled;
        public bool IsStatsCollectionEnabled
        {
            get { return _isStatsCollectionEnabled; }
            set
            {
                _isStatsCollectionEnabled = value;
                if (_peerConnection != null)
                {
                    _peerConnection.ToggleRTCStats(value);
                    if (_isStatsCollectionEnabled)
                    {
                        AutoResetEvent autoEvent = new AutoResetEvent(false);
                        _metricsCollector = new AudioVideoMetricsCollector(_telemetry);
                        TimerCallback tcb = _metricsCollector.TrackMetrics;
                        _metricsTimer = new Timer(tcb, autoEvent, 60000, 60000);
                    }
                    else {
                        Reset();
                    }
                }
                else
                {
                    Debug.WriteLine("StatsManager: Stats are not toggled as manager is not initialized yet.");
                }
            }
        }

        private void ProcessReports(IList<RTCStatsReport> reports)
        {
            foreach (var report in reports)
            {
                if (report.StatsType == RTCStatsType.StatsReportTypeSsrc) {
                    IDictionary<RTCStatsValueName, Object> statValues = report.Values;
                    if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNameTrackId))
                    {
                        string trackId = statValues[RTCStatsValueName.StatsValueNameTrackId].ToString();
                        if (trackId == "audio_label")
                        {
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNamePacketsSent)) {
                                _metricsCollector._audioPacketsSent += Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNamePacketsSent]);
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNamePacketsLost))
                            {
                                _metricsCollector._audioPacketsLost += Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNamePacketsLost]);
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNameCurrentDelayMs))
                            {
                                _metricsCollector._audioCurrentDelayMs += Convert.ToDouble(statValues[RTCStatsValueName.StatsValueNameCurrentDelayMs]);
                                _metricsCollector._audioDelayCount++;
                            }
                        }
                        else if (trackId == "video_label")
                        {
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNamePacketsSent))
                            {
                                _metricsCollector._videoPacketsSent += Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNamePacketsSent]);
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNamePacketsLost))
                            {
                                _metricsCollector._videoPacketsLost += Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNamePacketsLost]);
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNameCurrentDelayMs))
                            {
                                _metricsCollector._videoCurrentDelayMs += Convert.ToDouble(statValues[RTCStatsValueName.StatsValueNameCurrentDelayMs]);
                                _metricsCollector._videoDelayCount++;
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNameFrameHeightSent))
                            {
                                _metricsCollector.FrameHeight = Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNameFrameHeightSent]);
                            }
                            if (statValues.Keys.Contains(RTCStatsValueName.StatsValueNameFrameWidthSent))
                            {
                                _metricsCollector.FrameWidth = Convert.ToInt32(statValues[RTCStatsValueName.StatsValueNameFrameWidthSent]);
                            }
                        }
                    }
                }
            }
        }

        private void PeerConnection_OnRTCStatsReportsReady(RTCStatsReportsReadyEvent evt)
        {
                IList<RTCStatsReport> reports = evt.rtcStatsReports;
                Task.Run(()=> ProcessReports(reports));
        }

        private string ToMetricName(RTCStatsValueName name)
        {
            switch (name)
            {
                case RTCStatsValueName.StatsValueNameAudioOutputLevel:
                    return "audioOutputLevel";
                case RTCStatsValueName.StatsValueNameAudioInputLevel:
                    return "audioInputLevel";
                case RTCStatsValueName.StatsValueNameBytesSent:
                    return "bytesSent";
                case RTCStatsValueName.StatsValueNamePacketsSent:
                    return "packetsSent";
                case RTCStatsValueName.StatsValueNameBytesReceived:
                    return "bytesReceived";
                case RTCStatsValueName.StatsValueNameLabel:
                    return "label";
                case RTCStatsValueName.StatsValueNamePacketsReceived:
                    return "packetsReceived";
                case RTCStatsValueName.StatsValueNamePacketsLost:
                    return "packetsLost";
                case RTCStatsValueName.StatsValueNameProtocol:
                    return "protocol";
                case RTCStatsValueName.StatsValueNameTransportId:
                    return "transportId";
                case RTCStatsValueName.StatsValueNameSelectedCandidatePairId:
                    return "selectedCandidatePairId";
                case RTCStatsValueName.StatsValueNameSsrc:
                    return "ssrc";
                case RTCStatsValueName.StatsValueNameState:
                    return "state";
                case RTCStatsValueName.StatsValueNameDataChannelId:
                    return "datachannelid";

                // 'goog' prefixed constants.
                case RTCStatsValueName.StatsValueNameAccelerateRate:
                    return "googAccelerateRate";
                case RTCStatsValueName.StatsValueNameActiveConnection:
                    return "googActiveConnection";
                case RTCStatsValueName.StatsValueNameActualEncBitrate:
                    return "googActualEncBitrate";
                case RTCStatsValueName.StatsValueNameAvailableReceiveBandwidth:
                    return "googAvailableReceiveBandwidth";
                case RTCStatsValueName.StatsValueNameAvailableSendBandwidth:
                    return "googAvailableSendBandwidth";
                case RTCStatsValueName.StatsValueNameAvgEncodeMs:
                    return "googAvgEncodeMs";
                case RTCStatsValueName.StatsValueNameBucketDelay:
                    return "googBucketDelay";
                case RTCStatsValueName.StatsValueNameBandwidthLimitedResolution:
                    return "googBandwidthLimitedResolution";

                // Candidate related attributes. Values are taken from
                // http://w3c.github.io/webrtc-stats/#rtcstatstype-enum*.
                case RTCStatsValueName.StatsValueNameCandidateIPAddress:
                    return "ipAddress";
                case RTCStatsValueName.StatsValueNameCandidateNetworkType:
                    return "networkType";
                case RTCStatsValueName.StatsValueNameCandidatePortNumber:
                    return "portNumber";
                case RTCStatsValueName.StatsValueNameCandidatePriority:
                    return "priority";
                case RTCStatsValueName.StatsValueNameCandidateTransportType:
                    return "transport";
                case RTCStatsValueName.StatsValueNameCandidateType:
                    return "candidateType";

                case RTCStatsValueName.StatsValueNameChannelId:
                    return "googChannelId";
                case RTCStatsValueName.StatsValueNameCodecName:
                    return "googCodecName";
                case RTCStatsValueName.StatsValueNameComponent:
                    return "googComponent";
                case RTCStatsValueName.StatsValueNameContentName:
                    return "googContentName";
                case RTCStatsValueName.StatsValueNameCpuLimitedResolution:
                    return "googCpuLimitedResolution";
                case RTCStatsValueName.StatsValueNameDecodingCTSG:
                    return "googDecodingCTSG";
                case RTCStatsValueName.StatsValueNameDecodingCTN:
                    return "googDecodingCTN";
                case RTCStatsValueName.StatsValueNameDecodingNormal:
                    return "googDecodingNormal";
                case RTCStatsValueName.StatsValueNameDecodingPLC:
                    return "googDecodingPLC";
                case RTCStatsValueName.StatsValueNameDecodingCNG:
                    return "googDecodingCNG";
                case RTCStatsValueName.StatsValueNameDecodingPLCCNG:
                    return "googDecodingPLCCNG";
                case RTCStatsValueName.StatsValueNameDer:
                    return "googDerBase64";
                case RTCStatsValueName.StatsValueNameDtlsCipher:
                    return "dtlsCipher";
                case RTCStatsValueName.StatsValueNameEchoCancellationQualityMin:
                    return "googEchoCancellationQualityMin";
                case RTCStatsValueName.StatsValueNameEchoDelayMedian:
                    return "googEchoCancellationEchoDelayMedian";
                case RTCStatsValueName.StatsValueNameEchoDelayStdDev:
                    return "googEchoCancellationEchoDelayStdDev";
                case RTCStatsValueName.StatsValueNameEchoReturnLoss:
                    return "googEchoCancellationReturnLoss";
                case RTCStatsValueName.StatsValueNameEchoReturnLossEnhancement:
                    return "googEchoCancellationReturnLossEnhancement";
                case RTCStatsValueName.StatsValueNameEncodeUsagePercent:
                    return "googEncodeUsagePercent";
                case RTCStatsValueName.StatsValueNameExpandRate:
                    return "googExpandRate";
                case RTCStatsValueName.StatsValueNameFingerprint:
                    return "googFingerprint";
                case RTCStatsValueName.StatsValueNameFingerprintAlgorithm:
                    return "googFingerprintAlgorithm";
                case RTCStatsValueName.StatsValueNameFirsReceived:
                    return "googFirsReceived";
                case RTCStatsValueName.StatsValueNameFirsSent:
                    return "googFirsSent";
                case RTCStatsValueName.StatsValueNameFrameHeightInput:
                    return "googFrameHeightInput";
                case RTCStatsValueName.StatsValueNameFrameHeightReceived:
                    return "googFrameHeightReceived";
                case RTCStatsValueName.StatsValueNameFrameHeightSent:
                    return "googFrameHeightSent";
                case RTCStatsValueName.StatsValueNameFrameRateReceived:
                    return "googFrameRateReceived";
                case RTCStatsValueName.StatsValueNameFrameRateDecoded:
                    return "googFrameRateDecoded";
                case RTCStatsValueName.StatsValueNameFrameRateOutput:
                    return "googFrameRateOutput";
                case RTCStatsValueName.StatsValueNameDecodeMs:
                    return "googDecodeMs";
                case RTCStatsValueName.StatsValueNameMaxDecodeMs:
                    return "googMaxDecodeMs";
                case RTCStatsValueName.StatsValueNameCurrentDelayMs:
                    return "googCurrentDelayMs";
                case RTCStatsValueName.StatsValueNameTargetDelayMs:
                    return "googTargetDelayMs";
                case RTCStatsValueName.StatsValueNameJitterBufferMs:
                    return "googJitterBufferMs";
                case RTCStatsValueName.StatsValueNameMinPlayoutDelayMs:
                    return "googMinPlayoutDelayMs";
                case RTCStatsValueName.StatsValueNameRenderDelayMs:
                    return "googRenderDelayMs";
                case RTCStatsValueName.StatsValueNameCaptureStartNtpTimeMs:
                    return "googCaptureStartNtpTimeMs";
                case RTCStatsValueName.StatsValueNameFrameRateInput:
                    return "googFrameRateInput";
                case RTCStatsValueName.StatsValueNameFrameRateSent:
                    return "googFrameRateSent";
                case RTCStatsValueName.StatsValueNameFrameWidthInput:
                    return "googFrameWidthInput";
                case RTCStatsValueName.StatsValueNameFrameWidthReceived:
                    return "googFrameWidthReceived";
                case RTCStatsValueName.StatsValueNameFrameWidthSent:
                    return "googFrameWidthSent";
                case RTCStatsValueName.StatsValueNameInitiator:
                    return "googInitiator";
                case RTCStatsValueName.StatsValueNameIssuerId:
                    return "googIssuerId";
                case RTCStatsValueName.StatsValueNameJitterReceived:
                    return "googJitterReceived";
                case RTCStatsValueName.StatsValueNameLocalAddress:
                    return "googLocalAddress";
                case RTCStatsValueName.StatsValueNameLocalCandidateId:
                    return "localCandidateId";
                case RTCStatsValueName.StatsValueNameLocalCandidateType:
                    return "googLocalCandidateType";
                case RTCStatsValueName.StatsValueNameLocalCertificateId:
                    return "localCertificateId";
                case RTCStatsValueName.StatsValueNameAdaptationChanges:
                    return "googAdaptationChanges";
                case RTCStatsValueName.StatsValueNameNacksReceived:
                    return "googNacksReceived";
                case RTCStatsValueName.StatsValueNameNacksSent:
                    return "googNacksSent";
                case RTCStatsValueName.StatsValueNamePreemptiveExpandRate:
                    return "googPreemptiveExpandRate";
                case RTCStatsValueName.StatsValueNamePlisReceived:
                    return "googPlisReceived";
                case RTCStatsValueName.StatsValueNamePlisSent:
                    return "googPlisSent";
                case RTCStatsValueName.StatsValueNamePreferredJitterBufferMs:
                    return "googPreferredJitterBufferMs";
                case RTCStatsValueName.StatsValueNameReadable:
                    return "googReadable";
                case RTCStatsValueName.StatsValueNameRemoteAddress:
                    return "googRemoteAddress";
                case RTCStatsValueName.StatsValueNameRemoteCandidateId:
                    return "remoteCandidateId";
                case RTCStatsValueName.StatsValueNameRemoteCandidateType:
                    return "googRemoteCandidateType";
                case RTCStatsValueName.StatsValueNameRemoteCertificateId:
                    return "remoteCertificateId";
                case RTCStatsValueName.StatsValueNameRetransmitBitrate:
                    return "googRetransmitBitrate";
                case RTCStatsValueName.StatsValueNameRtt:
                    return "googRtt";
                case RTCStatsValueName.StatsValueNameSecondaryDecodedRate:
                    return "googSecondaryDecodedRate";
                case RTCStatsValueName.StatsValueNameSendPacketsDiscarded:
                    return "packetsDiscardedOnSend";
                case RTCStatsValueName.StatsValueNameSpeechExpandRate:
                    return "googSpeechExpandRate";
                case RTCStatsValueName.StatsValueNameSrtpCipher:
                    return "srtpCipher";
                case RTCStatsValueName.StatsValueNameTargetEncBitrate:
                    return "googTargetEncBitrate";
                case RTCStatsValueName.StatsValueNameTransmitBitrate:
                    return "googTransmitBitrate";
                case RTCStatsValueName.StatsValueNameTransportType:
                    return "googTransportType";
                case RTCStatsValueName.StatsValueNameTrackId:
                    return "googTrackId";
                case RTCStatsValueName.StatsValueNameTypingNoiseState:
                    return "googTypingNoiseState";
                case RTCStatsValueName.StatsValueNameViewLimitedResolution:
                    return "googViewLimitedResolution";
                case RTCStatsValueName.StatsValueNameWritable:
                    return "googWritable";
                case RTCStatsValueName.StatsValueNameCurrentEndToEndDelayMs:
                    return "currentEndToEndDelayMs";
                default:
                    return String.Empty;
            }
        }

        public void TrackEvent(String name)
        {
            Task.Run(() => _telemetry.TrackEvent(name));
        }

        public void TrackEvent(String name, IDictionary<string, string> props)
        {
            if (props == null)
            {
                Task.Run(() => _telemetry.TrackEvent(name));
            }
            else
            {
                props.Add("Timestamp", System.DateTimeOffset.UtcNow.ToString(@"hh\:mm\:ss"));
                Task.Run(() => _telemetry.TrackEvent(name, props));
            }
        }

        public void TrackMetric(String name, double value) {
            MetricTelemetry metric = new MetricTelemetry(name, value);
            metric.Timestamp = System.DateTimeOffset.UtcNow;
            Task.Run(() => _telemetry.TrackMetric(metric));
        }

        private Stopwatch _callWatch;
        public void StartCallWatch()
        {
            _telemetry.Context.Operation.Name = "Call Duration tracking";

            _callWatch = Stopwatch.StartNew();
        }

        public void StopCallWatch()
        {
            _callWatch.Stop();
            DateTime currentDateTime = DateTime.Now;
            TimeSpan time = _callWatch.Elapsed;
            Task.Run(() => _telemetry.TrackRequest("Call Duration", currentDateTime,
               time,
               "200", true));  // Response code, success
            _metricsCollector.TrackCurrentDelayMetrics();
        }
    }

    class AudioVideoMetricsCollector
    {
        private TelemetryClient _telemetry;
        public int _audioPacketsSent;
        public int _audioPacketsLost;
        public int _videoPacketsSent;
        public int _videoPacketsLost;
        public double _audioCurrentDelayMs;
        public int _audioDelayCount;
        public double _videoCurrentDelayMs;
        public int _videoDelayCount;
        private  int _frameHeight;
        public int FrameHeight
        {
            get { return _frameHeight; }
            set
            {
                if (_frameHeight > value)
                {
                    TrackVideoResolutionDowngrade(_frameHeight, value, "Height");
                }
                _frameHeight = value;
            }
        }

        private int _frameWidth;
        public int FrameWidth
        {
            get { return _frameWidth; }
            set
            {
                if (_frameWidth > value)
                {
                    TrackVideoResolutionDowngrade(_frameWidth, value, "Width");
                }
                _frameWidth = value;
            }
        }

        public AudioVideoMetricsCollector(TelemetryClient tc)
        {
            _telemetry = tc;
            ResetPackets();
            ResetDelays();
            _frameHeight = 0;
            _frameWidth = 0;
        }

        private void ResetPackets()
        {
            _audioPacketsSent = 0;
            _audioPacketsLost = 0;
            _videoPacketsSent = 0;
            _videoPacketsLost = 0;
        }
        private void ResetDelays()
        {
            _audioCurrentDelayMs = 0;
            _audioDelayCount = 0;
            _videoCurrentDelayMs = 0;
            _videoDelayCount = 0;
        }

        public void TrackMetrics(Object state)
        {
            double audioPacketRatio = (_audioPacketsSent != 0) ? (double) _audioPacketsLost / _audioPacketsSent : 0;
            double videoPacketRatio = (_videoPacketsSent != 0) ? (double) _videoPacketsLost / _videoPacketsSent : 0;

            if (_audioPacketsSent > 0) {
                MetricTelemetry metric = new MetricTelemetry("Audio Packet Lost Ratio", audioPacketRatio);
                metric.Timestamp = System.DateTimeOffset.UtcNow;
                Task.Run(() => _telemetry.TrackMetric(metric));
            }

            if (_videoPacketsSent > 0)
            {
                MetricTelemetry metric = new MetricTelemetry("Video Packet Lost Ratio", videoPacketRatio);
                metric.Timestamp = System.DateTimeOffset.UtcNow;
                Task.Run(() => _telemetry.TrackMetric(metric));
            }

            // reset flags for the new time period
            ResetPackets();
        }

        public void TrackCurrentDelayMetrics()
        {
            if (_audioDelayCount > 0)
            {
                MetricTelemetry metric = new MetricTelemetry("Audio Current Delay Ratio", _audioCurrentDelayMs / _audioDelayCount);
                metric.Timestamp = System.DateTimeOffset.UtcNow;
                Task.Run(() => _telemetry.TrackMetric(metric));
            }
            if (_videoDelayCount > 0)
            {
                MetricTelemetry metric = new MetricTelemetry("Video Current Delay Ratio", _videoCurrentDelayMs / _videoDelayCount);
                metric.Timestamp = System.DateTimeOffset.UtcNow;
                Task.Run(() => _telemetry.TrackMetric(metric));
            }
            ResetDelays();
        }

        private void TrackVideoResolutionDowngrade(int oldValue, int newValue, string name)
        {
            IDictionary<string, string> properties = new Dictionary<string, string> {
                { "Timestamp", System.DateTimeOffset.UtcNow.ToString(@"hh\:mm\:ss") } };
            IDictionary<string, double> metrics = new Dictionary<string, double> {
                { "Old " + name, oldValue},
                { "New " + name, newValue} };
            Task.Run(() => _telemetry.TrackEvent("Video " + name + " Downgrade", properties, metrics));
        }
    }

   
}
