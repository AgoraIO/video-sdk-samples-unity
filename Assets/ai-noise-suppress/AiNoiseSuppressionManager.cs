using Agora.Rtc;

public class AiNoiseSuppressionManager : AuthenticationWorkflowManager
{

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();

        // Enable AI noise suppression when uopu configue the engine.
        agoraEngine.SetAINSMode(true, AUDIO_AINS_MODE.AINS_MODE_AGGRESSIVE);

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
