using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;

public class SpatialAudio : AgoraUI
{
    internal SpatialAudioManager spatialAudioManager;
    internal GameObject channelFieldGo, sliderGo;
    internal GameObject audienceToggleGo, hostToggleGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the SpatialAudioManager
        spatialAudioManager = new SpatialAudioManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        spatialAudioManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        sliderGo = AddSlider("distanceSlider", new Vector2(319, 171));


        // Check the product type to determine if host and audience toggles are needed
        if (spatialAudioManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }

        // Add event functions to UI elements.
        joinBtnGo.GetComponent<Button>().onClick.AddListener(spatialAudioManager.Join);
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(spatialAudioManager.Leave);

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
                    spatialAudioManager.SetClientRole("Host");
                }
            });
            audienceToggle.onValueChanged.AddListener((value) =>
            {
                if (value == true)
                {
                    hostToggle.isOn = false;
                    spatialAudioManager.SetClientRole("Audience");
                }
            });

        }

        // Add a listener to the channel input field to update the channel name in SpatialAudioManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);

        // Reference the slider from the UI
        Slider distanceSlider = sliderGo.GetComponent<Slider>();

        // Specify a minimum and maximum value for slider.
        distanceSlider.maxValue = 10;
        distanceSlider.minValue = 0;

        // Add a listener to the slider and which invokes distanceSlider when the slider is dragged left or right.
        distanceSlider.onValueChanged.AddListener(delegate { spatialAudioManager.UpdateSpatialAudioPosition((int)distanceSlider.value); });

    }

    // Pass the channel name to the SpatialAudioManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        spatialAudioManager.configData.channelName = newValue;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        spatialAudioManager.DestroyEngine();
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
        if (sliderGo)
            Destroy(sliderGo.gameObject);
    }
}
