using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;

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
}
