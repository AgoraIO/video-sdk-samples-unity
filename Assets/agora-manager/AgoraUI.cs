using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System;
using static System.Net.Mime.MediaTypeNames;

public class AgoraUI : MonoBehaviour
{
    // References to UI elements
    internal Canvas canvas;
    internal GameObject joinBtnGo;
    internal GameObject leaveBtnGo;
    internal GameObject LocalViewGo;
    internal static GameObject[] RemoteViews;
    public TMP_FontAsset tmpFont;
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
    public virtual GameObject AddSlider(string SName, Vector3 TPos)
    {
        DefaultControls.Resources uiResources = new DefaultControls.Resources();

        // Use Unity's default sprites for the slider components
        uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        uiResources.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        uiResources.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        // Create the slider GameObject
        GameObject sliderGo = DefaultControls.CreateSlider(uiResources);
        sliderGo.name = SName;
        sliderGo.transform.localPosition = TPos;
        sliderGo.transform.SetParent(canvas.transform, false);
        return sliderGo;
    }
    public virtual GameObject MakeRemoteView(string VName)
    {
        GameObject userView = new GameObject();
        userView.name = VName;
        userView.AddComponent<RawImage>();

        userView.transform.SetParent(GameObject.Find("Content").transform); // Set parent to the Content of ScrollView

        // Set up transform
        userView.transform.Rotate(0f, 0.0f, 180.0f);
        userView.transform.localPosition = Vector3.zero;
        userView.transform.localScale = new Vector3(2f, 3f, 1f);

        if (RemoteViews == null)
        {
            RemoteViews = new GameObject[1]; // Initialize the array with a single element
            RemoteViews[0] = userView;
            Debug.Log("Adding the first remote user");
        }
        else
        {
            int length = RemoteViews.Length;
            GameObject[] temp = new GameObject[length];
            Array.Copy(RemoteViews, temp, length); // Copy the existing elements to temp

            RemoteViews = new GameObject[length + 1]; // Increase the size of the array
            Array.Copy(temp, RemoteViews, length); // Copy back the elements from temp
            RemoteViews[length] = userView; // Add the new element at the end
        }
        return userView;
    }
    public virtual GameObject AddToggle(string TName, Vector3 TPos, string text, Vector2 TSize)
    {
        DefaultControls.Resources uiResources = new DefaultControls.Resources();

        // Set the Toggle Background and Checkmark images to Unity's built-in sprites
        uiResources.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        uiResources.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        // Create the Toggle GameObject
        GameObject uiToggle = DefaultControls.CreateToggle(uiResources);
        uiToggle.name = TName;
        UnityEngine.UI.Text labelText = uiToggle.GetComponentInChildren<UnityEngine.UI.Text>();
        labelText.text = text;
        uiToggle.transform.localPosition = TPos;
        uiToggle.transform.SetParent(canvas.transform, false);
        return uiToggle;
    }

    public GameObject AddInputField(string fieldName, Vector3 position, string placeholderText)
    {
        // Create input field.
        TMP_DefaultControls.Resources resources = new TMP_DefaultControls.Resources();
        GameObject inputFieldGo = TMP_DefaultControls.CreateInputField(resources);
        inputFieldGo.name = fieldName;
        inputFieldGo.transform.SetParent(canvas.transform, false);

        TMP_InputField tmpInputField = inputFieldGo.GetComponent<TMP_InputField>();
        RectTransform inputFieldTransform = tmpInputField.GetComponent<RectTransform>();
        inputFieldTransform.sizeDelta = new Vector2(200, 30);

        TMP_Text textComponent = inputFieldGo.GetComponentInChildren<TMP_Text>();
        textComponent.alignment = TextAlignmentOptions.Center;
        tmpInputField.placeholder.GetComponent<TMP_Text>().text = placeholderText;

        return inputFieldGo;
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

    public virtual GameObject AddDropdown(string Dname, Vector3 DPos, Vector2 DSize)
    {
        GameObject dropDownGo = TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources());
        dropDownGo.name = Dname;
        dropDownGo.transform.SetParent(canvas.transform);
        dropDownGo.transform.localPosition = DPos;
        dropDownGo.transform.localScale = Vector3.one;
        RectTransform rectTransform = dropDownGo.GetComponent<RectTransform>();
        rectTransform.sizeDelta = DSize;
        return dropDownGo;
    }
    public virtual GameObject AddLabel(string LName, Vector3 LPos, string LText)
    {
        GameObject labelGo = new GameObject(LName);
        TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(canvas.transform);
        label.transform.localPosition = LPos;
        label.text = LText;
        label.fontSize = 6;
        return labelGo;
    }
    // Empty virtual methods for Start and Update
    public virtual void Start() { }
    public virtual void Update() { }

    public virtual void OnDestroy()
    {
        if (joinBtnGo)
            Destroy(joinBtnGo.gameObject);
        if (leaveBtnGo)
            Destroy(leaveBtnGo.gameObject);
        if (LocalViewGo)
            Destroy(LocalViewGo.gameObject);
        if (RemoteViews != null)
        {
            foreach (var go in RemoteViews)
            {
                Destroy(go.gameObject);
            }
        }
    }
}