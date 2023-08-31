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
    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalView = MakeLocalView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        RemoteView = MakeRemoteView("RemoteView",new Vector2(250, 250));

        // Add video surfaces to the local and remote views
        VideoSurface LocalVideoSurface = LocalView.AddComponent<VideoSurface>();
        VideoSurface RemoteVideoSurface = RemoteView.AddComponent<VideoSurface>();
        // Create an instance of the AgoraManagerGetStarted
        authenticationWorkflowManager = new AuthenticationWorkflowManager(LocalVideoSurface, RemoteVideoSurface);
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
    }
    
}

