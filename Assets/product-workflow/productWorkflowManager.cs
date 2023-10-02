using System;
using Agora.Rtc;
using UnityEngine;
public class ProductWorkflowManager : AuthenticationWorkflowManager
{
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
        // Adjust the volume of the recorded signal.
        agoraEngine.AdjustRecordingSignalVolume(volume);
    }

    public void MuteRemoteAudio(bool value)
    {
        if (remoteUid > 0)
        {
            // Pass the uid of the remote user you want to mute.
            agoraEngine.MuteRemoteAudioStream(Convert.ToUInt32(remoteUid), value);
        }
        else
        {
            Debug.Log("No remote user in the channel");
        }
    }

    public void PublishScreenTrack()
    {
        // Publish the screen track
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishScreenTrack.SetValue(true);
        channelOptions.publishMicrophoneTrack.SetValue(true);
        channelOptions.publishSecondaryScreenTrack.SetValue(true);
        channelOptions.publishCameraTrack.SetValue(false);
        agoraEngine.UpdateChannelMediaOptions(channelOptions);
    }

    public void UnPublishScreenTrack()
    {
        // Unpublish the screen track.
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishScreenTrack.SetValue(false);
        channelOptions.publishCameraTrack.SetValue(true);
        channelOptions.publishMicrophoneTrack.SetValue(true);
        agoraEngine.UpdateChannelMediaOptions(channelOptions);
    }

    public void PlayScreenTrackLocally(bool isScreenSharing, GameObject localViewGo)
    {
        if (isScreenSharing)
        {
            LocalView = localViewGo.AddComponent<VideoSurface>();
            // Render the screen sharing track on the local view GameObject.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_SCREEN_PRIMARY);
        }
        else
        {
            // Update the VideoSurface component of the local view GameObject.
            LocalView = localViewGo.AddComponent<VideoSurface>();
            // Render the local video track on the local view GameObject.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
        }
    }

    public void StopSharing()
    {
        // Stop screen sharing.
        agoraEngine.StopScreenCapture();

        // Publish the local video track when you stop sharing your screen.
        UnPublishScreenTrack();

    }

    private void StartScreenCaptureAndroid(long sourceId)
    {
        // Configure screen capture parameters for Android.
        var parameters2 = new ScreenCaptureParameters2();
        parameters2.captureAudio = true;
        parameters2.captureVideo = true;
        agoraEngine.StartScreenCapture(parameters2);
    }

    private void StartScreenCaptureWindows(long sourceId)
    {
        // Configure screen capture parameters for Windows.
        agoraEngine.StartScreenCaptureByDisplayId((uint)sourceId, default(Rectangle),
            new ScreenCaptureParameters { captureMouseCursor = true, frameRate = 30 });
    }

    // Get the list of shareable screens
    private ScreenCaptureSourceInfo[] GetScreenCaptureSources()
    {
        SIZE targetSize = new SIZE(360, 660);
        return agoraEngine.GetScreenCaptureSources(targetSize, targetSize, true);
    }

    // Share the screen
    public void StartSharing()
    {
        if (agoraEngine == null)
        {
            Debug.Log("Join a channel to start screen sharing");
            return;
        }

        // Get a list of shareable screens and windows.
        var captureSources = GetScreenCaptureSources();

        if (captureSources != null && captureSources.Length > 0)
        {
            var sourceId = captureSources[0].sourceId;

            // Start screen sharing based on platform.
#if UNITY_ANDROID || UNITY_IPHONE
            StartScreenCaptureAndroid(sourceId);
#else
            StartScreenCaptureWindows(sourceId);
#endif

            // Publish the screen track and update local video display.
            PublishScreenTrack();
        }
        else
        {
            Debug.LogWarning("No screen capture sources found.");
        }
    }

}
