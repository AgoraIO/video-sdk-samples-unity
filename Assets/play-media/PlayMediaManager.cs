using UnityEngine;
using Agora.Rtc;
using System;

public class PlayMediaManager : AgoraManager
{
    // Internal variables for managing media playback
    internal IMediaPlayer mediaPlayer; // Instance of the media player
    internal bool isMediaPlaying = false;
    internal long mediaDuration = 0;
    internal MEDIA_PLAYER_STATE state;
    internal long position;

    public void InitMediaPlayer()
    {
        // Check if the engine exists.
        if (agoraEngine == null)
        {
            // Log a message if the Agora engine is not initialized
            Debug.Log("Please click `Join` and then `Play Media` to play the video file");
            return;
        }

        // Create an instance of the media player
        mediaPlayer = agoraEngine.CreateMediaPlayer();

        // Create an instance of mediaPlayerObserver class
        PlayMediaEventHandler mediaPlayerObserver = new PlayMediaEventHandler(this);

        // Set the mediaPlayerObserver to receive callbacks
        mediaPlayer.InitEventHandler(mediaPlayerObserver);

        // Open the media file specified in the configuration data
        mediaPlayer.Open(configData.videoFileURL, 0);
    }

    public void PauseMediaFile()
    {
        mediaPlayer.Pause(); // Pause the media playback
    }

    public void ResumeMediaFile()
    {
        mediaPlayer.Resume(); // Resume paused media playback
    }

    public void PlayMediaFile()
    {
        mediaPlayer.Play(); // Start or resume playing the media file
    }

    public void PublishMediaFile()
    {
        // Configure channel options to publish the media player's audio and video tracks
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishMediaPlayerAudioTrack.SetValue(true);
        channelOptions.publishMediaPlayerVideoTrack.SetValue(true);
        channelOptions.publishMicrophoneTrack.SetValue(false);
        channelOptions.publishCameraTrack.SetValue(false);
        channelOptions.publishMediaPlayerId.SetValue(mediaPlayer.GetId());

        // Update the channel's media options with the configured options
        agoraEngine.UpdateChannelMediaOptions(channelOptions);
    }

    public void UnpublishMediaFile()
    {
        // Configure channel options to unpublish the media player's audio and video tracks
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishMediaPlayerAudioTrack.SetValue(false);
        channelOptions.publishMediaPlayerVideoTrack.SetValue(false);
        channelOptions.publishMicrophoneTrack.SetValue(true);
        channelOptions.publishCameraTrack.SetValue(true);

        // Update the channel's media options with the configured options
        agoraEngine.UpdateChannelMediaOptions(channelOptions);
    }

    public void PreviewMediaTrack(bool previewMedia)
    {
        GameObject localViewGo = LocalView.gameObject;

        // Add a VideoSurface component to the local view game object
        LocalView = localViewGo.AddComponent<VideoSurface>();

        if (previewMedia)
        {
            // Setup local view to display the media file.
            LocalView.SetForUser((uint)mediaPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);
        }
        else
        {
            // Setup local view to display the local video.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
        }
    }

    public void DestroyMediaPlayer()
    {
        mediaPlayer.Dispose(); // Dispose of the media player instance
        mediaPlayer = null; // Set the media player reference to null
    }

    public override void DestroyEngine()
    {
        // Destroy the media player
        if (mediaPlayer != null)
        {
            mediaPlayer.Stop();
            DestroyMediaPlayer();
        }

        base.DestroyEngine(); // Call the base class's engine cleanup method
    }
}

// Internal class for handling media player events
internal class PlayMediaEventHandler : IMediaPlayerSourceObserver
{
    private PlayMediaManager playMediaManager;

    internal PlayMediaEventHandler(PlayMediaManager refPlayMedia)
    {
        playMediaManager = refPlayMedia;
    }

    public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR error)
    {
        Debug.Log(state.ToString());
        playMediaManager.state = state;

        if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
        {
            // Media file opened successfully. Get the duration of the file to set up the progress bar.
            playMediaManager.mediaPlayer.GetDuration(ref playMediaManager.mediaDuration);
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYING)
        {
            playMediaManager.PreviewMediaTrack(true);
            playMediaManager.PublishMediaFile();
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYBACK_ALL_LOOPS_COMPLETED)
        {
            playMediaManager.PreviewMediaTrack(false);
            playMediaManager.UnpublishMediaFile();
            // Clean up
            playMediaManager.mediaPlayer.Dispose();
            playMediaManager.mediaPlayer = null;
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PAUSED)
        {
            playMediaManager.PreviewMediaTrack(false);
            playMediaManager.UnpublishMediaFile();
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_FAILED)
        {
            Debug.Log("Media player failed :" + error);
        }
    }

    public override void OnPositionChanged(long position)
    {
        if (playMediaManager.mediaDuration > 0)
        {
            // Update the ProgressBar
            playMediaManager.position = position;
        }
    }

    public override void OnPlayerEvent(MEDIA_PLAYER_EVENT eventCode, long elapsedTime, string message)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnMetaData(byte[] type, int length)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnAudioVolumeIndication(int volume)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnPlayBufferUpdated(Int64 playCachedBuffer)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnPlayerInfoUpdated(PlayerUpdatedInfo info)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnPlayerSrcInfoChanged(SrcInfo from, SrcInfo to)
    {
        // Required to implement IMediaPlayerObserver
    }

    public override void OnPreloadEvent(string src, PLAYER_PRELOAD_EVENT @event)
    {
        // Required to implement IMediaPlayerObserver
    }
}
