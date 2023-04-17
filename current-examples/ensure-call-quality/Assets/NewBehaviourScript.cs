using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif
    // Fill in your app ID.
    private string _appID = "";
    // Fill in your channel name.
    private string _channelName = "";
    // Fill in the temporary token you obtained from Agora Console.
    private string _token = "";
    // A variable to hold the user role.
    private string clientRole = "";
    // A variable to save the remote user uid.
    private uint remoteUid;
    private Toggle toggle1;
    private Toggle toggle2;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;
    private TMP_Text networkStatus; // A label to display the network quality.
    private TMP_Text videoQualityBtnText; // For changing the button text programmatically.
    private bool highQuality = false; // For switching between high and low video quality.

    // Start is called before the first frame update
    void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        SetupUI();
    }
    private void CheckPermissions() 
    {
        #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
        #endif
    }
    private void updateNetworkStatus(int quality)
    {  
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

    private void SetupUI()
    {
        GameObject go = GameObject.Find("LocalView");
        LocalView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
        go = GameObject.Find("RemoteView");
        RemoteView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
        go = GameObject.Find("Leave");
        go.GetComponent<Button>().onClick.AddListener(Leave);
        go = GameObject.Find("Join");
        go.GetComponent<Button>().onClick.AddListener(Join);
        GameObject Obj1 = GameObject.Find("Broadcaster");
        toggle1 = Obj1.GetComponent<Toggle>();
        toggle1.isOn = false;
        toggle1.onValueChanged.AddListener((value) =>
        {
            Func1(value);
        });
        GameObject Obj2 = GameObject.Find("Audience");
        toggle2 = Obj2.GetComponent<Toggle>();
        toggle2.isOn = false;
        toggle2.onValueChanged.AddListener((value) =>
        {
            Func2(value);
        });
        // Access the video quality button.
        go = GameObject.Find("videoQuality");
        // Access the label of the video quality button.
        videoQualityBtnText = go.GetComponentInChildren<TextMeshProUGUI>(true);
        // Change the video quality button text and font size.
        videoQualityBtnText.text = "Low Video Quality";
        videoQualityBtnText.fontSize = 14;
        // Set an event listener on the video quality button.
        go.GetComponent<Button>().onClick.AddListener(setStreamQuality);
        // Access the network quality label.
        networkStatus = GameObject.Find("networkStatus").GetComponent<TextMeshProUGUI>();
        // Change the label text and size.
        networkStatus.text  = "Network Quality:";
        networkStatus.fontSize = 14;
    }
    private void SetupVideoSDKEngine()
    {
        // Create an instance of the video SDK.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        // Specify the context configuration to initialize the created instance.
        RtcEngineContext context =  new RtcEngineContext(_appID, 0,
                                        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
                                        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB);
        // Initialize the created instance.
        RtcEngine.Initialize(context);
        // Specify a path for the log file.
        RtcEngine.SetLogFile("/path/to/folder/agorasdk1.log");
        // Set the log file size.
        RtcEngine.SetLogFileSize(256); // Range 128-20480 Kb
        // Specify a log level.
        RtcEngine.SetLogLevel(LOG_LEVEL.LOG_LEVEL_WARN);
        // Enable the dual stream mode
        RtcEngine.EnableDualStreamMode(true);
        // Set audio profile and audio scenario.
        RtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM);
        // Set the video profile
        VideoEncoderConfiguration videoConfig = new VideoEncoderConfiguration();
        // Set mirror mode
        videoConfig.mirrorMode = VIDEO_MIRROR_MODE_TYPE.VIDEO_MIRROR_MODE_DISABLED;
        // Set framerate
        videoConfig.frameRate = (int)FRAME_RATE.FRAME_RATE_FPS_15;
        // Set bitrate
        videoConfig.bitrate = (int)BITRATE.STANDARD_BITRATE;
        // Set dimensions
        videoConfig.dimensions =  new VideoDimensions((int)FRAME_WIDTH.FRAME_WIDTH_640, (int)FRAME_HEIGHT.FRAME_HEIGHT_360);
        // Set orientation mode
        videoConfig.orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE;
        // Set degradation preference
        videoConfig.degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_BALANCED;
        // Apply the configuration
        RtcEngine.SetVideoEncoderConfiguration(videoConfig);
        // Start the probe test
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

    public void Join()
    {
        if (toggle1.isOn == false && toggle2.isOn == false)
        {
            Debug.Log("Select a role first");
        }
        else
        {
            // Enable the video module.
            RtcEngine.EnableVideo();
            // Set the local video view.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
            // Join a channel.
            RtcEngine.JoinChannel(_token, _channelName);
        }
    }
    private void InitEventHandler()
    {
        // Creates a UserEventHandler instance.
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }
    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly NewBehaviourScript _videoSample;
        internal UserEventHandler(NewBehaviourScript videoSample)
        {
            _videoSample = videoSample;
        }
        // This callback is triggered when the local user joins the channel.
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("You joined channel: " +connection.channelId);
        }
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            // Setup remote view.
            _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            if(_videoSample.clientRole == "Audience")
            {
                // Start rendering remote video.
                _videoSample.RemoteView.SetEnable(true);
            }
            _videoSample.remoteUid = uid;
        }
        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {
            if (newRole == CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER)
            {
                _videoSample.LocalView.SetEnable(true);
                _videoSample.RemoteView.SetEnable(false);
                Debug.Log("Role changed to Broadcaster");
            }
            else
            {
                _videoSample.LocalView.SetEnable(false);
                _videoSample.RemoteView.SetEnable(true);
                Debug.Log("Role changed to Audience");
            }
        }
        // This callback is triggered when a remote user leaves the channel or drops offline.
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
        }
        public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason) 
        {
            Debug.Log("Connection state changed"
            + "\n New state: " + state
            + "\n Reason: " + reason);
        }
        public override void OnLastmileQuality(int quality) 
        {
            _videoSample.updateNetworkStatus(quality);
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
            _videoSample.updateNetworkStatus(rxQuality);
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
    void Func1(bool value)
    {
        if(value==true)
        {
            toggle2.isOn = false;
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            clientRole = "Host";
        }
    }
    void Func2(bool value)
    {
        if (value==true)
        {
            toggle1.isOn = false;
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
            clientRole = "Audience";
        }
    }
    public void Leave()
    {
        // Leaves the channel.
        RtcEngine.LeaveChannel();
        // Disable the video modules.
        RtcEngine.DisableVideo();
        // Stops rendering the remote video.
        RemoteView.SetEnable(false);
        // Stops rendering the local video.
        LocalView.SetEnable(false);
    }
    void Update()
    {
        CheckPermissions();
    }
    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            Leave();
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }
    public void setStreamQuality()
    {
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
