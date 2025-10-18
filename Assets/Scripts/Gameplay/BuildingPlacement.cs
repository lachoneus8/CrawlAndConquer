using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{

    public GameObject buildingPrefab;
    public List<GameObject> buildings;

    [Tooltip("Game object in scene that has a component that implements IGameplayController")]
    public GameObject gameplayController;
    private IGameplayController controller;

    [Header("SFX")]
    public AudioSource audioSource;
    public AudioClip spawnbuildingSFX;
    void Start()
    {

    }

    public void ClearBuildings()
    {
        foreach (GameObject building in buildings)
        {
            Destroy(building);
        }
        buildings.Clear();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && controller.IsSpawnbuildingAvailible())
        {
            //audioSource.PlayOneShot(spawnbuildingSFX);
        }
    }
}
