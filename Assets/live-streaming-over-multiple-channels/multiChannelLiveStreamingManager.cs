using Agora.Rtc;
using UnityEngine;

public class MultiChannelLiveStreamingManager : AgoraManager
{
    private RtcConnection rtcSecondConnection;
    internal IRtcEngineEx agoraEngineEx;

    // Override SetupAgoraEngine to set up an instance of the IRtcEngineEx engine.
    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Create an Ex engine instance
        agoraEngineEx = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();

        // Initialize the engine
        agoraEngineEx.Initialize(new RtcEngineContext(_appID, 0, CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT, region, null));

        // Enable the video module.
        agoraEngine.EnableVideo();

        // Initialize the event handler
        agoraEngine.InitEventHandler(new MultiChannelLiveStreamingEventHandler(this));
    }

    // Method to relay media to the destination channel.
    public void StartChannelRelay()
    {
        if (agoraEngine != null)
        {
            if (string.IsNullOrEmpty(configData.destChannelName) || string.IsNullOrEmpty(configData.destToken))
            {
                Debug.Log("Specify a valid destination channel name and token.");
                return;
            }

            // Configure a ChannelMediaRelayConfiguration instance to add source and destination channels.
            ChannelMediaRelayConfiguration mediaRelayConfiguration = new ChannelMediaRelayConfiguration();

            // Configure the source channel information.
            mediaRelayConfiguration.srcInfo = new ChannelMediaInfo
            {
                channelName = configData.channelName,
                uid = configData.uid,
                token = configData.rtcToken
            };

            // Set up the destination channel information.
            mediaRelayConfiguration.destInfos = new ChannelMediaInfo[1];
            mediaRelayConfiguration.destInfos[0] = new ChannelMediaInfo
            {
                channelName = configData.destChannelName,
                uid = configData.destUID,
                token = configData.destToken
            };

            // Number of destination channels.
            mediaRelayConfiguration.destCount = 1;

            // Start media relaying
            agoraEngine.StartOrUpdateChannelMediaRelay(mediaRelayConfiguration);
        }
        else
        {
            Debug.Log("Agora Engine is not initialized. Click 'Join' to join the primary channel and then join the second channel.");
        }
    }

    // Method to stop media relaying.
    public void StopChannelRelay()
    {
        if (agoraEngine != null)
        {
            agoraEngine.StopChannelMediaRelay();
        }
    }

    // Method to join the second channel.
    public void JoinSecondChannel()
    {
        if (agoraEngineEx != null)
        {
            if (string.IsNullOrEmpty(configData.secondChannelToken) || string.IsNullOrEmpty(configData.secondChannelName))
            {
                Debug.Log("please specify a valid channel name and a token for the second channel");
                return;
            }
            ChannelMediaOptions mediaOptions = new ChannelMediaOptions();
            mediaOptions.autoSubscribeAudio.SetValue(true);
            mediaOptions.autoSubscribeVideo.SetValue(true);
            mediaOptions.publishCameraTrack.SetValue(true);
            mediaOptions.publishMicrophoneTrack.SetValue(true);
            mediaOptions.clientRoleType.SetValue(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            mediaOptions.channelProfile.SetValue(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            rtcSecondConnection = new RtcConnection();
            rtcSecondConnection.channelId = configData.secondChannelName;
            rtcSecondConnection.localUid = configData.secondChannelUID;
            agoraEngineEx.JoinChannelEx(configData.secondChannelToken, rtcSecondConnection, mediaOptions);
        }
        else
        {
            Debug.Log("Engine was not initialized");
        }
    }

    // Method to leave the second channel.
    public void LeaveSecondChannel()
    {
        if (agoraEngineEx != null)
        {
            agoraEngineEx.LeaveChannelEx(rtcSecondConnection);
        }
    }

    // Dispose the engine instance
    public override void DestroyEngine()
    {
        base.DestroyEngine();
        if(agoraEngineEx != null)
        {
            LeaveSecondChannel();
            agoraEngineEx.Dispose();
        }
    }
}

// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class MultiChannelLiveStreamingEventHandler : UserEventHandler
{
    private MultiChannelLiveStreamingManager multiChannelLiveStreamingManager;

    internal MultiChannelLiveStreamingEventHandler(MultiChannelLiveStreamingManager videoSample) : base(videoSample)
    {
        multiChannelLiveStreamingManager = videoSample;
    }

    public override void OnChannelMediaRelayStateChanged(int state, int code)
    {
        // This example shows messages in the debug console when the relay state changes,
        // a production level app needs to handle state change properly.
        switch (state)
        {
            case 1: // RELAY_STATE_CONNECTING:
                Debug.Log("Channel media relay connecting.");
                break;
            case 2: // RELAY_STATE_RUNNING:
                Debug.Log("Channel media relay running.");
                break;
            case 3: // RELAY_STATE_FAILURE:
                Debug.Log("Channel media relay failure. Error code: " + code);
                break;
        }
    }
}