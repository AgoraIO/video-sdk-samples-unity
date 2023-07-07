using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class SamplesNavigator : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private MonoBehaviour previousScriptComponent;
    private Dictionary<string, Type> scriptDictionary = new Dictionary<string, Type>();
    private string previousOption = "";

    // Start is called before the first frame update
    void Start()
    {
        // Get a reference to the Dropdown component
        GameObject go = GameObject.Find("Dropdown");
        dropdown = go.GetComponent<TMP_Dropdown>();

        // Clear any existing options
        dropdown.ClearOptions();

        // Create a list of script names
        List<string> scriptNames = new List<string>();

        // Add script names and types to the dictionary
        scriptDictionary.Add("Select", null);
        scriptDictionary.Add("Get Started", typeof(GetStarted));
        scriptDictionary.Add("Ensure Call Quality", typeof(EnsureCallQuality));

        // Get the script names from the dictionary
        scriptNames.AddRange(scriptDictionary.Keys);

        // Assign the script names to the dropdown
        dropdown.AddOptions(scriptNames);

        // Attach the onValueChanged event listener
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void OnDropdownValueChanged(int index)
    {
        // Get the selected option text
        string selectedOption = dropdown.options[index].text;

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
            MonoBehaviour scriptInstance = gameObject.AddComponent(scriptType) as MonoBehaviour;

            // Store the script component as the previous script component
            previousScriptComponent = scriptInstance;

            // Execute the script's functionality
            scriptInstance.Invoke("Execute", 0f);
        }
    }
}
