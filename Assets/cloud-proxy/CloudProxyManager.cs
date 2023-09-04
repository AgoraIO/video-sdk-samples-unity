using UnityEngine;
using Agora.Rtc;

public class CloudProxyManager : AuthenticationWorkflowManager
{
    // Start is called before the first frame update
    public CloudProxyManager(GameObject LocalViewGo, GameObject RemoteViewGo):base(LocalViewGo, RemoteViewGo)
    {
        // Check if the required permissions are granted
        CheckPermissions();
    }
    public override void Join()
    {
        base.Join();

        // Create an instance of the engine.
        SetupAgoraEngine();

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
        InitEventHandler();
    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
        // Destroy the engine.
        if (RtcEngine != null)
        {
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }
}


// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class CloudProxyEventHandler : UserEventHandler
{
    private CloudProxyManager cloudProxy;
    internal CloudProxyEventHandler(CloudProxyManager videoSample):base(videoSample) 
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