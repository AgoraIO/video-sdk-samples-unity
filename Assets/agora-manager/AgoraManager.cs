using UnityEngine;
using Agora.Rtc;
using System;
using System.IO;


[Serializable]
public class ConfigData
{
    public string appID;
    public string channelName;
    public string rtcToken;
    public string encryptionKey = "";
    public string salt = "";
    public int tokenExpiryTime = 3600; // Default time of 1 hour
    public string tokenUrl = ""; // Add Token Generator URL ...
    public uint uid  = 0; // RTC elected user ID (0 = choose random)
    public string product;
    public string soundEffectFileURL;
    public string audioFileURL;
    public string videoFileURL;
    public string destChannelName;
    public string destToken;
    public uint destUID;
    public string secondChannelName;
    public string secondChannelToken;
    public uint secondChannelUID;
}

public class AgoraManager
{
    // Define some variables to be used later.
    internal string _appID;
    internal string _channelName;
    internal string _token;
    internal uint remoteUid;
    internal IRtcEngine agoraEngine;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal ConfigData configData;
    internal AREA_CODE region = AREA_CODE.AREA_CODE_GLOB;
    internal string userRole = "";

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    // Define an ArrayList of permissions required for Android devices.
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    // Define a private function called CheckPermissions() to check for required permissions.
    public void CheckPermissions()
    {
        #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        // Check for each permission in the permission list and request the user to grant it if necessary.
        foreach (string permission in permissionList)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                Permission.RequestUserPermission(permission);
            }
        }
        #endif
    }

    public AgoraManager()
    {
        // Load the required configuration parameters from config.json
        LoadConfigFromJSON();

        // Check if the required permissions are granted
        CheckPermissions();
    }

    private void LoadConfigFromJSON()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "agora-manager", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);

            _appID = configData.appID;
            _channelName = configData.channelName;
            _token = configData.rtcToken;

        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }

    // Define a public function called SetupAgoraEngine to setup the video SDK engine.
    public virtual void SetupAgoraEngine()
    {
        if(_appID == "")
        {
            Debug.Log("Please specify an app ID in the config file.");
            return;
        }
        // Create an instance of the video SDK engine.
        agoraEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

        // Set context configuration based on the product type
        CHANNEL_PROFILE_TYPE channelProfile = configData.product == "Video Calling"
            ? CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION
            : CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;

        RtcEngineContext context = new RtcEngineContext(_appID, 0, channelProfile,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, region, null);

        agoraEngine.Initialize(context);

        // Enable the video module.
        agoraEngine.EnableVideo();

        // Set the user role as broadcaster.
        agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Attach the eventHandler
        InitEventHandler();

    }

    public virtual void SetupLocalVideo()
    {
        // Set the local video view.
        LocalView.SetForUser(configData.uid, _channelName);

        // Start rendering local video.
        LocalView.SetEnable(true);

    }
    public virtual void SetClientRole(string role)
    {
        if(agoraEngine == null)
        {
            Debug.Log("Click join and then change the client role!");
            return;
        }
        userRole = role;
        if (role == "Host")
        {
            agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }
        else if(role == "Audience")
        {
            agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        }
    }
 
    // Define a public function called Leave() to leave the channel.
    public virtual void Leave()
    {
        if (agoraEngine != null)
        {
            // Leave the channel and clean up resources
            agoraEngine.LeaveChannel();
            agoraEngine.DisableVideo();
            LocalView.SetEnable(false);
            DestroyVideoView(remoteUid);
            DestroyEngine();
        }
    }

    public virtual void Join()
    {
        // Create an instance of the engine.
        SetupAgoraEngine();

        if(agoraEngine != null)
        {
            // Setup local video view.
            SetupLocalVideo();

            // Join the channel using the specified token and channel name.
            agoraEngine.JoinChannel(configData.rtcToken, configData.channelName);
        }
   
    }

    // Init event handler to receive callbacks
    public virtual void InitEventHandler()
    {
        agoraEngine.InitEventHandler(new UserEventHandler(this));
    }

    // Dynamically create views for the remote users
    public void MakeVideoView(uint uid, string channelName)
    {
        // Create and configure a remote user's video view
        AgoraUI agoraUI = new AgoraUI();
        GameObject userView = agoraUI.MakeRemoteView(uid.ToString());
        userView.AddComponent<AgoraUI>();

        VideoSurface videoSurface = userView.AddComponent<VideoSurface>();
        videoSurface.SetForUser(uid, channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);

        videoSurface.SetEnable(true);
        RemoteView = videoSurface;
    }

    // Destroy the remote user's video view when they leave
    public void DestroyVideoView(uint uid)
    {
        var userView = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(userView, null))
        {
            userView.SetActive(false); // Deactivate the GameObject
        }
    }


    // Use this function to destroy the engine
    public virtual void DestroyEngine()
    {
        if (agoraEngine != null)
        {
            // Destroy the engine.
            agoraEngine.LeaveChannel();
            agoraEngine.Dispose();
            agoraEngine = null;
        }
    }
}

// An event handler class to deal with video SDK events
internal class UserEventHandler : IRtcEngineEventHandler
{
    internal readonly AgoraManager agoraManager;

    internal UserEventHandler(AgoraManager videoSample)
    {
        agoraManager = videoSample;
    }
    // This callback is triggered when the local user joins the channel.
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log("You joined channel: " +connection.channelId);
    }
    // This callback is triggered when a remote user leaves the channel or drops offline.
    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        agoraManager.DestroyVideoView(uid);
    }
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        agoraManager.MakeVideoView(uid, connection.channelId);
        // Save the remote user ID in a variable.
        agoraManager.remoteUid = uid;
    }
}