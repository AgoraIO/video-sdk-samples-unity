using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;
using TMPro;

public class EnsureCallQuality : AgoraUI
{
    GameObject deviceTestBtn;
    GameObject videoQualityBtn;
    GameObject networkStatusObj;
    GameObject audioDevicesDropdown;
    GameObject videoDevicesDropdown;
    CallQualityManager callQualityManager;
    // Start is called before the first frame update
    public void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalView = MakeLocalView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        RemoteView = MakeRemoteView("RemoteView", new Vector2(250, 250));

        // Add a TMP_Dropdown component for audio devices
        audioDevicesDropdown = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        audioDevicesDropdown.name = "audioDevicesDropdown";
        audioDevicesDropdown.transform.SetParent(canvas.transform);
        audioDevicesDropdown.transform.localPosition = new Vector2(-187, -172);
        audioDevicesDropdown.transform.localScale = Vector3.one;

        // Add a TMP_Dropdown component for video devices
        videoDevicesDropdown = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        videoDevicesDropdown.name = "videoDevicesDropdown";
        videoDevicesDropdown.transform.SetParent(canvas.transform);
        videoDevicesDropdown.transform.localPosition = new Vector2(-30, -172);
        videoDevicesDropdown.transform.localScale = Vector3.one;

        // Add a network status label
        networkStatusObj = new GameObject("networkStatus");
        TextMeshProUGUI networkStatus = networkStatusObj.AddComponent<TextMeshProUGUI>();
        networkStatus.transform.SetParent(canvas.transform);
        networkStatus.transform.localPosition = new Vector2(200, 150);
        networkStatus.text = "Network Quality: ";
        networkStatus.fontSize = 14;

        // Add a button to test audio and video devices
        deviceTestBtn = AddButton("testDevicesBtn", new Vector3(162, -172, 0), "Start device test", new Vector2(200f, 30f));

        // Add a button to switch video quality
        videoQualityBtn = AddButton("videoQualityBtn", new Vector3(350, 172, 0), "Low Video Quality", new Vector2(130, 30f));

        // Add video surfaces to the local and remote views
        VideoSurface LocalVideoSurface = LocalView.AddComponent<VideoSurface>();
        VideoSurface RemoteVideoSurface = RemoteView.AddComponent<VideoSurface>();
        callQualityManager = new CallQualityManager(LocalVideoSurface, LocalVideoSurface);
        // Attach event listeners to buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(callQualityManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(callQualityManager.Join);
        deviceTestBtn.GetComponent<Button>().onClick.AddListener(callQualityManager.testAudioAndVideoDevice);
        videoQualityBtn.GetComponent<Button>().onClick.AddListener(callQualityManager.setStreamQuality);

        // Add an input field to input a channel name
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject inputFieldObj = TMP_DefaultControls.CreateInputField(resources);
        inputFieldObj.name = "channelName";
        inputFieldObj.transform.SetParent(canvas.transform, false);

        TMP_InputField tmpInputField = inputFieldObj.GetComponent<TMP_InputField>();
        RectTransform inputFieldTransform = tmpInputField.GetComponent<RectTransform>();
        inputFieldTransform.sizeDelta = new Vector2(200, 30);

        TMP_Text textComponent = inputFieldObj.GetComponentInChildren<TMP_Text>();
        textComponent.alignment = TextAlignmentOptions.Center;

        // Change the placeholder text
        tmpInputField.placeholder.GetComponent<TMP_Text>().text = "Channel Name";
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Destroy UI elements
        if (deviceTestBtn)
            Destroy(deviceTestBtn.gameObject);
        if (videoQualityBtn)
            Destroy(videoQualityBtn.gameObject);
        if (networkStatusObj)
            Destroy(networkStatusObj.gameObject);
        if (audioDevicesDropdown)
            Destroy(audioDevicesDropdown.gameObject);
        if (videoDevicesDropdown)
            Destroy(videoDevicesDropdown.gameObject);
        if(callQualityManager != null)
            callQualityManager.OnDestroy();
    }    
}