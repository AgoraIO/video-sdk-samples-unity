using Agora.Rtc;
using UnityEngine;

public class GeofencingManager : AuthenticationWorkflowManager
{

    public override void SetupAgoraEngine()
    {
        // Set the region of your choice.
        context.areaCode = AREA_CODE.AREA_CODE_CN;

        base.SetupAgoraEngine();

    }
    public override void Join()
    {
        //Join the channel.
        base.Join();
    }
    public override void Leave()
    {
        // Leave the channel.
        base.Leave();
    }
}

