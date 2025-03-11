using UnityEngine;
using TMPro; // Required for TextMeshPro components

public class TextInputHandler : MonoBehaviour
{
    float time = MysteryShip.FinalTime; // Access the static variable
    [SerializeField] private TMP_InputField inputField; // Reference to the TMP_InputField component

    private void Awake()
    {
        if (inputField == null)
        {
            inputField = GetComponent<TMP_InputField>(); // Auto-assign if not set in Inspector
            if (inputField == null)
            {
                Debug.LogError("No TMP_InputField assigned or found on this GameObject!");
            }
        }
    }

    private void Start()
    {
        // Optional: Set default text or placeholder
        inputField.text = ""; // Clear the input field on start
        inputField.onEndEdit.AddListener(OnInputSubmitted); // Subscribe to the submit event
    }

    private void OnInputSubmitted(string input)
    {
        if (!string.IsNullOrEmpty(input)) // Check if input is not empty
        {
            Debug.Log("User name: " + input +" with a time of" + time);
            // Add your custom logic here, e.g., save the input, trigger an action, etc.
            ProcessInput(input);
        }
    }

    private void ProcessInput(string input)
    {
        // Example: Do something with the input
        // For your game, this could interact with Invaders or MysteryShip
        Debug.Log("Processing input: " + input);
    }

    private void OnDestroy()
    {
        // Clean up the listener to avoid memory leaks
        if (inputField != null)
        {
            inputField.onEndEdit.RemoveListener(OnInputSubmitted);
        }
    }

    // Optional: Public method to get the current input text
    public string GetInputText()
    {
        return inputField != null ? inputField.text : "";
    }

    // Optional: Public method to set the input text programmatically
    public void SetInputText(string text)
    {
        if (inputField != null)
        {
            inputField.text = text;
        }
    }
}