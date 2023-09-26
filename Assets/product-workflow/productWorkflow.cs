using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProductWorkflow : AgoraUI
{
    internal ProductWorkflowManager productWorkflowManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject channelField;
    internal GameObject shareScreenBtnGo;
    internal GameObject volumeControlGo;
    private bool isSharing;

    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));

        // Set up a button to start and stop screen sharing.
        shareScreenBtnGo = AddButton("screenShare", new Vector3(-188, -172, 0), "Share Screen", new Vector2(160, 30));

        // Set up a slider to control the local audio volume.
        volumeControlGo = AddSlider("mediaPlayerSlider", new Vector2(319, 171));
        Slider volumeControl = volumeControlGo.GetComponent<Slider>();
        volumeControl.maxValue = 100;

        // Setup a toggle to mute and unmute the remote user.
        GameObject muteToggleGo = AddToggle("Mute", new Vector3(), "Mute", new Vector2());
        Toggle muteToggle = muteToggleGo.GetComponent<Toggle>();
        muteToggle.isOn = false;

        // Create an instance of the ProductWorkflowManager
        productWorkflowManager = new ProductWorkflowManager();

        if (productWorkflowManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;

            // Toggle event listeners for role selection
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

        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(productWorkflowManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(productWorkflowManager.Join);

        // Setup an input field to get the name of the channel from the local user.
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

        // Add a value-change event to the volume control slider.
        volumeControl.onValueChanged.AddListener((value) => productWorkflowManager.ChangeVolume((int)value));

        // Add a click-event function to the screenShare button.
        shareScreenBtnGo.GetComponent<Button>().onClick.AddListener(ToggleScreenSharing);

        // Invoke muteRemoteAudio when the user taps the mute toggle.
        muteToggle.onValueChanged.AddListener((value) =>
        {
            productWorkflowManager.MuteRemoteAudio(value);
        });
    }

    public void ToggleScreenSharing()
    {
        if (isSharing)
        {
            productWorkflowManager.StopSharing();
            shareScreenBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Share Screen";
            isSharing = false;
        }
        else
        {
            productWorkflowManager.StartSharing();
            shareScreenBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Stop Sharing";
            isSharing = true;
        }
    }

    public override void OnDestroy()
    {
        // Clean up
        base.OnDestroy();
        productWorkflowManager.DestroyEngine();
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (shareScreenBtnGo)
            Destroy(shareScreenBtnGo.gameObject);
    }
}
