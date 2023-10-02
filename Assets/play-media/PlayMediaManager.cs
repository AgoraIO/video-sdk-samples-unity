using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;


public class PlayMediaManager : AgoraManager
{
    internal IMediaPlayer mediaPlayer; // Instance of the media player
    internal bool isMediaPlaying = false;
    internal long mediaDuration = 0;    
    // In a real world app, you declare the media location variable with an empty string
    // and update it when a user chooses a media file from a local or remote source.
    internal TMP_Text playMediaBtnText;
    internal Slider mediaProgressBar;

    public void playMedia(Slider slider, TMP_Text text)
    { 
        mediaProgressBar = slider;
        playMediaBtnText = text;

        // Check if the engine exist.
        if(agoraEngine == null)
        {
            Debug.Log("Please click `Join` and then `Play Media` to play the video file");
            return;
        }
        // Initialize the mediaPlayer and open a media file
        if (mediaPlayer == null) {
        // Create an instance of the media player
        mediaPlayer = agoraEngine.CreateMediaPlayer();
        // Create an instance of mediaPlayerObserver class
        PlayMediaEventHandler mediaPlayerObserver = new PlayMediaEventHandler(this);
        // Set the mediaPlayerObserver to receive callbacks
        mediaPlayer.InitEventHandler(mediaPlayerObserver);
        // Open the media file
        mediaPlayer.Open(configData.mediaURL, 0);
        playMediaBtnText.text = "Opening Media File...";
        return;
        }
        
        // Set up the local video container to handle the media player output
        // or the camera stream, alternately.
        isMediaPlaying = !isMediaPlaying;
        // Set the stream publishing options
        updateChannelPublishOptions(isMediaPlaying);
        // Display the stream locally
        setupLocalVideo(isMediaPlaying);
        
        MEDIA_PLAYER_STATE state = mediaPlayer.GetState();
        if (isMediaPlaying) { // Play or resume playing media
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED) {
                mediaPlayer.Play();
            } else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PAUSED) {
                mediaPlayer.Resume();
            }
            playMediaBtnText.text = "Pause Playing Media";
        } else {
            if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYING) {
                // Pause media.
                mediaPlayer.Pause();
                playMediaBtnText.text = "Resume Playing Media";
            }
        }
    }
    public void updateChannelPublishOptions(bool publishMediaPlayer)
    {
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishMediaPlayerAudioTrack.SetValue(publishMediaPlayer);
        channelOptions.publishMediaPlayerVideoTrack.SetValue(publishMediaPlayer);
        channelOptions.publishMicrophoneTrack.SetValue(!publishMediaPlayer);
        channelOptions.publishCameraTrack.SetValue(!publishMediaPlayer);
        if (publishMediaPlayer)
        {
            channelOptions.publishMediaPlayerId.SetValue(mediaPlayer.GetId());
            agoraEngine.UpdateChannelMediaOptions(channelOptions);
        }
    }
    public void setupLocalVideo(bool forMediaPlayer)
    {
        if (forMediaPlayer)
        {
            GameObject go = GameObject.Find("LocalView");
            // Update the VideoSurface component of the local view object.
            LocalView = go.AddComponent<VideoSurface>();
            go.transform.Rotate(0.0f, 180.0f, 0.0f);
            // Setup local view to display the media file.
            LocalView.SetForUser((uint)mediaPlayer.GetId(), "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_MEDIA_PLAYER);

        }
        else
        {
            GameObject go = GameObject.Find("LocalView");
            // Update the VideoSurface component of the local view object.
            LocalView = go.AddComponent<VideoSurface>();
            go.transform.Rotate(0.0f, 180.0f, 0.0f);
            // Setup local view to display the local video.
            LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
        }
    }
    public override void DestroyEngine()
    {
        // Destroy the media player
        if (mediaPlayer != null)
        {
            mediaPlayer.Stop();
            agoraEngine.DestroyMediaPlayer(mediaPlayer);
        }
        base.DestroyEngine();

    }

}
internal class PlayMediaEventHandler : IMediaPlayerSourceObserver
{
    private PlayMediaManager playMedia;
    internal PlayMediaEventHandler(PlayMediaManager refPlayMedia)
    {
        playMedia = refPlayMedia;
    }
    public override void OnPlayerSourceStateChanged(MEDIA_PLAYER_STATE state, MEDIA_PLAYER_ERROR error)
    {
        Debug.Log(state.ToString());
        if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_OPEN_COMPLETED)
        {
            // Media file opened successfully. Get the duration of file to setup the progress bar.
            playMedia.mediaPlayer.GetDuration(ref playMedia.mediaDuration);
            Debug.Log("File duration is : " + playMedia.mediaDuration);
            // Update the UI
            Debug.Log("File opened successfully click `Play Media File` to play the file");
            playMedia.playMediaBtnText.text = "Play Media File";
            playMedia.mediaProgressBar.value = 0;
            playMedia.mediaProgressBar.maxValue = playMedia.mediaDuration / 1000;
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_PLAYBACK_ALL_LOOPS_COMPLETED)
        {
            playMedia.isMediaPlaying = false;
            // Media file finished playing
            playMedia.playMediaBtnText.text = " Media File";
            // Restore camera and microphone streams
            playMedia.setupLocalVideo(false);
            playMedia.updateChannelPublishOptions(false);
            // Clean up
            playMedia.mediaPlayer.Dispose();
            playMedia.mediaPlayer = null;
            playMedia.mediaProgressBar.value = 0;
        }
        else if (state == MEDIA_PLAYER_STATE.PLAYER_STATE_FAILED)
        {
            playMedia.isMediaPlaying = false;
            Debug.Log("Media player failed :" + error);
            playMedia.playMediaBtnText.text = "Load Media File";
        }
    }
    public override void OnPositionChanged(long position)
    {
        if (playMedia.mediaDuration > 0)
        {
            int result = (int)((float)position / (float)playMedia.mediaDuration * 100);
            // Update the ProgressBar
            playMedia.mediaProgressBar.value = result;
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

}
