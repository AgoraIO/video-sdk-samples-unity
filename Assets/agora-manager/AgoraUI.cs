using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using TMPro;

public class AgoraUI : MonoBehaviour
{
    // References to UI elements
    internal Canvas canvas;
    internal GameObject joinBtn;
    internal GameObject leaveBtn;
    internal GameObject LocalView;
    internal GameObject RemoteView;

    // Create a video view
    public virtual GameObject MakeLocalView(string VName, Vector3 VPos, Vector2 VSize)
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
        return go;
    }
    public virtual GameObject MakeRemoteView(string VName, Vector2 VSize)
    {
        GameObject scrollView = GameObject.Find("RemoteViews");
        Transform contentTransform = scrollView.transform.Find("Viewport/Content");

        // Create the game object with a RawImage component
        GameObject go = new GameObject(VName, typeof(RawImage));
        // Set the RectTransform settings
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(scrollView.transform.Find("Viewport/Content")); // Access the Content GameObject within the Scroll View
        return go;
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

    public virtual void OnDestroy()
    {
        if(joinBtn)
           Destroy(joinBtn.gameObject);
        if(leaveBtn)
           Destroy(leaveBtn.gameObject);
        if(LocalView)
           Destroy(LocalView.gameObject);
        if(RemoteView)
           Destroy(RemoteView.gameObject);
    }
}
