using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AudioVoiceEffects : AgoraUI
{
    // Reference to the AudioVoiceEffectsManager
    internal AudioVoiceEffectsManager audioVoiceEffectsManager;

    //UI elements
    internal GameObject audienceToggleGo, hostToggleGo;
    GameObject audioMixingBtn;
    GameObject playEffectBtn;
    GameObject voiceEffectBtn;
    GameObject playbackDeviceToggle;
    GameObject channelFieldGo;
    internal GameObject channelField;

    // States and indices
    internal int soundEffectState = 0;
    internal int voiceEffectIndex = 0;
    internal bool isAudioMixing = false;


    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the AudioVoiceEffectsManager
        audioVoiceEffectsManager = new AudioVoiceEffectsManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        audioVoiceEffectsManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
    }

    public void SetupUI()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        // Create and position UI elements
        joinBtnGo = AddButton("Join", new Vector3(-327, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtnGo = AddButton("Leave", new Vector3(330, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");
        audioMixingBtn = AddButton("audioMixing", new Vector3(-162, -172, 0), "Start Audio Mixing", new Vector2(160f, 30f));
        playEffectBtn = AddButton("playEffectBtn", new Vector3(164, -172, 0), "Play Sound Effect", new Vector2(160f, 30f));
        voiceEffectBtn = AddButton("voiceEffectBtn", new Vector3(1, -172, 0), "Apply Voice Effects", new Vector2(160f, 30f));
        playbackDeviceToggle = AddToggle("playbackDevice", new Vector2(-18, -50), "Speakerphone", new Vector2(200, 30));


        if (audioVoiceEffectsManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                audioVoiceEffectsManager.SetClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                audioVoiceEffectsManager.SetClientRole("Audience");
            });
        }

        // Add click-event functions to buttons
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(audioVoiceEffectsManager.Leave);
        joinBtnGo.GetComponent<Button>().onClick.AddListener(audioVoiceEffectsManager.Join);
        audioMixingBtn.GetComponent<Button>().onClick.AddListener(() => StartStopAudioMixing());
        playEffectBtn.GetComponent<Button>().onClick.AddListener(() => PlayPauseSoundEffect());
        voiceEffectBtn.GetComponent<Button>().onClick.AddListener(() => ApplyVoiceEffect());
        playbackDeviceToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            audioVoiceEffectsManager.enableSpeakerPhone = value;
        });

        // Add a listener to the channel input field to update the channel name in AudioVoiceEffectsManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);

    }

    // Update the channel name in the AudioVoiceEffectsManager
    private void HandleChannelFieldChange(string newValue)
    {
        audioVoiceEffectsManager.configData.channelName = newValue;
    }

    // Play, pause, and resume the sound effect.
    public void PlayPauseSoundEffect()
    {
        TMP_Text playEffectBtnText = playEffectBtn.GetComponentInChildren<TextMeshProUGUI>(true);

        switch (soundEffectState)
        {
            case 0: // Stopped
                audioVoiceEffectsManager.PlaySoundEffect();
                playEffectBtnText.text = "Pause Sound Effect";
                soundEffectState = 1;
                break;
            case 1: // Playing
                audioVoiceEffectsManager.PauseSoundEffect();
                soundEffectState = 2;
                playEffectBtnText.text = "Resume Sound Effect";
                break;
            case 2: // Paused
                audioVoiceEffectsManager.ResumeSoundEffect();
                soundEffectState = 1;
                playEffectBtnText.text = "Pause Sound Effect";
                break;
        }
    }

    // Apply different voice effects.
    public void ApplyVoiceEffect()
    {
        TMP_Text voiceEffectBtnText = voiceEffectBtn.GetComponentInChildren<TextMeshProUGUI>(true);
        voiceEffectIndex++;

        switch (voiceEffectIndex)
        {
            case 1:
                // Apply the chat beautifier voice effect
                audioVoiceEffectsManager.ApplyVoiceEffect(VOICE_BEAUTIFIER_PRESET.CHAT_BEAUTIFIER_MAGNETIC);
                voiceEffectBtnText.text = "Effect: Chat Beautifier";
                break;
            case 2:
                // Apply the singing beautifier voice effect
                audioVoiceEffectsManager.ApplyVoiceEffect(VOICE_BEAUTIFIER_PRESET.SINGING_BEAUTIFIER);
                voiceEffectBtnText.text = "Effect: Singing Beautifier";
                break;
            default:
                // Remove all effects and reset other 0po87hfcoi90
                voiceEffectIndex = 0;
                voiceEffectBtnText.text = "Apply Voice Effect";
                break;
        }
    }

    // Start and stop audio mixing
    public void StartStopAudioMixing()
    {
        var audioMixingState = audioVoiceEffectsManager.GetAudioMixingState();

        switch (audioMixingState)
        {
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_STOPPED:
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_FAILED:
                // Start audio mixing
                audioVoiceEffectsManager.StartAudioMixing();
                break;
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PAUSED:
                // Resume audio mixing
                audioVoiceEffectsManager.ResumeAudioMixing();
                break;
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PLAYING:
                // Pause audio mixing
                audioVoiceEffectsManager.PauseAudioMixing();
                break;
            default:
                audioVoiceEffectsManager.StartAudioMixing();
                break;
        }
    }

    private void DestroyUIElements()
    {
        // Destroy UI elements.
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (voiceEffectBtn)
            Destroy(voiceEffectBtn.gameObject);
        if (audioMixingBtn)
            Destroy(audioMixingBtn.gameObject);
        if (playEffectBtn)
            Destroy(playEffectBtn.gameObject);
        if (playbackDeviceToggle)
            Destroy(playbackDeviceToggle.gameObject);
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (channelField)
            Destroy(channelField.gameObject);
    }


    public override void Update()
    {
        base.Update();

        var audioMixingState = audioVoiceEffectsManager.GetAudioMixingState();
        TMP_Text audioMixingBtnTxt = audioMixingBtn.GetComponentInChildren<TextMeshProUGUI>(true);

        // Use a switch statement to set the text of the audio mixing button based on the audio mixing state.
        switch (audioMixingState)
        {
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_FAILED:
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_STOPPED:
                audioMixingBtnTxt.text = "Start Audio Mixing";
                break;
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PLAYING:
                audioMixingBtnTxt.text = "Pause Audio Mixing";
                break;
            case AUDIO_MIXING_STATE_TYPE.AUDIO_MIXING_STATE_PAUSED:
                audioMixingBtnTxt.text = "Resume Audio Mixing";
                break;
        }

        // Check if a voice effect is in progress and update the text of the playEffectBtn accordingly.
        if (audioVoiceEffectsManager.GetSoundEffectState())
        {
            playEffectBtn.GetComponentInChildren<TextMeshProUGUI>(true).text = "Play Sound Effect";
            soundEffectState = 0;
        }
    }
    public override void OnDestroy()
    {
        // Clean up resources on script destruction
        base.OnDestroy();
        audioVoiceEffectsManager.DestroyEngine();
        DestroyUIElements();
    }
}
