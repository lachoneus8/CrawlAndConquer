using UnityEngine;

public class TutorialGameplayController : MonoBehaviour, IGameplayController
{
    TutorialMenuController tutorialMenuController;
    [Tooltip("tutorial steps start index at 1")]
    public int currentTutorialStep = 1;
    [Header("step 1 : Camera Controls")]
    [Header("step 2 : How units move")]
    [Header("step 3 : Building Functions")]
    [Header("step 4 : Placing Buildings")]

    [Header("step 5 : winning the game")]
    [Tooltip("setup victory conditions")]
    public Points points;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (points.currentPoints == points.victoryPoints)
        {
            Debug.LogWarning("you start the game with enough points to win, check currentPoints and victoryPoints.");
            return;
        }
        points.currentPoints = points.startingPoints;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public Points GetPoints()
    {
        return points;
    }

    public void nextTutorialStep() {
        switch (currentTutorialStep) {
            case 0:
                return;
            default:
                Debug.LogWarning("moved to an invalid tutorial step");
                tutorialMenuController.LoadMainGameScene();
                return;
        }
        
    }
}
