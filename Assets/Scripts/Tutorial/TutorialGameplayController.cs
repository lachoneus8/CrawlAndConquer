using UnityEngine;

public class TutorialGameplayController : MonoBehaviour, IGameplayController
{
    public TutorialMenuController tutorialMenuController;
    [Tooltip("tutorial steps start index at 1")]
    public int currentTutorialStep = 1;
    [Header("step 1 : Camera Controls")]
    [Header("step 2 : How units move")]
    [Header("step 3 : Building Functions")]
    [Header("step 4 : Placing Buildings")]

    [Header("step 5 : Winning the game")]
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
    public void GoBackTutorialStep()
    {
        currentTutorialStep -= 1;
        tutorialMenuController.SetTutorialScene(currentTutorialStep - 1);//adjust for array index
    }

    public void GoNextTutorialStep() {
        currentTutorialStep+=1;
        tutorialMenuController.SetTutorialScene(currentTutorialStep - 1);//adjust for array index
    }

    public void SetupTutorialStep() {
        switch (currentTutorialStep){
            case 1://setup step 1 : Camera Controls
                return;
            case 2://setup step 2 : How units move
                return;
            case 3://setup step 3 : Building Functions
                return;
            case 4://setup step 4 : Placing Buildings
                return;
            case 5://setup step 5 : Winning the game
                return;
            default:
                Debug.LogWarning("moved to an invalid tutorial step");
                tutorialMenuController.LoadMainGameScene();
                return;
        }
    }
}
