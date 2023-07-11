using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;
using TMPro;


public class AuthenticationWorkflow : AgoraUI
{
    AgoraManagerAuthenticationWorkflow AuthenticationWorkflowManager;
    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalView = MakeView("LocalView", new Vector3(-250, 0, 0), new Vector2(250, 250));
        RemoteView = MakeView("RemoteView", new Vector3(250, 0, 0), new Vector2(250, 250));

        // Add video surfaces to the local and remote views
        VideoSurface LocalVideoSurface = LocalView.AddComponent<VideoSurface>();
        VideoSurface RemoteVideoSurface = RemoteView.AddComponent<VideoSurface>();

        // Create an instance of the AgoraManagerGetStarted
        AuthenticationWorkflowManager = new AgoraManagerAuthenticationWorkflow(LocalVideoSurface, RemoteVideoSurface);

        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(AuthenticationWorkflowManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(AuthenticationWorkflowManager.Join);

        // Create an input field to input the channel name.
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
        AuthenticationWorkflowManager.OnDestroy();
    }
    
}

