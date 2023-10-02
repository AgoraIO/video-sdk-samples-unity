using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;

public class MediaStreamEncryption : AgoraUI
{
    internal MediaEncryptionManager mediaEncryptionManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the MediaEncryptionManager
        mediaEncryptionManager = new MediaEncryptionManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        mediaEncryptionManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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

        // Check the product type for conditional UI elements
        if (mediaEncryptionManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>().onClick.AddListener(mediaEncryptionManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(mediaEncryptionManager.Leave);

        // Handle role selection with toggles (Host and Audience)
        if (hostToggleGo && audienceToggleGo)
        {
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;

            // Toggle event listeners for role selection.
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                mediaEncryptionManager.SetClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                mediaEncryptionManager.SetClientRole("Audience");
            });
        }

        // Add a listener to the channel input field to update the channel name in MediaEncryptionManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Pass the channel name to the MediaEncryptionManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        mediaEncryptionManager.configData.channelName = newValue;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        mediaEncryptionManager.DestroyEngine();
        DestroyUIElements();
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
