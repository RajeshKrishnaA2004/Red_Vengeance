using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Scene Management")]
    public string targetSceneName = "level 1";
    
    /// <summary>
    /// Loads the specified scene
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is null or empty!");
        }
    }
    
    /// <summary>
    /// Loads the default target scene
    /// </summary>
    public void LoadTargetScene()
    {
        LoadScene(targetSceneName);
    }

    /// <summary>
    /// Quits the game application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Game is exiting...");
        Application.Quit();
    }
}
