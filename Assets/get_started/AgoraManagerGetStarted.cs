using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;

public class AgoraManagerGetStarted : AgoraManager
{  
    // Start is called before the first frame update
    public AgoraManagerGetStarted()
    {
        // Check if the required permissions are granted
        CheckPermissions();

        // Add video surfaces to the local and remote views
        LocalView = GameObject.Find("LocalView").AddComponent<VideoSurface>();
        RemoteView = GameObject.Find("RemoteView").AddComponent<VideoSurface>();
    }
}
