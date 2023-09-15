using UnityEngine;
using Agora.Rtc;

public class CloudProxyManager : AuthenticationWorkflowManager
{

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Start cloud proxy service and set automatic transmission mode.
        int proxyStatus = agoraEngine.SetCloudProxy(CLOUD_PROXY_TYPE.UDP_PROXY);
        if (proxyStatus == 0)
        {
            Debug.Log("Proxy service started successfully");
        }
        else
        {
            Debug.Log("Proxy service failed with error :" + proxyStatus);
        }
        agoraEngine.InitEventHandler(new CloudProxyEventHandler(this));

    }

    // Join the channel
    public override void Join()
    {
        base.Join();
    }

    // Leave the channel.
    public override void Leave()
    {
        base.Leave();        
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