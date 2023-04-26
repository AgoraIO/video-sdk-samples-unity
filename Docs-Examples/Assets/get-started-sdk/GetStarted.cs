using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class GetStarted : MonoBehaviour
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
    public void Join()
    {
        // Create an instance of Agora engine
        RtcEngine = agoraManager.SetupVideoSDKEngine(_appID, _channelName, _token);
        // Setup event handler to handle the events.
        UserEventHandler obj = new UserEventHandler(agoraManager);
        RtcEngine.InitEventHandler(obj);
        // Join a channel.
        agoraManager.Join();
    }
}
    
// Event handler class to handle the events raised by Agora's RtcEngine instance
internal class GetStartedEventHandler : UserEventHandler
{
    internal GetStartedEventHandler(AgoraManager videoSample):base(videoSample) {}
}  
