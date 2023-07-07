using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using System;
using System.IO;

[Serializable]
public class ConfigData
{
    public string appID;
    public string channelName;
    public string token;
    public string secretKey = "";
    public string salt = "";
    public int tokenExpiryTime = 3600; // Default time of 1 hour
    public string tokenUrl = ""; // Add Token Generator URL ...
    public int uid  = 0; // RTC elected user ID (0 = choose random)
}

public class AgoraManager
{
    // Define some variables to be used later.
    internal string _appID;
    internal string _channelName;
    internal string _token;
    internal uint remoteUid;
    internal IRtcEngine RtcEngine;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;

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
    
    private void LoadConfigFromJSON()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "AgoraManager", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            ConfigData configData = JsonUtility.FromJson<ConfigData>(json);

            _appID = configData.appID;
            _channelName = configData.channelName;
            _token = configData.token;
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }
    // Define a public function called SetupVideoSDKEngine to setup the video SDK engine.
    public virtual IRtcEngine SetupVideoSDKEngine()
    {
        LoadConfigFromJSON();
        // Create an instance of the video SDK engine.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        
        // Specify the context configuration to initialize the created instance.
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, null);

        // Initialize the instance with the specified context.
        RtcEngine.Initialize(context);

        // Enable the video module.
        RtcEngine.EnableVideo();

        // Set the user role as broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);


        // Return the initialized instance of the video SDK engine.
        return RtcEngine;
    }
 
    // Define a public function called Leave() to leave the channel.
    public virtual void Leave()
    {
        // Leave the channel.
        RtcEngine.LeaveChannel();

        // Disable the video modules.
        RtcEngine.DisableVideo();

        // Stop rendering the remote video.
        RemoteView.SetEnable(false);

        // Stop rendering the local video.
        LocalView.SetEnable(false);

        // Kill the engine instance
        RtcEngine.Dispose();
        RtcEngine = null;
    }

    public virtual void Join()
    {
        Debug.Log("Called Join");
        
        // Create an instance of the engine.
        RtcEngine = SetupVideoSDKEngine();

        // Setup an event handler to receive callbacks.
        InitEventHandler();

        // Set the local video view.
        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);

        // Start rendering local video.
        LocalView.SetEnable(true);

        // Join the channel using the specified token and channel name.
        RtcEngine.JoinChannel(_token, _channelName);
    }
    public void OnDestroy()
    {
        if(RtcEngine != null)
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
            RtcEngine = null;
            
    }
    public virtual void InitEventHandler()
    {
        RtcEngine.InitEventHandler(new UserEventHandler(this));
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
        _videoSample.RemoteView.SetEnable(false);
    }
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        // Setup remote view.
        _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        // Save the remote user ID in a variable.
        _videoSample.remoteUid = uid;
    }
}