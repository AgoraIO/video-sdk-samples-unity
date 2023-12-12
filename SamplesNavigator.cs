using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class SamplesNavigator : MonoBehaviour
{
    #region Variables

    private TMP_Dropdown sampleDropdown;
    private TMP_Dropdown productDropdown;
    private MonoBehaviour previousScriptComponent;
    private Dictionary<string, Type> scriptDictionary = new Dictionary<string, Type>();
    private string previousOption = "";
    internal ConfigData configData;
    internal string path;

    #endregion

    #region Initialization

    void Start()
    {
        ReadConfigData();
        InitializeDropdowns();
        AttachEventListeners();
    }

    void InitializeDropdowns()
    {
        // Get references to dropdown components
        sampleDropdown = GameObject.Find("sampleDropdown").GetComponent<TMP_Dropdown>();
        productDropdown = GameObject.Find("productDropdown").GetComponent<TMP_Dropdown>();

        // Populate script dictionary and dropdown options
        PopulateScriptDictionary();
        PopulateDropdowns();
    }

    void PopulateScriptDictionary()
    {
        // Add script names and types to the dictionary
        scriptDictionary.Add("SDK QuickStart", typeof(GetStarted));
        scriptDictionary.Add("Call Quality Best Practice", typeof(EnsureCallQuality));
        scriptDictionary.Add("Authentication Workflow", typeof(AuthenticationWorkflow));
        scriptDictionary.Add("Secure Channel Encryption", typeof(MediaStreamEncryption));
        scriptDictionary.Add("Cloud Proxy", typeof(CloudProxy));
        scriptDictionary.Add("Screen share, volume control and mute", typeof(ProductWorkflow));
        scriptDictionary.Add("Stream Media to a Channel", typeof(PlayMedia));
        scriptDictionary.Add("Audio and Voice Effects", typeof(AudioVoiceEffects));
        scriptDictionary.Add("Geofencing", typeof(Geofencing));
        scriptDictionary.Add("Raw audio and video", typeof(RawAudioVideo));
        scriptDictionary.Add("Custom audio and video", typeof(CustomAudioAndVideo));
        scriptDictionary.Add("Spatial Audio", typeof(SpatialAudio));
        scriptDictionary.Add("Virtual Background", typeof(VirtualBackground));
        scriptDictionary.Add("Multi-channel live streaming", typeof(MultiChannelLiveStreaming));
        // (Note: The above dictionary should be completed with all your available scripts.)
    }

    void PopulateDropdowns()
    {
        if (configData.appID == "")
        { 
            Debug.Log("Please provide an App ID to run the sample game");
            return;
        }
        // Define dropdown options
        List<string> scriptNames = new List<string>(scriptDictionary.Keys);
        List<string> productNames = new List<string> { "Video Calling", "ILS" };

        // Assign options to the dropdowns
        sampleDropdown.AddOptions(scriptNames);
        productDropdown.AddOptions(productNames);
    }

    void AttachEventListeners()
    {
        // Attach event listeners
        sampleDropdown.onValueChanged.AddListener(OnSampleDropdownValueChanged);
        productDropdown.onValueChanged.AddListener(OnProductDropdownValueChanged);
    }

    #endregion

    #region Event Handlers

    void OnProductDropdownValueChanged(int index)
    {
        // Handle product dropdown value change
        UpdateConfigData(index);

        // Destroy the previous script component
        DestroyPreviousScript();

        // Handle script selection based on the new product
        HandleScriptSelection();
    }

    void OnSampleDropdownValueChanged(int index)
    {
        // Handle sample dropdown value change
        DestroyPreviousScript();
        CreateOrGetScript(index);
    }

    #endregion

    #region Custom Methods
    void UpdateConfigData(int index)
    {
        // Update the 'product' attribute
        configData.product = productDropdown.options[index].text;

        // Convert the updated configData back to JSON
        string updatedJson = JsonUtility.ToJson(configData);

        // Write the updated JSON back to the file
        File.WriteAllText(path, updatedJson);
    }

    void ReadConfigData()
    {
        path = Path.Combine(Application.dataPath, "agora-manager", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }

    void HandleScriptSelection()
    {
        string selectedProduct = productDropdown.options[productDropdown.value].text;
        // Check if a script is selected (e.g., not "Select")
        if (selectedProduct != "Select")
        {
            CreateOrGetScript(sampleDropdown.value);
        }
    }

    void CreateOrGetScript(int index)
    {
        string selectedOption = sampleDropdown.options[index].text;

        // Check if there was a previous option
        if (!string.IsNullOrEmpty(previousOption))
        {
            if (scriptDictionary.TryGetValue(previousOption, out Type previousScriptType) && previousOption != selectedOption)
            {
                DestroyPreviousScript();
            }
        }

        // Store the current option as the previous option
        previousOption = selectedOption;

        if (configData.product == "Video Calling" && selectedOption == "Multi-channel live streaming")
        {
            Debug.Log("This feature is only available in interactive live streaming");
            return;
        }

        // Get the corresponding script type from the dictionary
        if (scriptDictionary.TryGetValue(selectedOption, out Type scriptType))
        {
            // Create or get an instance of the selected script type
            MonoBehaviour scriptInstance = gameObject.GetComponent(scriptType) as MonoBehaviour;
            scriptInstance = gameObject.AddComponent(scriptType) as MonoBehaviour;

            // Store the script component as the previous script component
            previousScriptComponent = scriptInstance;
        }
    }

    void DestroyPreviousScript()
    {
        if (previousScriptComponent != null)
        {
            Destroy(previousScriptComponent);
            Debug.Log("Destroyed previous script: " + previousScriptComponent.GetType());
            previousScriptComponent = null;
            previousOption = "";
        }
    }

    #endregion
}
