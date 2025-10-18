using UnityEngine;

[System.Serializable]
public struct Points
{
    [Tooltip("how many points you start with")]
    public uint startingPoints;
    [Tooltip("how many points needed for victory")]
    public uint victoryPoints;
    [Tooltip("current points, set automatically")]
    public uint currentPoints;
}
[System.Serializable]
public enum BuildingType
{
    None,
    Beacon,
    WorkerSpawner,
}

public interface IGameplayController
{
    public Points GetPoints();
    public bool IsGameOver();
    public int GetNumPlaceableBuildings(BuildingType type);
    public void AddPlaceableBuilding(BuildingType type);
    public bool IsSpawnbuildingAvailible(BuildingType type);
    public bool TrySpawnbuilding(Vector3 SpawnPosition, BuildingType type, GameObject prefab);
}
