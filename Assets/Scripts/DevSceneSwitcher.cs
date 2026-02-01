using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Yeni Input System kütüphanesi

public class NextSceneDev : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = (currentSceneIndex + 1) % SceneManager.sceneCountInBuildSettings;
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}