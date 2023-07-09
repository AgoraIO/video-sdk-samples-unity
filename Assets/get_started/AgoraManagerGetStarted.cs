using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;

public class AgoraManagerGetStarted : AgoraManager
{  
    // Start is called before the first frame update
     public AgoraManagerGetStarted(VideoSurface LocalVideoSurface, VideoSurface RemoteVideoSurface)
    {
        LocalView = LocalVideoSurface;
        RemoteView = RemoteVideoSurface;
        // Check if the required permissions are granted
        CheckPermissions();

        // Create an instance of the engine.
        SetupVideoSDKEngine();

        // Setup an event handler to receive callbacks.
        InitEventHandler();

    }
    
}
