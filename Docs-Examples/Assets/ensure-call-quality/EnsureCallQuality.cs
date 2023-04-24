using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class EnsureCallQuality : GetStarted
{
    // Fill in your app ID.
    internal string appID = "";
    // Fill in your channel name.
    internal string channelName = "";
    // Fill in the temporary token you obtained from Agora Console.
    internal string token = "";
    void Start()
    {
        _appID = appID; 
        _channelName = channelName;
        _token = token;
        base.Start();
        CallQualityEventHandler eventHandler = new CallQualityEventHandler(agoraManager);
        RtcEngine.InitEventHandler(eventHandler);
        startProbeTest();
    }
    public void startProbeTest() 
    {
        // Configure a LastmileProbeConfig instance.
        LastmileProbeConfig config = new LastmileProbeConfig();
        // Probe the uplink network quality.
        config.probeUplink = true;
        // Probe the downlink network quality.
        config.probeDownlink = true;
        // The expected uplink bitrate (bps). The value range is [100000,5000000].
        config.expectedUplinkBitrate = 100000;
        // The expected downlink bitrate (bps). The value range is [100000,5000000].
        config.expectedDownlinkBitrate = 100000;
        RtcEngine.StartLastmileProbeTest(config);
        Debug.Log("Running the last mile probe test ...");
    }
}
// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class CallQualityEventHandler : UserEventHandler
{
    internal CallQualityEventHandler(AgoraManager videoSample):base(videoSample) {}
    public override void OnLastmileProbeResult(LastmileProbeResult result) 
    {
        _videoSample.RtcEngine.StopLastmileProbeTest();
        Debug.Log("Probe test finished");
        // The result object contains the detailed test results that help you
        // manage call quality, for example, the downlink jitter.
        Debug.Log("Downlink jitter: " + result.downlinkReport.jitter);
    }
} 