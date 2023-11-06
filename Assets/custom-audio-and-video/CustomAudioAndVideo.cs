using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;

[RequireComponent(typeof(AudioSource))]
public class CustomAudioAndVideo : AgoraUI
{
    internal CustomAudioVideoManager customAudioVideoManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo;
    private bool isJoin = false;
    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the CustomAudioVideoManager
        customAudioVideoManager = new CustomAudioVideoManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        customAudioVideoManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();

    }
    public override void Update()
    {
        base.Update();
        if(isJoin)
        {
            StartCoroutine(customAudioVideoManager.ShareScreen());
        }

    }
    // Set up UI elements
    private void SetupUI()
    {
        // Find the canvas
        canvas = GameObject.Find("Canvas")?.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        // Create and position UI elements
        joinBtnGo = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtnGo = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");

        // Check the product type to determine if host and audience toggles are needed
        if (customAudioVideoManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements.
        joinBtnGo.GetComponent<Button>().onClick.AddListener(HandleJoinCall);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(HandleLeaveCall);

        // Toggle event listeners for role selection.
        if (hostToggleGo && audienceToggleGo)
        {
            // Toggle event listeners for role selection
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    audienceToggle.isOn = false;
                    customAudioVideoManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    customAudioVideoManager.SetClientRole("Audience");
                }
            });

        }
        // Add a listener to the channel input field to update the channel name in CustomAudioAndVideo
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }
    private void HandleJoinCall()
    {
        customAudioVideoManager.Join();
        isJoin = true;
    }
    private void HandleLeaveCall()
    {
        customAudioVideoManager.Leave();
        isJoin = false;
    }
    // Pass the channel name to the CustomAudioAndVideo class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        customAudioVideoManager.configData.channelName = newValue;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        customAudioVideoManager.DestroyEngine();
        DestroyUIElements();
    }
    void OnAudioFilterRead(float[] data, int channels)
    {
        customAudioVideoManager.OnAudioFilterRead(data, channels);
    }

    private void DestroyUIElements()
    {
        // Destroy UI elements, e.g., channelFieldGo, audienceToggleGo, hostToggleGo
        if (channelFieldGo)
            Destroy(channelFieldGo.gameObject);
        if (audienceToggleGo)
            Destroy(audienceToggleGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
    }
}
