using UnityEngine;
using Agora.Rtc;
using TMPro;

public class AudioVoiceEffectsManager : AuthenticationWorkflowManager
{
    // Internal fields for managing audio and voice effects
    internal int soundEffectId = 1; // Unique identifier for the sound effect file
    internal int soundEffectStatus = 0;
    internal int voiceEffectIndex = 0;
    internal bool audioPlaying = false; // Manage the audio mixing state
    internal bool isChecked = false;

    // Method to set up the Agora engine
    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Pre-load sound effects to improve performance
        agoraEngine.PreloadEffect(soundEffectId, configData.soundEffectFilePath);

        // Specify the audio scenario and audio profile
        agoraEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT, AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_CHATROOM);
    }

    // Method to start or stop audio mixing
    public void StartAudioMixing(TMP_Text audioMixingBtnText)
    {
        audioPlaying = !audioPlaying;

        if (audioPlaying)
        {
            audioMixingBtnText.text = "Stop Audio File";
            agoraEngine.StartAudioMixing(configData.audioFilePath, false, -1);
        }
        else
        {
            agoraEngine.StopAudioMixing();
            audioMixingBtnText.text = "Play Audio";
        }
    }

    // Method to play or pause sound effects
    public void PlaySoundEffect(TMP_Text playEffectBtnText)
    {
        if (soundEffectStatus == 0)
        { // Stopped
            agoraEngine.PlayEffect(
                    soundEffectId,   // The ID of the sound effect file.
                    configData.soundEffectFilePath,   // The path of the sound effect file.
                    0,  // The number of sound effect loops. -1 means an infinite loop. 0 means once.
                    1,   // The pitch of the audio effect. 1 represents the original pitch.
                    0.0, // The spatial position of the audio effect. 0.0 represents that the audio effect plays in the front.
                    100, // The volume of the audio effect. 100 represents the original volume.
                    true,// Whether to publish the audio effect to remote users.
                    0    // The playback starting position of the audio effect file in ms.
            );
            playEffectBtnText.text = "Pause Sound Effect";
            soundEffectStatus = 1;
        }
        else if (soundEffectStatus == 1)
        {   // Playing
            agoraEngine.PauseEffect(soundEffectId);
            soundEffectStatus = 2;
            playEffectBtnText.text = "Resume Sound Effect";
        }
        else if (soundEffectStatus == 2)
        {   // Paused
            agoraEngine.ResumeEffect(soundEffectId);
            soundEffectStatus = 1;
            playEffectBtnText.text = "Pause Sound Effect";
        }
    }

    // Method to apply voice effects
    public void ApplyVoiceEffect(TMP_Text voiceEffectBtnText)
    {
        voiceEffectIndex++;
        // Turn off all previous effects
        agoraEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.VOICE_BEAUTIFIER_OFF);
        agoraEngine.SetAudioEffectPreset(AUDIO_EFFECT_PRESET.AUDIO_EFFECT_OFF);
        agoraEngine.SetVoiceConversionPreset(VOICE_CONVERSION_PRESET.VOICE_CONVERSION_OFF);

        if (voiceEffectIndex == 1)
        {
            agoraEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.CHAT_BEAUTIFIER_MAGNETIC);
            voiceEffectBtnText.text = "Effect: Chat Beautifier";
        }
        else if (voiceEffectIndex == 2)
        {
            agoraEngine.SetVoiceBeautifierPreset(VOICE_BEAUTIFIER_PRESET.SINGING_BEAUTIFIER);
            voiceEffectBtnText.text = "Effect: Singing Beautifier";
        }
        // Add more voice effects as needed...

        if (voiceEffectIndex > 2)
        { // Remove all effects
            voiceEffectIndex = 0;
            // Reset other voice settings
            voiceEffectBtnText.text = "Apply Voice Effect";
        }
    }

    // Method to control playback through the speaker
    public void PlaybackSpeaker(bool value)
    {
        agoraEngine.SetDefaultAudioRouteToSpeakerphone(false); // Disables the default audio route.
        agoraEngine.SetEnableSpeakerphone(value); // Enables or disables the speakerphone temporarily.
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
        audioVoiceEffectsManager.agoraEngine.StopEffect(soundId);
        audioVoiceEffectsManager.soundEffectStatus = 0; // Stopped
    }

    // Occurs when you start audio mixing, with different states
    public override void OnAudioMixingStateChanged(AUDIO_MIXING_STATE_TYPE state, AUDIO_MIXING_REASON_TYPE reason)
    {
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
        if(routing != (int)AudioRoute.ROUTE_DEFAULT)
        {
            Debug.Log("Audio route changed");
        }
    }

}