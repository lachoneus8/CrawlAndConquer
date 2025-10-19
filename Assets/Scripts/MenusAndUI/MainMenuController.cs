using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{


    [Header("SFX")]
    public AudioSource backgroundAmbianceAudioSource;
    public AudioClip MenuThemeSFX;
    void Start()
    {
        backgroundAmbianceAudioSource.clip = MenuThemeSFX;
        backgroundAmbianceAudioSource.loop = true;
        backgroundAmbianceAudioSource.Play();
    }

    void Update()
    {

    }

    public void LoadStartGameScene()
    {
        backgroundAmbianceAudioSource.Stop();
        SceneManager.LoadSceneAsync(1);
    }
    public void LoadMainGameScene()
    {
        backgroundAmbianceAudioSource.Stop();
        SceneManager.LoadSceneAsync(2);
    }
    public void QuitGame()
    {
        backgroundAmbianceAudioSource.Stop();
        // If running in the Unity editor, stop play mode
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
