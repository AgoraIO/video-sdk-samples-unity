using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class RawAudioVideo : AgoraUI
{
    internal RawAudioVideoManager rawAudioVideoManager;
    internal GameObject channelFieldGo;
    internal GameObject audienceToggleGo, hostToggleGo, zoomBtnGo, playAudioFrameBtn;
    bool isZoomIn = false;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the RawAudioVideoManager
        rawAudioVideoManager = new RawAudioVideoManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        // The remote video surface is added when a remote user join the channel.
        rawAudioVideoManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();

    }

    public override void Update()
    {
        base.Update();
        rawAudioVideoManager.ResizeVideoFrame();
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
        zoomBtnGo = AddButton("ZoomBtn", new Vector3(185, -172, 0), "Zoom In", new Vector2(160f, 30f));
        playAudioFrameBtn = AddButton("playAudioFrame", new Vector3(24, -172, 0), "Process audio frame", new Vector2(160f, 30f));

        // Check the product type to determine if host and audience toggles are needed
        if (rawAudioVideoManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements
        joinBtnGo.GetComponent<Button>().onClick.AddListener(rawAudioVideoManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(rawAudioVideoManager.Leave);
        zoomBtnGo.GetComponent<Button>().onClick.AddListener(ZoomInOut);
        playAudioFrameBtn.GetComponent<Button>().onClick.AddListener(ProcessAudioFrame);


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
                    rawAudioVideoManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    rawAudioVideoManager.SetClientRole("Audience");
                }
            });

        }
    }

    // OnDestroy is called when the component is being destroyed
    public override void OnDestroy()
    {
        base.OnDestroy();
        rawAudioVideoManager.DestroyEngine();
        DestroyUIElements();
    }

    public void ZoomInOut()
    {
        isZoomIn = !isZoomIn;
        if(isZoomIn)
        {
            zoomBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Zoom Out";
            rawAudioVideoManager._videoFrameWidth = 250;
            rawAudioVideoManager._videoFrameHeight = 250;
        }
        else
        {
            zoomBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Zoom In";
            rawAudioVideoManager._videoFrameWidth = 1080;
            rawAudioVideoManager._videoFrameHeight = 720;
        }
        rawAudioVideoManager.SetVideoEncoderConfiguration();

    }

    public void ProcessAudioFrame()
    {
        rawAudioVideoManager.isPlaying = !rawAudioVideoManager.isPlaying;
        if (rawAudioVideoManager.isPlaying)
        {
            zoomBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Stop processing";
        }
        else
        {
            zoomBtnGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Process Audio Frame";
        }
        rawAudioVideoManager.SetVideoEncoderConfiguration();

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
        if (zoomBtnGo)
            Destroy(zoomBtnGo.gameObject);
    }
}
