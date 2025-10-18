using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialMenuController : MonoBehaviour
{
    [Tooltip("menu for each tutorial step")]
    public List<GameObject> tutorialMenus;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetTutorialScene(int index) {
        if (index > tutorialMenus.Count) {
            Debug.LogWarning("tutorial scene index out of bounds in TutorialMenuController, check tutorialMenus list.");
            return;
        }
        foreach (GameObject menu in tutorialMenus) {
            menu.SetActive(false);
        }
        tutorialMenus[index].SetActive(true);
    }

    public void LoadMainGameScene()
    {
        SceneManager.LoadSceneAsync(2);
    }

}
