using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AudioVoiceEffects : AgoraUI
{
    internal AudioVoiceEffectsManager audioVoiceEffectsManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    GameObject audioMixingBtn;
    GameObject playEffectBtn;
    GameObject voiceEffectBtn;
    GameObject playbackDeviceToggle;
    internal GameObject channelField;


    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-327, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(330, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));

        // Setup access to the audio mixing button.
        audioMixingBtn = AddButton("audioMixing", new Vector3(-162, -172, 0), "Play Audio File", new Vector2(160f, 30f));
        playEffectBtn = AddButton("playEffectBtn", new Vector3(164, -172, 0), "Play Sound Effects", new Vector2(160f, 30f));
        voiceEffectBtn = AddButton("voiceEffectBtn", new Vector3(1, -172, 0), "Play Audio Effects", new Vector2(160f, 30f));
        playbackDeviceToggle = AddToggle("playbackDevice", new Vector2(-18, 1), "Speakerphone", new Vector2(200, 30));

        // Create an instance of the AudioVoiceEffectsManager
        audioVoiceEffectsManager = new AudioVoiceEffectsManager();

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
                audioVoiceEffectsManager.setClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                audioVoiceEffectsManager.setClientRole("Audience");
            });
        }

        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        channelField = TMP_DefaultControls.CreateInputField(resources);
        channelField.name = "channelName";
        channelField.transform.SetParent(canvas.transform, false);

        TMP_InputField tmpInputField = channelField.GetComponent<TMP_InputField>();
        RectTransform inputFieldTransform = tmpInputField.GetComponent<RectTransform>();
        inputFieldTransform.sizeDelta = new Vector2(200, 30);

        TMP_Text textComponent = channelField.GetComponentInChildren<TMP_Text>();
        textComponent.alignment = TextAlignmentOptions.Center;

        // Change the placeholder text
        tmpInputField.placeholder.GetComponent<TMP_Text>().text = "Channel Name";

        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(audioVoiceEffectsManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(audioVoiceEffectsManager.Join);
        // Add click-event functions to the audio effect, voice effect, and audio mixing buttons.
        audioMixingBtn.GetComponent<Button>().onClick.AddListener(() => audioVoiceEffectsManager.StartAudioMixing(audioMixingBtn.GetComponentInChildren<TextMeshProUGUI>(true)));
        playEffectBtn.GetComponent<Button>().onClick.AddListener(() => audioVoiceEffectsManager.ApplyVoiceEffect(playEffectBtn.GetComponentInChildren<TextMeshProUGUI>(true)));
        voiceEffectBtn.GetComponent<Button>().onClick.AddListener(() => audioVoiceEffectsManager.PlaySoundEffect(voiceEffectBtn.GetComponentInChildren<TextMeshProUGUI>(true)));
        playbackDeviceToggle.GetComponent<Toggle>().onValueChanged.AddListener((value) =>
        {
            audioVoiceEffectsManager.PlaybackSpeaker(value);
        });

    }
    public override void OnDestroy()
    {
        // Clean up
        base.OnDestroy();
        audioVoiceEffectsManager.DestroyEngine();
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
}
