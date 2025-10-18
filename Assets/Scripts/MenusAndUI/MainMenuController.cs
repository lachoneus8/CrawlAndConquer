using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {

    }

    public void LoadStartGameScene()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void LoadMainGameScene()
    {
        SceneManager.LoadSceneAsync(2);
    }
    public void QuitGame()
    {
        // If running in the Unity editor, stop play mode
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
