using Agora.Rtc;
using UnityEngine;

public class SpatialAudioManager : AgoraManager
{
    private ILocalSpatialAudioEngine localSpatial;

    public override void SetupAgoraEngine()
    {
        base.SetupAgoraEngine();
        ConfigureSpatialAudioEngine();
    }

    private void ConfigureSpatialAudioEngine()
    {
        agoraEngine.EnableSpatialAudio(true);
        LocalSpatialAudioConfig localSpatialAudioConfig = new LocalSpatialAudioConfig();
        localSpatialAudioConfig.rtcEngine = agoraEngine;
        localSpatial = agoraEngine.GetLocalSpatialAudioEngine();
        localSpatial.Initialize();
        // By default Agora subscribes to the audio streams of all remote users.
        // Unsubscribe all remote users; otherwise, the audio reception range you set
        // is invalid.
        localSpatial.MuteLocalAudioStream(true);
        localSpatial.MuteAllRemoteAudioStreams(true);

        // Set the audio reception range, in meters, of the local user
        localSpatial.SetAudioRecvRange(50);

        // Set the length, in meters, of unit distance
        localSpatial.SetDistanceUnit(1);

        // Update self position
        float[] pos = new float[] { 0.0F, 0.0F, 0.0F };
        float[] forward = new float[] { 1.0F, 0.0F, 0.0F };
        float[] right = new float[] { 0.0F, 1.0F, 0.0F };
        float[] up = new float[] { 0.0F, 0.0F, 1.0F };
        // Set the position of the local user
        localSpatial.UpdateSelfPosition(pos, forward, right, up);
    }

    public void UpdateSpatialAudioPosition(float sourceDistance)
    {
        if (remoteUid < 1)
        {
            Debug.Log("No remote user in the channel");
            return;
        }
        // Set the coordinates in the world coordinate system.
        // This parameter is an array of length 3
        // The three values represent the front, right, and top coordinates
        float[] position = new float[] { sourceDistance, 4.0F, 0.0F };
        // Set the unit vector of the x-axis in the coordinate system.
        // This parameter is an array of length 3,
        // The three values represent the front, right, and top coordinates
        float[] forward = new float[] { sourceDistance, 0.0F, 0.0F };
        // Update the spatial position of the specified remote user
        RemoteVoicePositionInfo remotePosInfo = new RemoteVoicePositionInfo(position, forward);
        int res = localSpatial.UpdateRemotePosition((uint)remoteUid, remotePosInfo);
        if (res == 0)
        {
            Debug.Log("Remote user spatial position updated");
        }
        else
        {
            Debug.Log("Updating position failed with error: " + res);
        }
    }
    public override void Leave()
    {
        if(localSpatial != null)
        {
            localSpatial.ClearRemotePositions();
        }
        base.Leave();
    }
}
