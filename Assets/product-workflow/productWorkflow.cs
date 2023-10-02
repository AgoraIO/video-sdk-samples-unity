using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductWorkflow : AgoraUI
{
    internal ProductWorkflowManager productWorkflowManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject channelFieldGo;
    internal GameObject shareScreenBtnGo;
    internal GameObject volumeControlGo;
    internal GameObject muteToggleGo;
    private bool isSharing = false;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the ProductWorkflowManager
        productWorkflowManager = new ProductWorkflowManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        productWorkflowManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        shareScreenBtnGo = AddButton("screenShare", new Vector3(-188, -172, 0), "Share Screen", new Vector2(160, 30));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");
        if (productWorkflowManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }
        muteToggleGo = AddToggle("Mute", new Vector3(-19, -116), "Mute", new Vector2());
        volumeControlGo = AddSlider("localAudioSlider", new Vector2(319, 171));

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>()?.onClick.AddListener(productWorkflowManager.Join);
        leaveBtnGo.GetComponent<Button>()?.onClick.AddListener(productWorkflowManager.Leave);
        shareScreenBtnGo.GetComponent<Button>()?.onClick.AddListener(ToggleScreenSharing);
        if (hostToggleGo != null && audienceToggleGo != null)
        {
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;

            // Toggle event listeners for role selection.
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                productWorkflowManager.SetClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                productWorkflowManager.SetClientRole("Audience");
            });
        }
        Toggle muteToggle = muteToggleGo.GetComponent<Toggle>();
        muteToggle.isOn = false;
        muteToggle.onValueChanged.AddListener((value) =>
        {
            productWorkflowManager.MuteRemoteAudio(value);
        });
        Slider volumeControl = volumeControlGo.GetComponent<Slider>();
        volumeControl.maxValue = 100;
        volumeControl.onValueChanged.AddListener((value) => productWorkflowManager.ChangeVolume((int)value));

        // Add a listener to the channel input field to update the channel name in ProductWorkflowManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField?.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Pass the channel name to the ProductWorkflowManager class to fetch a token from the token server.
    private void HandleChannelFieldChange(string newValue)
    {
        productWorkflowManager.configData.channelName = newValue;
    }

    public void ToggleScreenSharing()
    {
        if (isSharing)
        {
            // Stop screen sharing.
            productWorkflowManager.StopSharing();

            // Rotate the local view for screen sharing.
            LocalViewGo.transform.Rotate(180.0f, 0.0f, 180.0f);

            // Play the local video instead of screen sharing.
            productWorkflowManager.PlayScreenTrackLocally(false, LocalViewGo);

            // Change the button text.
            var buttonText = shareScreenBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
            if (buttonText != null)
            {
                buttonText.text = "Share Screen";
            }

            // Update the screen sharing state
            isSharing = false;
        }
        else
        {
            // Start screen sharing.
            productWorkflowManager.StartSharing();

            // Rotate the local view for screen sharing.
            LocalViewGo.transform.Rotate(-180.0f, 0.0f, 180.0f);

            // Play the screen sharing track locally.
            productWorkflowManager.PlayScreenTrackLocally(true, LocalViewGo);

            // Change the button text.
            var buttonText = shareScreenBtnGo.GetComponentInChildren<TextMeshProUGUI>(true);
            if (buttonText != null)
            {
                buttonText.text = "Stop Sharing";
            }

            // Update the screen sharing state.
            isSharing = true;
        }
    }

    public override void OnDestroy()
    {
        // Clean up resources.
        base.OnDestroy();
        productWorkflowManager.DestroyEngine();
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (shareScreenBtnGo)
            Destroy(shareScreenBtnGo.gameObject);
        if (channelFieldGo)
            Destroy(channelFieldGo.gameObject);
    }
}
