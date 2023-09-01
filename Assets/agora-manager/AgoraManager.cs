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
    public string token;
    public string encryptionKey = "";
    public string salt = "";
    public int tokenExpiryTime = 3600; // Default time of 1 hour
    public string tokenUrl = ""; // Add Token Generator URL ...
    public int uid  = 0; // RTC elected user ID (0 = choose random)
    public string product;
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

    private void LoadConfigFromJSON()
    {
        string path = System.IO.Path.Combine(Application.dataPath, "agora-manager", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);

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
    public virtual void  SetupVideoSDKEngine()
    {
        LoadConfigFromJSON();
        // Create an instance of the video SDK engine.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        if(configData.product == "Video Calling")
        {
            // Specify the context configuration to initialize the created instance.
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, null);

            // Initialize the instance with the specified context.
            RtcEngine.Initialize(context);
        }
        else
        {
            // Specify the context configuration to initialize the created instance.
            RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, null);

            // Initialize the instance with the specified context.
            RtcEngine.Initialize(context);
        }

        // Enable the video module.
        RtcEngine.EnableVideo();

        // Set the user role as broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

    }

    public virtual void setClientRole(string role)
    {
        if(role == "Host")
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        }
        else
        {
            RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        }
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
    }

    public virtual void Join()
    {
        // Set the local video view.
        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);

        // Start rendering local video.
        LocalView.SetEnable(true);

        // Join the channel using the specified token and channel name.
        RtcEngine.JoinChannel(_token, _channelName);
    }

    public void DestroyVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            go.SetActive(false); // Deactivate the GameObject
        }
    }
    public virtual void InitEventHandler()
    {
        RtcEngine.InitEventHandler(new UserEventHandler(this));
    }
    public void MakeVideoView(uint uid)
    {
        var go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            return; // reuse
        }

        // create a GameObject and assign to this new user
        var videoSurface = MakeImageSurface(uid.ToString());
        if (ReferenceEquals(videoSurface, null)) return;
        // configure videoSurface
        
        videoSurface.SetForUser(uid, _channelName, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        videoSurface.OnTextureSizeModify += (int width, int height) =>
        {
            float scale = (float)height / (float)width;
            videoSurface.transform.localScale = new Vector3(-5, 5 * scale, 1);
            Debug.Log("OnTextureSizeModify: " + width + "  " + height);
        };
        videoSurface.SetEnable(true);
    }
    public VideoSurface MakeImageSurface(string goName)
    {
        GameObject go = new GameObject();

        if (go == null)
        {
            return null;
        }
        go.name = goName;
        // to be rendered onto
        go.AddComponent<RawImage>();

        go.transform.SetParent(GameObject.Find("Content").transform); // Set parent to the Content of ScrollView
        // set up transform
        go.transform.Rotate(0f, 0.0f, 180.0f);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = new Vector3(2f, 3f, 1f);

        // configure videoSurface
        var videoSurface = go.AddComponent<VideoSurface>();
        return videoSurface;
    }
    public void OnDestroy()
    {
        if(RtcEngine != null)
            RtcEngine.LeaveChannel();
            RtcEngine.Dispose();
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
        _videoSample.DestroyVideoView(uid);
    }
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        _videoSample.MakeVideoView(uid);
        // Save the remote user ID in a variable.
        _videoSample.remoteUid = uid;
    }
}