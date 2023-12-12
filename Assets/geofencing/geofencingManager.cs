using Agora.Rtc;

public class GeofencingManager : AuthenticationWorkflowManager
{

    public override void SetupAgoraEngine()
    {
        // Set the region of your choice.
        region = AREA_CODE.AREA_CODE_CN;

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
