using UnityEngine;
using Agora.Rtc;

public class CloudProxyManager : AuthenticationWorkflowManager
{
    internal bool directConnectionFailed = false;

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Start cloud proxy service and set the UDP transmission mode.
        if (directConnectionFailed)
        {
            // Start cloud proxy service and set automatic UDP mode.
            int proxyStatus = agoraEngine.SetCloudProxy(CLOUD_PROXY_TYPE.UDP_PROXY);
            if (proxyStatus == 0)
            {
                Debug.Log("Proxy service setup successful");
            }
            else
            {
                Debug.Log("Proxy service setup failed with error :" + proxyStatus);
            }
        }

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
        if(state == CONNECTION_STATE_TYPE.CONNECTION_STATE_FAILED && reason == CONNECTION_CHANGED_REASON_TYPE.CONNECTION_CHANGED_JOIN_FAILED)
        {
            cloudProxy.directConnectionFailed = true;
            Debug.Log("Join failed, reason: " + reason);
        }
        else if (reason == CONNECTION_CHANGED_REASON_TYPE.CONNECTION_CHANGED_SETTING_PROXY_SERVER)
        {
            Debug.Log("Proxy server setting changed");
        }
    }
}