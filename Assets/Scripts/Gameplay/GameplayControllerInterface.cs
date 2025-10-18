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

public interface IGameplayController
{
    public Points GetPoints();
    public bool IsGameOver();
    public int GetNumPlaceableBuildings();
    public void AddPlaceableBuilding();
}
