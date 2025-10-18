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

    private void Start()
    {
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
            if (controller.GetNumPlaceableBuildings() >= MaxCharges)
            {
                Debug.Log("player at max number of charges for placeable buildings");
                UpdateUI();
                return;
            }
        }

        timeTillAvailible -= Time.deltaTime;

        if (timeTillAvailible <= 0)
        {
            controller.AddPlaceableBuilding();
            timeTillAvailible = rechargeTime;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update filling circle based on timeTillAvailible
        fillCircle.fillAmount = 1 - (timeTillAvailible / rechargeTime);
        displayNumText.text = "" + controller.GetNumPlaceableBuildings();
    }
}

