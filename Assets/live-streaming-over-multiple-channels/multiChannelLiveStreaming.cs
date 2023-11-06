using Agora.Rtc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiChannelLiveStreaming : AgoraUI
{
    internal MultiChannelLiveStreamingManager multiChannelLiveStreamingManager;
    internal GameObject channelFieldGo, relayMediaBtnGo;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject secondChannelBtnGo;
    private bool isMediaRelaying = false;
    private bool isSecondChannel = false;


    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the MultiChannelLiveStreamingManager
        multiChannelLiveStreamingManager = new MultiChannelLiveStreamingManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        multiChannelLiveStreamingManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        relayMediaBtnGo = AddButton("RelayMediaBtn", new Vector3(165, -172, 0), "Relay Media", new Vector2(200f, 30f));
        secondChannelBtnGo = AddButton("joinSecondChannel", new Vector3(-19, -172, 0), "Join Second Channel", new Vector2(160, 30));

        // Check the product type to determine if host and audience toggles are needed
        if (multiChannelLiveStreamingManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements.
        joinBtnGo.GetComponent<Button>().onClick.AddListener(multiChannelLiveStreamingManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(multiChannelLiveStreamingManager.Leave);
        relayMediaBtnGo.GetComponent<Button>().onClick.AddListener(StartAndStopMediaPlaying);
        secondChannelBtnGo.GetComponent<Button>().onClick.AddListener(JoinLeaveSecondChannel);
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
                    multiChannelLiveStreamingManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    multiChannelLiveStreamingManager.SetClientRole("Audience");
                }
            });

        }

        // Add a listener to the channel input field to update the channel name in MultiChannelLiveStreamingManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Pass the channel name to the MultiChannelLiveStreamingManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        multiChannelLiveStreamingManager.configData.channelName = newValue;
    }

    public void StartAndStopMediaPlaying()
    {
        isMediaRelaying = !isMediaRelaying;
        if(isMediaRelaying)
        {
            relayMediaBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Stop Relaying";
            multiChannelLiveStreamingManager.StartChannelRelay();
        }
        else
        {
            relayMediaBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Relay Media";
            multiChannelLiveStreamingManager.StopChannelRelay();
        }
    }
    public void JoinLeaveSecondChannel()
    {
        isSecondChannel = !isSecondChannel;
        if(isSecondChannel)
        {
            secondChannelBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Leave Second Channel";
            multiChannelLiveStreamingManager.JoinSecondChannel();
        }
        else
        {
            secondChannelBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Join Second Channel";
            multiChannelLiveStreamingManager.LeaveSecondChannel();
        }

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        multiChannelLiveStreamingManager.DestroyEngine();
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
        if (secondChannelBtnGo)
            Destroy(secondChannelBtnGo.gameObject);
        if (relayMediaBtnGo)
            Destroy(relayMediaBtnGo.gameObject);
    }
}
