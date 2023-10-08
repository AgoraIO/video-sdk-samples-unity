using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Agora.Rtc;

public class EnsureCallQuality : AgoraUI
{
    GameObject deviceTestGo;
    GameObject videoQualityGo;
    GameObject networkStatusGo;
    GameObject audioDevicesDropdownGo;
    GameObject videoDevicesDropdownGo;
    GameObject channelFieldGo;
    CallQualityManager callQualityManager;
    internal GameObject audienceToggleGo, hostToggleGo;
    bool isDeviceTest = false;
    bool isHighQuality = false;
    string selectedAudioDevice = "";
    string selectedVideoDevice = "";


    // Start is called before the first frame update
    public override void Start()
    {
        // Create an instance of the CallQualityManager
        callQualityManager = new CallQualityManager();

        // Setup UI elements
        SetupUI();

        // Attach a video surface to the local view game object.
        callQualityManager.LocalView = LocalViewGo.AddComponent<VideoSurface>();
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
        joinBtnGo = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(100, 30f));
        leaveBtnGo = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(100, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        deviceTestGo = AddButton("testDevicesBtn", new Vector3(218, -172, 0), "Start device test", new Vector2(160f, 30f));
        videoQualityGo = AddButton("videoQualityBtn", new Vector3(336, 172, 0), "High Video Quality", new Vector2(130, 30f));

        audioDevicesDropdownGo = AddDropdown("audioDevicesDropdown", new Vector2(-191, -172), new Vector2(215, 30));
        videoDevicesDropdownGo = AddDropdown("videoDevicesDropdown", new Vector2(27, -172), new Vector2(215, 30));
        networkStatusGo = AddLabel("networkStatus", new Vector2(370, 100), "Network Quality: ");

        // Toggles to switch the user roles
        if (callQualityManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
        }
        channelFieldGo = AddInputField("channelName", new Vector3(0, 0, 0), "Channel Name");

        // Attach event listeners to UI elements
        if (hostToggleGo && audienceToggleGo)
        {
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                callQualityManager.SetClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                callQualityManager.SetClientRole("Audience");
            });
        }

        // Add events listerners to the UI elements.
        leaveBtnGo.GetComponent<Button>().onClick.AddListener(callQualityManager.Leave);
        joinBtnGo.GetComponent<Button>().onClick.AddListener(callQualityManager.Join);
        deviceTestGo.GetComponent<Button>().onClick.AddListener(ToggleDeviceTest);
        videoQualityGo.GetComponent<Button>().onClick.AddListener(ToggleStreamQuality);

        // Add a listener to the channel input field to update the channel name in CallQualityManager
        TMP_InputField tmpInputField = channelFieldGo.GetComponent<TMP_InputField>();
        tmpInputField.onValueChanged.AddListener(HandleChannelFieldChange);
    }

    // Function to toggle stream quality between high and low
    public void ToggleStreamQuality()
    {
        if (isHighQuality)
        {
            videoQualityGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Low Video Quality";
            isHighQuality = false;
            callQualityManager.SetLowStreamQuality();
        }
        else
        {
            videoQualityGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "High Video Quality";
            isHighQuality = true;
            callQualityManager.SetHighStreamQuality();
        }
    }

    // Function to toggle device test
    public void ToggleDeviceTest()
    {
        TMP_Dropdown audioDeviceDropdown = audioDevicesDropdownGo.GetComponent<TMP_Dropdown>();
        selectedAudioDevice = audioDeviceDropdown.options[audioDeviceDropdown.value].text;

        TMP_Dropdown videoDeviceDropdown = videoDevicesDropdownGo.GetComponent<TMP_Dropdown>();
        selectedVideoDevice = videoDeviceDropdown.options[audioDeviceDropdown.value].text;

        if (isDeviceTest)
        {
            isDeviceTest = false;
            deviceTestGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Start device test";
            callQualityManager.StopAudioVideoDeviceTest();
        }
        else
        {
            isDeviceTest = true;
            deviceTestGo.GetComponentInChildren<TextMeshProUGUI>(true).text = "Stop device test";
            callQualityManager.StartAudioVideoDeviceTest(selectedAudioDevice, selectedVideoDevice);
        }
    }

    // Pass the channel name to the CallQualityManager class to fetch a token from the token server
    private void HandleChannelFieldChange(string newValue)
    {
        callQualityManager.configData.channelName = newValue;
    }

    public override void Update()
    {
        base.Update();
        TMP_Dropdown videoDevicesDropdown = GameObject.Find("videoDevicesDropdown").GetComponent<TMP_Dropdown>();
        videoDevicesDropdown.ClearOptions();
        videoDevicesDropdown.AddOptions(callQualityManager.videoDevices);

        TMP_Dropdown audioDevicesDropdown = GameObject.Find("audioDevicesDropdown").GetComponent<TMP_Dropdown>();
        audioDevicesDropdown.ClearOptions();
        audioDevicesDropdown.AddOptions(callQualityManager.audioDevices);

        TextMeshProUGUI networkStatus = networkStatusGo.GetComponent<TextMeshProUGUI>();
        networkStatus.text = callQualityManager.networkStatus;

    }

    private void DestroyUIElements()
    {
        // Destroy UI elements
        if (deviceTestGo)
            Destroy(deviceTestGo.gameObject);
        if (videoQualityGo)
            Destroy(videoQualityGo.gameObject);
        if (networkStatusGo)
            Destroy(networkStatusGo.gameObject);
        if (audioDevicesDropdownGo)
            Destroy(audioDevicesDropdownGo.gameObject);
        if (videoDevicesDropdownGo)
            Destroy(videoDevicesDropdownGo.gameObject);
        if (audienceToggleGo)
            Destroy(audienceToggleGo.gameObject);
        if (hostToggleGo)
            Destroy(hostToggleGo.gameObject);
        if (channelFieldGo)
            Destroy(channelFieldGo.gameObject);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        callQualityManager.DestroyEngine();
        DestroyUIElements();
    }
}
