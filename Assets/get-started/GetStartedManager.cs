using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;

public class GetStartedManager : AgoraManager
{  
    // Start is called before the first frame update
    public GetStartedManager(GameObject localViewGo, GameObject RemoteVideoGo): base()
    {
        LocalView = localViewGo.AddComponent<VideoSurface>();
        RemoteView = RemoteVideoGo.AddComponent<VideoSurface>();

        // Check if the required permissions are granted
        CheckPermissions();

    }
    public override void Join()
    {
        // Create an instance of the engine.
        SetupAgoraEngine();

        // Setup an event handler to receive callbacks.
        InitEventHandler();

        //Join the channel.
        base.Join();
    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
        // Destroy the engine.
        if (RtcEngine != null)
            RtcEngine.Dispose();
            RtcEngine = null;
    }
}
