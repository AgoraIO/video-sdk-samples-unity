using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using System;
using System.IO;
using System.Linq;


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

    // Define a public function called SetupVideoSDKEngine to setup the video SDK engine.
    public virtual void SetupAgoraEngine()
    {
        // Create an instance of the video SDK engine.
        agoraEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();

        // Set context configuration based on the product type
        CHANNEL_PROFILE_TYPE channelProfile = configData.product == "Video Calling"
            ? CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION
            : CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;

        RtcEngineContext context = new RtcEngineContext(_appID, 0, channelProfile,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, null);

        agoraEngine.Initialize(context);

        // Enable the video module.
        agoraEngine.EnableVideo();

        // Set the user role as broadcaster.
        agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Attach the eventHandler
        InitEventHandler();
        
        // Attack a video surface to the local view game object.
        LocalView = GameObject.Find("LocalView").AddComponent<VideoSurface>();

    }

    public virtual void setClientRole(string role)
    {
        if(agoraEngine == null)
        {
            Debug.Log("Click join and then change the client role!");
            return;
        }
        if(role == "Host")
        {
            Debug.Log("Role is set to Host");
            agoraEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }
        else
        {
            Debug.Log("Role is set to Audience");
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

        // Set the local video view.
        LocalView.SetForUser(configData.uid, configData.channelName);

        // Start rendering local video.
        LocalView.SetEnable(true);

        // Join the channel using the specified token and channel name.
        agoraEngine.JoinChannel(_token, _channelName);
    }

    // Init event handler to receive callbacks
    public virtual void InitEventHandler()
    {
        agoraEngine.InitEventHandler(new UserEventHandler(this));
    }

    // Dynamically create views for the remote users
    public void MakeVideoView(uint uid)
    {
        // Create and configure a remote user's video view
        AgoraUI agoraUI = new AgoraUI();
        GameObject userView = agoraUI.MakeRemoteView(uid.ToString());
        userView.AddComponent<AgoraUI>();

        VideoSurface videoSurface = userView.AddComponent<VideoSurface>();
        videoSurface.SetForUser(uid, _channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            float scale = (float)height / (float)width;
            videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
            Debug.Log("OnTextureSizeModify: " + width + " " + height);
        };
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
    internal readonly AgoraManager _videoSample;

    internal UserEventHandler(AgoraManager videoSample)
    {
        _videoSample = videoSample;
    }
    // This callback is triggered when the local user joins the channel.
    public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
    {
        Debug.Log("You joined channel: " +connection.channelId);
    }
    // This callback is triggered when a remote user leaves the channel or drops offline.
    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        _videoSample.DestroyVideoView(uid);
    }
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        _videoSample.MakeVideoView(uid);
        // Save the remote user ID in a variable.
        _videoSample.remoteUid = uid;
    }
}