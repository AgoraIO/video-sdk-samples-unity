using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Agora.Rtc;

public class AgoraManagerCloudProxy : AgoraManager
{  
    // Start is called before the first frame update
     public AgoraManagerCloudProxy(VideoSurface LocalVideoSurface, VideoSurface RemoteVideoSurface)
    {
        LocalView = LocalVideoSurface;
        RemoteView = RemoteVideoSurface;
        // Check if the required permissions are granted
        CheckPermissions();

        // Create an instance of the engine.
        SetupVideoSDKEngine();

        // Start cloud proxy service and set automatic transmission mode.
        int proxyStatus = RtcEngine.SetCloudProxy(CLOUD_PROXY_TYPE.UDP_PROXY);
        if (proxyStatus == 0) 
        {
            Debug.Log("Proxy service started successfully");
        } 
        else 
        {
            Debug.Log("Proxy service failed with error :" + proxyStatus);
        }

        // Setup an event handler to receive callbacks.
        RtcEngine.InitEventHandler(new CloudProxyEventHandler(this));

    }   
}


// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class CloudProxyEventHandler : UserEventHandler
{
    private AgoraManagerCloudProxy cloudProxy;
    internal CloudProxyEventHandler(AgoraManagerCloudProxy videoSample):base(videoSample) 
    {
        cloudProxy = videoSample;
    }
    public override void OnConnectionStateChanged(RtcConnection connection, CONNECTION_STATE_TYPE state, CONNECTION_CHANGED_REASON_TYPE reason) 
    {
        Debug.Log("Connection state changed"
             + "\n New state: " + state
             + "\n Reason: " + reason);
    }
}