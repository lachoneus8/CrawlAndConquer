using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingPlacementUI : MonoBehaviour
{
    public TMP_Text displayNumText;
    [Tooltip("remember to set the image type to radial,filled.")]
    public Image fillCircle;
    [Tooltip("set to negative value for no upperlimit.")]
    public int MaxCharges = -1;
    [Tooltip("number of seconds between each increase.")]
    public float rechargeTime;

    private float timeTillAvailible;

    [Tooltip("Game object in scene that has a component that implements IGameplayController")]
    public GameObject gameplayController;
    private IGameplayController controller;
    
    public bool isBuildingSelectedForPlacement;
    public GameObject selectedIndicator;
    public List<BuildingPlacementUI> otherBuildingPlacementUIs;

    public BuildingType buildingType;
    public GameObject buildingPrefab;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip spawnbuildingSFX;

    private void Start()
    {
        controller = gameplayController.GetComponent<IGameplayController>();
        timeTillAvailible = rechargeTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.IsGameOver())
        {
            return;
        }
        if (MaxCharges > 0)
        {
            if (controller.GetNumPlaceableBuildings(buildingType) >= MaxCharges)
            {
                //Debug.Log("player at max number of charges for placeable buildings");
            }
            else
            {
                timeTillAvailible -= Time.deltaTime;

                if (timeTillAvailible <= 0)
                {
                    controller.AddPlaceableBuilding(buildingType);
                    timeTillAvailible = rechargeTime;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) && isBuildingSelectedForPlacement && controller.IsSpawnbuildingAvailible(buildingType))
        {

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            controller.TrySpawnbuilding(mousePosition, buildingType, buildingPrefab);
            audioSource.PlayOneShot(spawnbuildingSFX);
        }
        UpdateUI();

        
    }

    private void UpdateUI()
    {
        // Update filling circle based on timeTillAvailible
        fillCircle.fillAmount = 1 - (timeTillAvailible / rechargeTime);
        displayNumText.text = "" + controller.GetNumPlaceableBuildings(buildingType);
    }

    public void SelectBuilding()
    {
        isBuildingSelectedForPlacement = !isBuildingSelectedForPlacement;
        if(!(otherBuildingPlacementUIs is null)) { 
            foreach (BuildingPlacementUI building in otherBuildingPlacementUIs) {
                building.isBuildingSelectedForPlacement = false;
                building.selectedIndicator.SetActive(building.isBuildingSelectedForPlacement);
            }
        }
        selectedIndicator.SetActive(isBuildingSelectedForPlacement);
    }
}

