
// Import Unity and Agora libraries
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class UserInterface : MonoBehaviour
{
    // Declare variables for UI elements
    internal Canvas canvas;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal GameObject joinBtn;
    internal GameObject leaveBtn;
    // Start is called before the first frame update
    public virtual void Start() {}
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
    // Update is called once per frame
    public virtual void Update() {}
    // Method that adds the local video view to the UI
    public virtual VideoSurface MakeView(string VName, Vector3 VPos, Vector2 VSize)
    {
        // Create a new raw image object
        GameObject go = new GameObject(VName, typeof(RawImage));

        // Get the RectTransform component and set its parent, rotation, position, scale, and sizeDelta properties
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(canvas.transform); // Set the parent to the canvas
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f); // Set the rotation to face the camera
        rectTransform.localPosition = VPos; // Set the position
        rectTransform.localScale = Vector3.one; // Set the scale to (1, 1, 1)
        rectTransform.sizeDelta = VSize; // Set the width and height of the RawImage
        // Add VideoSurface component to the game object
        var View = go.AddComponent<VideoSurface>();
        return View;
    }
    // Method that adds the join and leave buttons to the UI
    public virtual GameObject AddButton(string BName, Vector3 BPos, string BText, Vector2 BSize)
    {
        // Create resources object for creating buttons
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        // Create Join button
        GameObject Button = TMP_DefaultControls.CreateButton(resources);
        Button.name = BName;
        Button.transform.SetParent(canvas.transform, false);
        Button.transform.localPosition = BPos;
        Button.transform.localScale = Vector3.one;
        RectTransform rectTransform = Button.GetComponent<RectTransform>();
        rectTransform.sizeDelta = BSize;
        Button.GetComponentInChildren<TMP_Text>().text = BText;
        return Button;
    }   
}
