using UnityEngine;
using Agora.Rtc;

public class AudioVoiceEffectsManager : AuthenticationWorkflowManager
{
    // Internal fields for managing audio and voice effects
    internal int soundEffectId = 1; // Unique identifier for the sound effect file
    internal AUDIO_MIXING_STATE_TYPE audioMixingState;
    internal bool isEffectFinished = false;
    internal bool enableSpeakerPhone = false;

    // Method to set up the Agora engine
    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Pre-load sound effects to improve performance
        agoraEngine.PreloadEffect(soundEffectId, configData.soundEffectFileURL);

        // Specify the audio scenario and audio profile
        agoraEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM);

        // Initialize event handling for Agora
        agoraEngine.InitEventHandler(new AudioVoiceEffectEventHandler(this));

#if (UNITY_ANDROID || UNITY_IOS)
            agoraEngine.SetDefaultAudioRouteToSpeakerphone(!enableSpeakerPhone); // Disables the default audio route.
        agoraEngine.SetEnableSpeakerphone(enableSpeakerPhone); // Enables or disables the speakerphone temporarily.
#endif
    }

    // Return the audio mixing state
    public AUDIO_MIXING_STATE_TYPE GetAudioMixingState()
    {
        return audioMixingState;
    }

    // Method to get the current voice effect state
    public bool GetSoundEffectState()
    {
        return isEffectFinished;
    }

    // Method to start audio mixing
    public void StartAudioMixing()
    {
        if(agoraEngine == null)
        {
            Debug.Log("Join the channel to start audio mixing");
            return;
        }
        agoraEngine.StartAudioMixing(configData.audioFileURL, false, 1);
    }

    // Method to pause audio mixing.
    public void PauseAudioMixing()
    {
        agoraEngine.PauseAudioMixing();
    }

    // Method to resume audio mixing
    public void ResumeAudioMixing()
    {
        agoraEngine.ResumeAudioMixing();
    }

    // Method to stop audio mixing
    public void StopAudioMixing()
    {
        agoraEngine.StopAudioMixing();
    }

    // Method to play the sound effect
    public void PlaySoundEffect()
    {
        if (agoraEngine == null)
        {
            Debug.Log("Join the channel to play the sound effect");
            return;
        }
        agoraEngine.PlayEffect(
            soundEffectId,   // The ID of the sound effect file.
            configData.soundEffectFileURL,   // The path of the sound effect file.
            0,  // The number of sound effect loops. -1 means an infinite loop. 0 means once.
            1,   // The pitch of the audio effect. 1 represents the original pitch.
            0.0, // The spatial position of the audio effect. 0.0 represents that the audio effect plays in the front.
            100, // The volume of the audio effect. 100 represents the original volume.
            true,// Whether to publish the audio effect to remote users.
            0    // The playback starting position of the audio effect file in ms.
        );
    }

    // Pause the sound effect
    public void PauseSoundEffect()
    {
        agoraEngine.PauseEffect(soundEffectId);
    }

    // Resume the sound effect
    public void ResumeSoundEffect()
    {
        agoraEngine.ResumeEffect(soundEffectId);
    }

    // Method to apply voice effects
    public void ApplyVoiceEffect(VOICE_BEAUTIFIER_PRESET effect)
    {
        if (agoraEngine == null)
        {
            Debug.Log("Join the channel to apply voice effects");
            return;
        }
        agoraEngine.SetVoiceBeautifierPreset(effect);
    }

    // Method to remove all voice effects
    public void RemoveEffect()
    {
        // Turn off all previous effects
        agoraEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.VOICE_BEAUTIFIER_OFF);
        agoraEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.AUDIO_EFFECT_OFF);
        agoraEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CONVERSION_OFF);
    }

    public override void Leave()
    {
        base.Leave();
        audioMixingState = AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_STOPPED;
        isEffectFinished = true;
    }

}

// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class AudioVoiceEffectEventHandler : UserEventHandler
{
    private AudioVoiceEffectsManager audioVoiceEffectsManager;

    internal AudioVoiceEffectEventHandler(AudioVoiceEffectsManager videoSample) : base(videoSample)
    {
        audioVoiceEffectsManager = videoSample;
    }

    // Occurs when the audio effect playback finishes
    public override void OnAudioEffectFinished(int soundId)
    {
        // Handle the event, stop the audio effect, and reset its status
        Debug.Log("Audio effect finished");
        audioVoiceEffectsManager.isEffectFinished = true;
        audioVoiceEffectsManager.StopAudioMixing();
    }

    // Occurs when you start audio mixing, with different states
    public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE reason)
    {
        audioVoiceEffectsManager.audioMixingState = state;
        // Handle audio mixing state changes, such as failure, pause, play, or stop
        if (state == AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_FAILED)
        {
            Debug.Log("Audio mixing failed: " + reason);
        }
        else if (state == AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PAUSED)
        {
            Debug.Log("Audio mixing paused : " + reason);
        }
        else if (state == AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PLAYING)
        {
            Debug.Log("Audio mixing started: " + reason);
        }
        else if (state == AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_STOPPED)
        {
            Debug.Log("Audio mixing stopped: " + reason);
        }
    }

    // Occurs when the audio route changes
    public override void OnAudioRoutingChanged(int routing)
    {
        if (routing != (int)AudioRoute.ROUTE_DEFAULT)
        {
            Debug.Log("Audio route changed");
        }
    }
}
