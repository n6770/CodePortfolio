using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1f;
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
