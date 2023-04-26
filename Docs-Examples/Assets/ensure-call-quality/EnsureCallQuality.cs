using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class EnsureCallQuality : MonoBehaviour
{
    // This provides all the required business logic to enable
    internal AgoraManager agoraManager;  // Instance of AgoraManager class
    internal Canvas canvas;  // Reference to Unity's UI canvas
    internal IRtcEngine RtcEngine;  // Agora's RtcEngine instance
    UserInterface UI;  // Instance of UserInterface class to manage the UI
        
    // Fill in your app ID.
    internal string _appID = "";
    // Fill in your channel name.
    internal string _channelName = "";
    // Fill in the temporary token you obtained from Agora Console.
    internal string _token = "";
        
    internal void Start()
    {
        // Add a canvas to the scene to add the UI elements
        canvas = SetupCanvas();
        // Create an object of AgoraManager
        agoraManager = new AgoraManager();
        // Setup UI
        UI = new UserInterface(canvas);
        UI.SetupUI();
        // Pass the local view and remote view to display videos
        agoraManager.LocalView = UI.LocalView;
        agoraManager.RemoteView = UI.RemoteView;
        // Call the join and leave function of AgoraManager class when the user presses the button from the UI. 
        UI.leaveBtn.GetComponent<Button>().onClick.AddListener(Leave);
        UI.joinBtn.GetComponent<Button>().onClick.AddListener(Join);
    }
    // Helper method to set up the UI canvas
    public Canvas SetupCanvas()
    {
        // Create a new GameObject and name it "Canvas"
        GameObject canvasObject = new GameObject("Canvas");
        // Add a Canvas component to the GameObject
        Canvas CanvasRef = canvasObject.AddComponent<Canvas>();
        // Set the render mode to Screen Space - Overlay
        CanvasRef.renderMode = RenderMode.ScreenSpaceOverlay;
        // Add a CanvasScaler component to the GameObject
        canvasObject.AddComponent<CanvasScaler>();
        // Add a GraphicRaycaster component to the GameObject
        canvasObject.AddComponent<GraphicRaycaster>();
        return CanvasRef;
    }
    // Button click event handler to leave the channel
    public void Leave()
    {
        // Leave Channel
        agoraManager.Leave();
        RtcEngine.Dispose();
        RtcEngine = null;
    } 
    // Button click event handler to join the channel
    public virtual void Join()
    {
        // Create an instance of Agora engine
        RtcEngine = agoraManager.SetupVideoSDKEngine(_appID, _channelName, _token);
        // Setup event handler to handle the events.
        CallQualityEventHandler obj = new CallQualityEventHandler(agoraManager);
        RtcEngine.InitEventHandler(obj);
        // Start the probe test
        startProbeTest();
        // Join a channel.
        agoraManager.Join();
    }
    public void startProbeTest() 
    {
        // Configure a LastmileProbeConfig instance.
        LastmileProbeConfig config = new LastmileProbeConfig();
        // Probe the uplink network quality.
        config.probeUplink = true;
        // Probe the downlink network quality.
        config.probeDownlink = true;
        // The expected uplink bitrate (bps). The value range is [100000,5000000].
        config.expectedUplinkBitrate = 100000;
        // The expected downlink bitrate (bps). The value range is [100000,5000000].
        config.expectedDownlinkBitrate = 100000;
        RtcEngine.StartLastmileProbeTest(config);
        Debug.Log("Running the last mile probe test ...");
    }
}
    
// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class CallQualityEventHandler : UserEventHandler
{
    internal CallQualityEventHandler(AgoraManager videoSample):base(videoSample) {}
    public override void OnLastmileProbeResult(LastmileProbeResult result) 
    {
        _videoSample.RtcEngine.StopLastmileProbeTest();
        Debug.Log("Probe test finished");
        // The result object contains the detailed test results that help you
        // manage call quality, for example, the downlink jitter.
        Debug.Log("Downlink jitter: " + result.downlinkReport.jitter);
    }
} 