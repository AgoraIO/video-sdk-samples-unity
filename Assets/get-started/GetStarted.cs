using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;
using UnityEngine.UI;


public class GetStarted : AgoraUI
{
    GetStartedManager getStartedManager;
    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalView = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        RemoteView = MakeRemoteView("RemoteView", new Vector2(250, 250));

        // Add video surfaces to the local and remote views
        VideoSurface LocalVideoSurface = LocalView.AddComponent<VideoSurface>();
        VideoSurface RemoteVideoSurface = RemoteView.AddComponent<VideoSurface>();
        // Create an instance of the AgoraManagerGetStarted
        getStartedManager = new GetStartedManager(LocalVideoSurface, RemoteVideoSurface);
        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(getStartedManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(getStartedManager.Join);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        getStartedManager.OnDestroy();
    }
}
