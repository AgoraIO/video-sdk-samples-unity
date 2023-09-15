using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.IO;

public class SamplesNavigator : MonoBehaviour
{
    private TMP_Dropdown sampleDropdown;
    private TMP_Dropdown productDropdown;
    private MonoBehaviour previousScriptComponent;
    private Dictionary<string, Type> scriptDictionary = new Dictionary<string, Type>();
    private string previousOption = "";
    internal ConfigData configData;

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the sample dropdown component
        GameObject go = GameObject.Find("sampleDropdown");
        sampleDropdown = go.GetComponent<TMP_Dropdown>();
        
        // Get a reference to the sample dropdown component
        go = GameObject.Find("productDropdown");
        productDropdown = go.GetComponent<TMP_Dropdown>();

        // Create a list of script names
        List<string> scriptNames = new List<string>();
        List<string> productNames = new List<string> { "Video Calling", "ILS" };

        // Add script names and types to the dictionary
        scriptDictionary.Add("Select", null);
        scriptDictionary.Add("SDK QuickStart", typeof(GetStarted));
        scriptDictionary.Add("Call Quality Best Practice", typeof(EnsureCallQuality));
        scriptDictionary.Add("Authentication Workflow", typeof(AuthenticationWorkflow));
        scriptDictionary.Add("Secure Channel Encryption", typeof(MediaStreamEncryption));
        scriptDictionary.Add("Cloud Proxy", typeof(CloudProxy));
        // Get the script names from the dictionary
        scriptNames.AddRange(scriptDictionary.Keys);

        // Assign the script names to the dropdown
        sampleDropdown.AddOptions(scriptNames);
        productDropdown.AddOptions(productNames);

        // Attach the onValueChanged event listener
        sampleDropdown.onValueChanged.AddListener(OnSampleDropdownValueChanged);
        productDropdown.onValueChanged.AddListener(onProductDropdownValueChanged);

    }
    void onProductDropdownValueChanged(int index)
    {
        string path = System.IO.Path.Combine(Application.dataPath, "agora-manager", "config.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            configData = JsonUtility.FromJson<ConfigData>(json);
            
            // Update the 'product' attribute
            configData.product = productDropdown.options[index].text;
            
            // Convert the updated configData back to JSON
            string updatedJson = JsonUtility.ToJson(configData);
            
            // Write the updated JSON back to the file
            File.WriteAllText(path, updatedJson);
        }
        else
        {
            Debug.LogError("Config file not found!");
        }
    }

    void OnSampleDropdownValueChanged(int index)
    {
        // Get the selected option text
        string selectedOption = sampleDropdown.options[index].text;

        // Check if there was a previous option
        if (!string.IsNullOrEmpty(previousOption))
        {
            // Get the corresponding script type of the previous option
            if (scriptDictionary.TryGetValue(previousOption, out Type previousScriptType))
            {
                // Destroy the previous script component if it exists
                if (previousScriptComponent != null)
                {
                    Destroy(previousScriptComponent);
                    Debug.Log("Destroyed previous script: " + previousScriptComponent.GetType());
                }
            }
        }

        // Store the current option as the previous option
        previousOption = selectedOption;

        // Get the corresponding script type from the dictionary
        if (scriptDictionary.TryGetValue(selectedOption, out Type scriptType))
        {
            // Create an instance of the selected script type
            MonoBehaviour scriptInstance = gameObject.GetComponent(scriptType) as MonoBehaviour;

            if (scriptInstance == null)
            {
                // If the script component does not exist, add it
                scriptInstance = gameObject.AddComponent(scriptType) as MonoBehaviour;
            }

            // Store the script component as the previous script component
            previousScriptComponent = scriptInstance;
        }
    }
}
