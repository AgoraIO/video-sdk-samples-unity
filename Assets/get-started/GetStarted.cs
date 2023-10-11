using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;

public class GetStarted : AgoraUI
{
    internal GetStartedManager getStartedManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the AgoraManagerGetStarted
        getStartedManager = new GetStartedManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        // The remote video surface is added when a remote user join the channel.
        getStartedManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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

        // Check the product type to determine if host and audience toggles are needed
        if (getStartedManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>().onClick.AddListener(getStartedManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(getStartedManager.Leave);

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
                    getStartedManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    getStartedManager.SetClientRole("Audience");
                }
            });

        }
    }

    // OnDestroy is called when the component is being destroyed
    public override void OnDestroy()
    {
        base.OnDestroy();
        getStartedManager.DestroyEngine();
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
    }
}
