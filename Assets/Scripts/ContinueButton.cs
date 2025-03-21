using UnityEngine;
using UnityEngine.UI; 
using UnityEngine.SceneManagement; 


[RequireComponent(typeof(Button))] 
public class ContinueButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("No Button component found on this GameObject!");
            return;
        }

                    Debug.Log("Replay button setup.");

        button.onClick.AddListener(OnButtonClick);
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
        {
            
            SceneManager.LoadScene("scoreScene");
            UnityEngine.Debug.Log("Scene load command issued for 'scoreScene'.");

        }
}