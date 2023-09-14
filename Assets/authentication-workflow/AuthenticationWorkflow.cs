using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;
using TMPro;


public class AuthenticationWorkflow : AgoraUI
{
    AuthenticationWorkflowManager authenticationWorkflowManager;
    internal GameObject channelField;
    internal GameObject audienceToggleGo, hostToggleGo;
    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        RemoteViewGo = MakeRemoteView("RemoteView",new Vector2(250, 250));

        // Create an instance of the AgoraManagerGetStarted
        authenticationWorkflowManager = new AuthenticationWorkflowManager(LocalViewGo, RemoteViewGo);

        if (authenticationWorkflowManager.configData.product != "Video Calling")
        {
            hostToggleGo = AddToggle("Host", new Vector2(-19, 50), "Host", new Vector2(200, 30));
            audienceToggleGo = AddToggle("Audience", new Vector2(-19, 100), "Audience", new Vector2(200, 30));
            Toggle audienceToggle = audienceToggleGo.GetComponent<Toggle>();
            Toggle hostToggle = hostToggleGo.GetComponent<Toggle>();
            hostToggle.isOn = false;
            audienceToggle.isOn = false;
            hostToggle.onValueChanged.AddListener((value) =>
            {
                audienceToggle.isOn = !value;
                authenticationWorkflowManager.setClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                authenticationWorkflowManager.setClientRole("Audience");
            });
        }

        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(authenticationWorkflowManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(authenticationWorkflowManager.Join);

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
    }
    public override void OnDestroy()
    {
        if(channelField)
            Destroy(channelField.gameObject);
        base.OnDestroy();
        authenticationWorkflowManager.OnDestroy();
        if(channelField)
            Destroy(channelField.gameObject);
        if(audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(RemoteViewGo.gameObject);
    }
    
}

