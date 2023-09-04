using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using TMPro;
using System.Runtime.InteropServices;
using System;


public class CallQualityManager : AuthenticationWorkflowManager
{   
    private TMP_Text networkStatus; // A label to display the network quality.
    private bool highQuality = false; // For switching between high and low video quality.
    private IAudioDeviceManager _audioDeviceManager; // To manage audio devices.
    private IVideoDeviceManager _videoDeviceManager; // To manage video devices.
    private DeviceInfo[] _audioRecordingDeviceInfos; // Represent information about audio recording devices.
    private DeviceInfo[] _audioPlaybackDeviceInfos; // Represent information about audio playback devices.
    private DeviceInfo[] _videoDeviceInfos; // Represent information about video devices.
    private TMP_Dropdown videoDevicesDropdown; // To access the video devices dropdown.
    private TMP_Dropdown audioDevicesDropdown; // To access the audio devices dropdown.
    private bool isTestRunning = false; // To track the device test state.

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

    // Start is called before the first frame update
    public CallQualityManager(GameObject LocalViewGo, GameObject RemoteViewGo): base(LocalViewGo, RemoteViewGo)
    {
        // Check if the required permissions are granted
        CheckPermissions();

        // Setup an instance of Agora engine.
        SetupAgoraEngine();

        // Setup an event handler to receive callbacks.
        RtcEngine.InitEventHandler(new CallQualityEventHandler(this));

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
        RtcEngine.SetLogFile("/path/to/folder/agorasdk1.log");

        // Set the log file size.
        RtcEngine.SetLogFileSize(256); // Range 128-20480 Kb

        // Specify a log level.
        RtcEngine.SetLogLevel(LOG_LEVEL.LOG_LEVEL_WARN);

        // Enable the dual stream mode.
        RtcEngine.EnableDualStreamMode(true);

        // Set audio profile and audio scenario.
        RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM);

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
        RtcEngine.SetVideoEncoderConfiguration(videoConfig);
        
    }
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

        RtcEngine.StartLastmileProbeTest(config);
        Debug.Log("Running the last mile probe test ...");
    }

    private void GetAudioRecordingDevice()
    {
        _audioDeviceManager = RtcEngine.GetAudioDeviceManager();
        _audioRecordingDeviceInfos = _audioDeviceManager.EnumerateRecordingDevices();

        audioDevicesDropdown = GameObject.Find("audioDevicesDropdown").GetComponent<TMP_Dropdown>();
        List<string> options = new List<string>();

        for (var i = 0; i < _audioRecordingDeviceInfos.Length; i++)
        {
            Debug.Log(string.Format("AudioRecordingDevice device index: {0}, name: {1}, id: {2}", i,
                _audioRecordingDeviceInfos[i].deviceName, _audioRecordingDeviceInfos[i].deviceId));
            options.Add(_audioRecordingDeviceInfos[i].deviceName);
        }

        audioDevicesDropdown.ClearOptions();
        audioDevicesDropdown.AddOptions(options);
    }

    private void GetVideoDeviceManager()
    {
        _videoDeviceManager = RtcEngine.GetVideoDeviceManager();
        _videoDeviceInfos = _videoDeviceManager.EnumerateVideoDevices();

        videoDevicesDropdown = GameObject.Find("videoDevicesDropdown").GetComponent<TMP_Dropdown>();
        videoDevicesDropdown.transform.localPosition = new Vector2(-23, -172);

        Debug.Log(string.Format("VideoDeviceManager count: {0}", _videoDeviceInfos.Length));
        List<string> options = new List<string>();

        for (var i = 0; i < _videoDeviceInfos.Length; i++)
        {
            Debug.Log(string.Format("VideoDeviceManager device index: {0}, name: {1}, id: {2}", i,
                _videoDeviceInfos[i].deviceName, _videoDeviceInfos[i].deviceId));
            options.Add(_videoDeviceInfos[i].deviceName);
        }

        videoDevicesDropdown.ClearOptions();
        videoDevicesDropdown.AddOptions(options);
    }
    public void testAudioAndVideoDevice()
    {
        GameObject go = GameObject.Find("testDevicesBtn");
        if(!isTestRunning)
        {
            string selectedAudioDevice = audioDevicesDropdown.options[audioDevicesDropdown.value].text;
            string selectedVideoDevice = videoDevicesDropdown.options[videoDevicesDropdown.value].text;
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
                Marshal.GetHINSTANCE(typeof(EnsureCallQuality).Module),
                IntPtr.Zero);
                ShowWindow(hWnd, SW_SHOW);
                _videoDeviceManager.StartDeviceTest(hWnd);
                isTestRunning = true;
                go.GetComponentInChildren<TextMeshProUGUI>(true).text = "Stop test";
        }
        else
        {
            DestroyWindow(hWnd);
            isTestRunning = false;
            go.GetComponentInChildren<TextMeshProUGUI>(true).text = "Start device test";
            _audioDeviceManager.StopAudioDeviceLoopbackTest();
            _videoDeviceManager.StopDeviceTest();
        }
    }
    public void updateNetworkStatus(int quality)
    {  
        TMP_Text networkStatus = GameObject.Find("networkStatus").GetComponent<TextMeshProUGUI>();
        if (quality > 0 && quality < 3) 
        {
            networkStatus.text = "Network Quality: Perfect";
            networkStatus.color = Color.green;
        }
        else if (quality <= 4) 
        {
            networkStatus.text = "Network Quality: Good";
            networkStatus.color = Color.yellow;
        }
        else if (quality <= 6) 
        {
            networkStatus.text = "Network Quality: Poor";
            networkStatus.color = Color.red;
        }
        else 
        {
            networkStatus.color = Color.white;
        }
    }
    public void setStreamQuality()
    {
        GameObject videoQualityBtn = GameObject.Find("videoQualityBtn");
        TMP_Text videoQualityBtnText = videoQualityBtn.GetComponentInChildren<TextMeshProUGUI>(true); // For changing the button text programmatically.
        if(remoteUid == 0)
        {
            Debug.Log("No remote user in the channel");
            return;
        }
        highQuality = !highQuality;
        if (highQuality) 
        {
            RtcEngine.SetRemoteVideoStreamType(remoteUid, VIDEO_STREAM_TYPE.VIDEO_STREAM_HIGH);
            videoQualityBtnText.text = "Low Video Quality";
            Debug.Log("Switching to high-quality video");
        } 
        else 
        {
            RtcEngine.SetRemoteVideoStreamType(remoteUid, VIDEO_STREAM_TYPE.VIDEO_STREAM_LOW);
            Debug.Log("Switching to low-quality video");
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
        _videoSample.RtcEngine.StopLastmileProbeTest();
       
        Debug.Log("Probe test finished");
        // The result object contains the detailed test results that help you
        // manage call quality, for example, the downlink jitter.
        Debug.Log("Downlink jitter: " + result.downlinkReport.jitter);
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