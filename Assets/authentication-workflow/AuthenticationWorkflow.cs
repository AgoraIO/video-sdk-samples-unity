using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;

public class AuthenticationWorkflow : AgoraUI
{
    internal AuthenticationWorkflowManager authenticationWorkflowManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the AgoraManagerGetStarted
        authenticationWorkflowManager = new AuthenticationWorkflowManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        // The remote video surface is added when a remote user join the channel.
        authenticationWorkflowManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");

        // Check the product type to determine if host and audience toggles are needed
        if (authenticationWorkflowManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>().onClick.AddListener(authenticationWorkflowManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(authenticationWorkflowManager.Leave);

        if (hostToggleGo && audienceToggleGo)
        {
            // Toggle event listeners for role selection
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                authenticationWorkflowManager.SetClientRole("Host");
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                authenticationWorkflowManager.SetClientRole("Audience");
            });

        }

        // Add a listener to the channel input field to update the channel name in AuthenticationWorkflowManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Pass the channel name to the AuthenticationWorkflowManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        authenticationWorkflowManager.configData.channelName = newValue;
    }

    // Function to destroy UI elements
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

    public override void OnDestroy()
    {
        base.OnDestroy();
        authenticationWorkflowManager.DestroyEngine();
        DestroyUIElements();
    }
}
