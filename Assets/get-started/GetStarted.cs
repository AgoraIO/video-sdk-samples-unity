using UnityEngine;
using UnityEngine.UI;


public class GetStarted : AgoraUI
{
    internal GetStartedManager getStartedManager;
    internal GameObject audienceToggleGo, hostToggleGo;

    // Start is called before the first frame update
    public override void Start()
    {
        // Setup UI
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        // Create and position UI elements
        joinBtn = AddButton("Join", new Vector3(-350, -172, 0), "Join", new Vector2(160f, 30f));
        leaveBtn = AddButton("Leave", new Vector3(350, -172, 0), "Leave", new Vector2(160f, 30f));
        LocalViewGo = MakeLocalView("LocalView", new Vector3(-250, -2, 0), new Vector2(271, 294));
        RemoteViewGo = MakeRemoteView("RemoteView", new Vector2(250, 250));

        // Create an instance of the AgoraManagerGetStarted
        getStartedManager = new GetStartedManager(LocalViewGo, RemoteViewGo);

        if (getStartedManager.configData.product != "Video Calling")
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
                getStartedManager.setClientRole("Host");
            });

            audienceToggle.onValueChanged.AddListener((value) =>
            {
                hostToggle.isOn = !value;
                getStartedManager.setClientRole("Audience");
            });
        }


        // Add click-event functions to the join and leave buttons
        leaveBtn.GetComponent<Button>().onClick.AddListener(getStartedManager.Leave);
        joinBtn.GetComponent<Button>().onClick.AddListener(getStartedManager.Join);
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        getStartedManager.OnDestroy();
        if (audienceToggleGo)
            Destroy(LocalViewGo.gameObject);
        if (hostToggleGo)
            Destroy(RemoteViewGo.gameObject);

    }
}
