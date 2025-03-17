using UnityEngine;
using UnityEngine.UI; // Required for Button component

[RequireComponent(typeof(Button))] // Ensures the GameObject has a Button component
public class ReplayButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        // Get the Button component on this GameObject
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("No Button component found on this GameObject!");
            return;
        }

        // Add the OnClick listener
                    Debug.Log("Replay button setup.");

        button.onClick.AddListener(OnButtonClick);
    }

    void OnDestroy()
    {
        // Clean up the listener to avoid memory leaks
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        // Call StartReplay from ReplayManager
        if (ReplayManager.Instance != null)
        {
            ReplayManager.Instance.StartReplay();
            Debug.Log("Replay button clicked - starting replay.");
        }
        else
        {
            Debug.LogError("ReplayManager.Instance is null! Ensure ReplayManager is in the scene.");
        }
    }
}