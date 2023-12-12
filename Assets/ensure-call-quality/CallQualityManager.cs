using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using TMPro;
using System.Runtime.InteropServices;
using System;


public class CallQualityManager : AuthenticationWorkflowManager
{   
    private IAudioDeviceManager _audioDeviceManager; // To manage audio devices.
    private IVideoDeviceManager _videoDeviceManager; // To manage video devices.
    private DeviceInfo[] _audioRecordingDeviceInfos; // Represent information about audio recording devices.
    private DeviceInfo[] _videoDeviceInfos; // Represent information about video devices.

    public string networkStatus = "";
    public List<string> videoDevices;
    public List<string> audioDevices;

    [DllImport("user32.dll")]
    private static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName,
        uint dwStyle,
        int x, int y,
        int nWidth,
        int nHeight,
        IntPtr hWndParent,
        IntPtr hMenu,
        IntPtr hInstance,
        IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hWnd);

    private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;
    private const uint WS_VISIBLE = 0x10000000;
    private const int SW_SHOW = 5;
    private IntPtr hWnd;

    public CallQualityManager()
    {

        // Setup an instance of Agora engine.
        SetupAgoraEngine();

        // Get the list of audio recording devices connected to the user's app.
        GetAudioRecordingDevice();

        // Get the list of video devices connected to the user's app.
        GetVideoDeviceManager();

        // Start the probe test.
        StartProbeTest();
    }
 
    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Specify a path for the log file.
        agoraEngine.SetLogFile("/path/to/folder/agorasdk1.log");

        // Set the log file size.
        agoraEngine.SetLogFileSize(256); // Range 128-20480 Kb

        // Specify a log level.
        agoraEngine.SetLogLevel(LOG_LEVEL.LOG_LEVEL_WARN);

        // Enable the dual stream mode.
        agoraEngine.EnableDualStreamMode(true);

        // Set audio profile and audio scenario.
        agoraEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM);

        // Set the video profile.
        VideoEncoderConfiguration videoConfig = new VideoEncoderConfiguration();

        // Set mirror mode.
        videoConfig.mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED;

        // Set framerate.
        videoConfig.frameRate = (int)FRAME_RATE.FRAME_RATE_FPS_15;

        // Set bitrate.
        videoConfig.bitrate = (int)BITRATE.STANDARD_BITRATE;

        // Set dimensions.
        videoConfig.dimensions = new VideoDimensions(640, 360);

        // Set orientation mode.
        videoConfig.orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE;

        // Set degradation preference.
        videoConfig.degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_BALANCED;

        // Set the latency level
        videoConfig.advanceOptions.compressionPreference = COMPRESSION_PREFERENCE.PREFER_LOW_LATENCY;

        // Apply the configuration.
        agoraEngine.SetVideoEncoderConfiguration(videoConfig);

        // Attach the eventHandler
        agoraEngine.InitEventHandler(new CallQualityEventHandler(this));
    }

    // Probe test to check network quality.
    public void StartProbeTest()
    {
        // Configure a LastmileProbeConfig instance.
        LastmileProbeConfig config = new LastmileProbeConfig();

        // Probe theuplink network quality.
        config.probeUplink = true;

        // Probe the downlink network quality.
        config.probeDownlink = true;

        // The expected uplink bitrate (bps). The value range is [100000,5000000].
        config.expectedUplinkBitrate = 100000;

        // The expected downlink bitrate (bps). The value range is [100000,5000000].
        config.expectedDownlinkBitrate = 100000;

        agoraEngine.StartLastmileProbeTest(config);
        Debug.Log("Running the last mile probe test ...");
    }

    // Get the list of available audio devices.
    private void GetAudioRecordingDevice()
    {
        _audioDeviceManager = agoraEngine.GetAudioDeviceManager();
        _audioRecordingDeviceInfos = _audioDeviceManager.EnumerateRecordingDevices();
        audioDevices = new List<string>();

        for (var i = 0; i < _audioRecordingDeviceInfos.Length; i++)
        {
            Debug.Log(string.Format("AudioRecordingDevice device index: {0}, name: {1}, id: {2}", i,
                _audioRecordingDeviceInfos[i].deviceName, _audioRecordingDeviceInfos[i].deviceId));
            audioDevices.Add(_audioRecordingDeviceInfos[i].deviceName);
        }
    }

    // Get the list of available video devices.
    private void GetVideoDeviceManager()
    {
        _videoDeviceManager = agoraEngine.GetVideoDeviceManager();
        _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();

        videoDevices = new List<string>();
        for (var i = 0; i < _videoDeviceInfos.Length; i++)
        {
            Debug.Log(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            videoDevices.Add(_videoDeviceInfos[i].deviceName);
        }
    }

    // Device test to check if the audio and video device is working properly. Only valid before joining the channel.
    public void StartAudioVideoDeviceTest(string selectedAudioDevice, string selectedVideoDevice)
    {
        Debug.Log("Please conduct the device test before joining the channel.");
        SetupAgoraEngine();
        foreach (var device in _audioRecordingDeviceInfos)
        {
            if(selectedAudioDevice == device.deviceName)
            {
                _audioDeviceManager.SetRecordingDevice(device.deviceId);
            }
        }
        _audioDeviceManager.StartAudioDeviceLoopbackTest(500);
        foreach (var device in _videoDeviceInfos)
        {
            if(selectedVideoDevice == device.deviceName)
            {
                _videoDeviceManager.SetDevice(device.deviceId);
            }
        }
        hWnd = CreateWindowEx(
                0,
                "Static",
                "My Window",
                WS_OVERLAPPEDWINDOW | WS_VISIBLE,
                100,
                100,
                640,
                480,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);
            ShowWindow(hWnd, SW_SHOW);
            _videoDeviceManager.StartDeviceTest(hWnd);
    }

    public void StopAudioVideoDeviceTest()
    {
        DestroyWindow(hWnd);
        _audioDeviceManager.StopAudioDeviceLoopbackTest();
        _videoDeviceManager.StopDeviceTest();
        DestroyEngine();
    }

    public void updateNetworkStatus(int quality)
    {  
        if (quality > 0 && quality < 3) 
        {
            networkStatus = "Network Quality: Perfect";
        }
        else if (quality <= 4) 
        {
            networkStatus = "Network Quality: Good";
        }
        else if (quality <= 6) 
        {
            networkStatus = "Network Quality: Poor";
        }
    }

    // Switch between high and low remote user video quality.
    public void SetLowStreamQuality()
    {
        if(remoteUid > 1)
        {
            agoraEngine.SetRemoteVideoStreamType(remoteUid, VIDEO_STREAM_TYPE.VIDEO_STREAM_LOW);
            Debug.Log("Switching to low-quality video");
        }
        else
        {
            Debug.Log("No remote user in the channel");
        }
    }
    public void SetHighStreamQuality()
    {
        if (remoteUid > 1)
        {
            agoraEngine.SetRemoteVideoStreamType(remoteUid, VIDEO_STREAM_TYPE.VIDEO_STREAM_HIGH);
            Debug.Log("Switching to high-quality video");
        }
        else
        {
            Debug.Log("No remote user in the channel");
        }
    }
}
    
// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class CallQualityEventHandler : UserEventHandler
{
    private CallQualityManager callQuality;
    internal CallQualityEventHandler(CallQualityManager videoSample):base(videoSample) 
    {
        callQuality = videoSample;
    }
    public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason) 
    {
        Debug.Log("Connection state changed"
             + "\n New state: " + state
             + "\n Reason: " + reason);
    }
    public override void OnLastmileQuality(int quality) 
    {
        callQuality.updateNetworkStatus(quality);
    }
    public override void OnLastmileProbeResult(LastmileProbeResult result) 
    {
        callQuality.agoraEngine.StopLastmileProbeTest();
       
        Debug.Log("Probe test finished");
        // The result object contains the detailed test results that help you
        // manage call quality, for example, the downlink jitter.
        Debug.Log("Downlink jitter: " + result.downlinkReport.jitter);

        //Destroy the engine
        callQuality.DestroyEngine();

    }
    public override void OnNetworkQuality(RtcConnection connection, uint remoteUid, int txQuality, int rxQuality) 
    {
        // Use downlink network quality to update the network status
        callQuality.updateNetworkStatus(rxQuality);
    }
    public override void OnRtcStats(RtcConnection connection, RtcStats rtcStats) 
    {
        string msg = "";
        msg = rtcStats.userCount + " user(s)";
        msg = "Packet loss rate: " + rtcStats.rxPacketLossRate;
        Debug.Log(msg);
    }
    public override void OnRemoteVideoStateChanged(RtcConnection connection, uint remoteUid, REMOTE_VIDEO_STATE state, REMOTE_VIDEO_STATE_REASON reason, int elapsed) 
    {
        string msg = "Remote video state changed: \n Uid =" + remoteUid
             + " \n NewState =" + state
             + " \n reason =" + reason
             + " \n elapsed =" + elapsed;
        Debug.Log(msg);
    }
    public override void OnRemoteVideoStats(RtcConnection connection, RemoteVideoStats stats) 
    {
        string msg = "Remote Video Stats: "
             + "\n User id =" + stats.uid
             + "\n Received bitrate =" + stats.receivedBitrate
             + "\n Total frozen time =" + stats.totalFrozenTime;
        Debug.Log(msg);
    }
    
} 