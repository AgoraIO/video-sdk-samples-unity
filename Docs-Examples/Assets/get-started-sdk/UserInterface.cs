
// Import Unity and Agora libraries
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class UserInterface
{
    // Declare variables for UI elements
    internal Canvas canvas;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal GameObject joinBtn;
    internal GameObject leaveBtn;
    // Constructor that initializes the canvas variable
    public UserInterface(Canvas obj)
    {
        canvas = obj;
    }
    
    // Method that sets up the UI by calling other methods to add UI elements
    public void SetupUI()
    {
        AddJoinLeaveBtns();
        AddLocalView();
        AddRemoteView();
    }
    
    // Method that adds the remote video view to the UI
    public void AddRemoteView()
    {
        // Create a new raw image object
        GameObject go = new GameObject("LocalView", typeof(RawImage));

        // Get the RectTransform component and set its parent, rotation, position, scale, and sizeDelta properties
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(canvas.transform); // Set the parent to the canvas
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f); // Set the rotation to face the camera
        rectTransform.localPosition = new Vector3(250, 0, 0); // Set the position
        rectTransform.localScale = Vector3.one; // Set the scale to (1, 1, 1)
        rectTransform.sizeDelta = new Vector2(250, 250); // Set the width and height of the RawImage
        
        // Add VideoSurface component to the game object
        RemoteView = go.AddComponent<VideoSurface>();
    }

    // Method that adds the local video view to the UI
    public void AddLocalView()
    {
        // Create a new raw image object
        GameObject go = new GameObject("LocalView", typeof(RawImage));

        // Get the RectTransform component and set its parent, rotation, position, scale, and sizeDelta properties
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(canvas.transform); // Set the parent to the canvas
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f); // Set the rotation to face the camera
        rectTransform.localPosition = new Vector3(-250, 0, 0); // Set the position
        rectTransform.localScale = Vector3.one; // Set the scale to (1, 1, 1)
        rectTransform.sizeDelta = new Vector2(250, 250); // Set the width and height of the RawImage
        
        // Add VideoSurface component to the game object
        LocalView = go.AddComponent<VideoSurface>();
    }
    
    // Method that adds the join and leave buttons to the UI
    public void AddJoinLeaveBtns()
    {
        // Create resources object for creating buttons
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        
        // Create Join button
        joinBtn = TMP_DefaultControls.CreateButton(resources);
        joinBtn.name = "Join";
        joinBtn.transform.SetParent(canvas.transform, false);
        joinBtn.transform.localPosition = new Vector3(-350, -172, 0);
        joinBtn.transform.localScale = Vector3.one;
        joinBtn.GetComponentInChildren<TMP_Text>().text = "Join";
        
        // Create Leave button
        leaveBtn = TMP_DefaultControls.CreateButton(resources);
        leaveBtn.name = "Leave";
        leaveBtn.transform.SetParent(canvas.transform, false);
        leaveBtn.transform.localPosition = new Vector3(350, -172, 0);
        leaveBtn.transform.localScale = Vector3.one;
        leaveBtn.GetComponentInChildren<TMP_Text>().text = "Leave";
    }   
}
