using Agora.Rtc;
using TMPro;
using UnityEngine;
public class ProductWorkflowManager : AuthenticationWorkflowManager
{
    // Track screen sharing state.
    private bool sharingScreen = false;

    public override void Join()
    {
        //Join the channel.
        base.Join();
    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
    }
    public void ChangeVolume(int volume)
    {
        // Adjust the recorded signal volume.
        agoraEngine.AdjustRecordingSignalVolume(volume);
    }
    public void MuteRemoteAudio(bool value)
    {
        // Pass the uid of the remote user you want to mute.
        agoraEngine.MuteRemoteAudioStream(System.Convert.ToUInt32(remoteUid), value);
    }
    public void UpdateChannelPublishOptions(bool publishMediaPlayer)
    {
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishScreenTrack.SetValue(publishMediaPlayer);
        channelOptions.publishMicrophoneTrack.SetValue(true);
        channelOptions.publishSecondaryScreenTrack.SetValue(publishMediaPlayer);
        channelOptions.publishCameraTrack.SetValue(!publishMediaPlayer);
        agoraEngine.UpdateChannelMediaOptions(channelOptions);
    }
    public void SetupLocalVideo(bool isScreenSharing)
    {
        if (isScreenSharing)
        {
            LocalView = new VideoSurface();
            // Render the screen sharing track on the local view.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN_PRIMARY);

        }
        else
        {
            LocalView = new VideoSurface();
            // Render the local video track on the local view.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
        }
    }
    public void StopSharing()
    {
        // Stop sharing.
        agoraEngine.StopScreenCapture();
        // Publish the local video track when you stop sharing your screen.
        UpdateChannelPublishOptions(false);
        // Display the local video in the local view.
        SetupLocalVideo(false);
        // Update the screen sharing state.
        sharingScreen = false;
    }
    public void StartSharing()
    {
        if (agoraEngine == null)
        {
            Debug.Log("Join a channel to start screen sharing");
            return;
        }
        // The target size of the screen or window thumbnail (the width and height are in pixels).
        SIZE t = new SIZE(360, 240);
        // The target size of the icon corresponding to the application program (the width and height are in pixels)
        SIZE s = new SIZE(360, 240);
        // Get a list of shareable screens and windows
        var info = agoraEngine.GetScreenCaptureSources(t, s, true);
        // Get the first source id to share the whole screen.
        long dispId = info[0].sourceId;
        // To share a part of the screen, specify the screen width and size using the Rectangle class.
        agoraEngine.StartScreenCaptureByWindowId(System.Convert.ToUInt32(dispId), new Rectangle(),
             default(ScreenCaptureParameters));
        // Publish the screen track and unpublish the local video track.
        UpdateChannelPublishOptions(true);
        // Display the screen track in the local view.
        SetupLocalVideo(true); 
    }

}
