using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class UserInterface : MonoBehaviour
{
    // References to UI elements
    internal Canvas canvas;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal GameObject joinBtn;
    internal GameObject leaveBtn;

    // Set up the canvas
    public Canvas SetupCanvas()
    {
        // Create the canvas object
        Canvas CanvasRef = new GameObject("Canvas").AddComponent<Canvas>();
        // Set canvas settings
        CanvasRef.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasRef.gameObject.AddComponent<CanvasScaler>();
        CanvasRef.gameObject.AddComponent<GraphicRaycaster>();
        // Return the canvas reference
        return CanvasRef;
    }

    // Create a video view
    public virtual VideoSurface MakeView(string VName, Vector3 VPos, Vector2 VSize)
    {
        // Create the game object with a RawImage component
        GameObject go = new GameObject(VName, typeof(RawImage));
        // Set the RectTransform settings
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(canvas.transform);
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        rectTransform.localPosition = VPos;
        rectTransform.localScale = Vector3.one;
        rectTransform.sizeDelta = VSize;
        // Add the VideoSurface component and return it
        return go.AddComponent<VideoSurface>();
    }

    // Create a button
    public virtual GameObject AddButton(string BName, Vector3 BPos, string BText, Vector2 BSize)
    {
        // Create the button game object using the TMP_DefaultControls utility class
        GameObject Button = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        // Set the game object settings
        Button.name = BName;
        Button.transform.SetParent(canvas.transform, false);
        Button.transform.localPosition = BPos;
        Button.transform.localScale = Vector3.one;
        RectTransform rectTransform = Button.GetComponent<RectTransform>();
        rectTransform.sizeDelta = BSize;
        // Set the text of the button's child TMP_Text component and return the button game object
        Button.GetComponentInChildren<TMP_Text>().text = BText;
        return Button;
    }   

    // Empty virtual methods for Start and Update
    public virtual void Start() {}
    public virtual void Update() {}
}
