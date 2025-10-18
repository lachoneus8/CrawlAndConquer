using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialMenuController : MonoBehaviour, IGameplayController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void LoadMainGameScene()
    {
        SceneManager.LoadSceneAsync(2);
    }

    public Points GetPoints()
    {
        throw new System.NotImplementedException();
    }
}
