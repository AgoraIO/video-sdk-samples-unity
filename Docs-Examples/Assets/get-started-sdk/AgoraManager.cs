

using UnityEngine;
using UnityEngine.UI;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

using Agora.Rtc;

// Create a class called AgoraManager and inherit from the IRtcEngineEventHandler interface.
public class AgoraManager : IRtcEngineEventHandler
{
    // Define some variables to be used later.
    internal string _appID;
    internal string _channelName;
    internal string _token;
    internal uint remoteUid;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;

    #if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    // Define an ArrayList of permissions required for Android devices.
    private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
    #endif

    // Define a private function called CheckPermissions() to check for required permissions.
    private void CheckPermissions()
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

    // Define a public function called SetupVideoSDKEngine to setup the video SDK engine.
    public IRtcEngine SetupVideoSDKEngine(string appID, string channel, string token)
    {
        _appID = appID;
        _channelName = channel;
        _token = token;
        
        // Call the CheckPermissions() function to check for required permissions.
        CheckPermissions();

        // Create an instance of the video SDK engine.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        
        // Specify the context configuration to initialize the created instance.
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, AREA_CODE.AREA_CODE_GLOB, new LogConfig("./log.txt"));

        // Initialize the instance with the specified context.
        RtcEngine.Initialize(context);

        // Return the initialized instance of the video SDK engine.
        return RtcEngine;
    }

    // Define a public function called Leave() to leave the channel.
    public void Leave()
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

    // Define a public function called Join() to join the channel.
    public void Join()
    {
        // Enable the video module.
        RtcEngine.EnableVideo();

        // Set the user role as broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        // Set the local video view.
        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);

        // Start rendering local video.
        LocalView.SetEnable(true);

        // Join the channel using the specified token and channel name.
        RtcEngine.JoinChannel(_token, _channelName);
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
    public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
    {
        // Setup remote view.
        _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
        // Save the remote user ID in a variable.
        _videoSample.remoteUid = uid;
    }
    // This callback is triggered when a remote user leaves the channel or drops offline.
    public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
    {
        _videoSample.RemoteView.SetEnable(false);
    }
}