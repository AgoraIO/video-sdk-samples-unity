using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class VirtualBackground : AgoraUI
{
    internal VirtualBackgroundManager virtualBackgroundManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo;
    internal GameObject virtualBackgroundGo;
    bool isVirtualBackGroundEnabled = false;

    int counter = 0; // to cycle through the different types of backgrounds

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the VirtualBackgroundManager
        virtualBackgroundManager = new VirtualBackgroundManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        // The remote video surface is added when a remote user join the channel.
        virtualBackgroundManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        virtualBackgroundGo = AddButton("VirtualBackground", new Vector3(-165, -172, 0), "Enable Virtual Background", new Vector2(200, 30f));

        // Check the product type to determine if host and audience toggles are needed
        if (virtualBackgroundManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>().onClick.AddListener(virtualBackgroundManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(virtualBackgroundManager.Leave);
        virtualBackgroundGo.GetComponent<Button>().onClick.AddListener(SetVirtualBackground);

        // Check if audienceToggleGo and hostToggleGo exist before adding listeners
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
                    virtualBackgroundManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    virtualBackgroundManager.SetClientRole("Audience");
                }
            });

        }
        // Add a listener to the channel input field to update the channel name in VirtualBackgroundManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }
    // Pass the channel name to the VirtualBackgroundManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        virtualBackgroundManager.configData.channelName = newValue;
    }
    public void SetVirtualBackground()
    {
        TMP_Text BtnText = virtualBackgroundGo.GetComponentInChildren<TextMeshProUGUI>(true);
        // Options for virtual background
        string[] options = { "Color", "Blur", "Image" };

        if (counter >= 3)
        {
            isVirtualBackGroundEnabled = false;
            BtnText.text = "Enable Virtual Background";
            counter = 0;
            Debug.Log("Virtual background turned off");
        }
        else
        {
            isVirtualBackGroundEnabled = true;
            BtnText.text = "Background :" + options[counter];
        }
        // Set the virtual background
        virtualBackgroundManager.setVirtualBackground(isVirtualBackGroundEnabled, options[counter]);
        counter++;
    }

    // OnDestroy is called when the component is being destroyed
    public override void OnDestroy()
    {
        base.OnDestroy();
        virtualBackgroundManager.DestroyEngine();
        DestroyUIElements();
    }

    // Function to destroy UI elements
    private void DestroyUIElements()
    {
        // Destroy UI elements, e.g., audienceToggleGo, hostToggleGo
        if (audienceToggleGo)
            Destroy(audienceToggleGo.gameObject);
        if (LocalViewGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (virtualBackgroundGo)
            Destroy(virtualBackgroundGo.gameObject);
        if (channelFieldGo)
            Destroy(channelFieldGo.gameObject);
    }
}
