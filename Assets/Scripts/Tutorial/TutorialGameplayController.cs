using UnityEngine;
using System.Collections.Generic;

public class TutorialGameplayController : MonoBehaviour, IGameplayController
{

    public Camera mainCamera;
    public TutorialMenuController tutorialMenuController;
    [Tooltip("tutorial steps start index at 1")]
    public int currentTutorialStep = 1;
    [Header("step 1 : Camera Controls")]
    GameObject step1EnemiesPrefab;
    GameObject step1AlliesPrefab;
    [Header("step 2 : How units move")]
    [Header("step 3 : Building Functions")]
    [Header("step 4 : Placing Buildings")]

    [Header("step 5 : Winning the game")]
    [Tooltip("He needs no introduction!!")]
    public GameObject LordScorpion;
    [Tooltip("setup victory conditions")]
    public Points points;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    List<GameObject> spawnedEntities;

    public Dictionary<BuildingType, int> buildingsAvailible = new Dictionary<BuildingType, int> { { BuildingType.Beacon, 1 }, { BuildingType.WorkerSpawner, 1 }, };
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
        SetupTutorialStep(currentTutorialStep);
    }

    public void GoNextTutorialStep() {
        currentTutorialStep+=1;
        tutorialMenuController.SetTutorialScene(currentTutorialStep - 1);//adjust for array index
        SetupTutorialStep(currentTutorialStep);
    }

    public void SetupTutorialStep(int step)
    {
        CameraMovement cameraMovement = mainCamera.GetComponent<CameraMovement>();
        LordScorpion.SetActive(false);
        cameraMovement.panSpeed = 0;
        switch (step){
            case 1://setup step 1 : Camera Controls
                //spawnedEntities.Add(Instantiate(step1AlliesPrefab));
                //spawnedEntities.Add(Instantiate(step1EnemiesPrefab));
                mainCamera.transform.position = new Vector3(0, 0, -10);
                cameraMovement.panSpeed = 20;
                return;
            case 2://setup step 2 : How units move
                mainCamera.transform.position = new Vector3(-20, 20, -10);
                return;
            case 3://setup step 3 : Building Functions
                return;
            case 4://setup step 4 : Placing Buildings
                return;
            case 5://setup step 5 : Winning the game
                cameraMovement.panSpeed = 0;
                mainCamera.transform.position = new Vector3(0, 0, -10);
                LordScorpion.SetActive(true);
                return;
            default:
                Debug.LogWarning("moved to an invalid tutorial step");
                tutorialMenuController.LoadMainGameScene();
                return;
        }
    }

    public bool IsGameOver()
    {
        return false;
    }

    public int GetNumPlaceableBuildings(BuildingType type)
    {
        if (buildingsAvailible.TryGetValue(type, out int result))
        {
            return result;
        }
        return 0;
    }

    public void AddPlaceableBuilding(BuildingType type)
    {
        if (buildingsAvailible.ContainsKey(type))
        {
            buildingsAvailible[type]+=1;
        }
    }

    public bool IsSpawnbuildingAvailible(BuildingType type)
    {
        return true;
    }

    public bool TrySpawnbuilding(Vector3 SpawnPosition, BuildingType type, GameObject prefab)
    {
        if (!buildingsAvailible.ContainsKey(type))
        {
            Debug.LogWarning("tried to spawn building of type "+type+" but that type didnt exist in dictionary.");
            return false;
        }
        buildingsAvailible[type] -= 1;
        GameObject building = Instantiate(prefab);//todo set parent
        building.transform.position = SpawnPosition;
        //add this to relevant lists of game object entities
        return true;
    }
}
