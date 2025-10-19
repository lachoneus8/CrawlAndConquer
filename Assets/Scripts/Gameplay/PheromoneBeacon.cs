using UnityEngine;

public class PheromoneBeacon : Entity
{
    protected virtual void Start()
    {
        
        // Disable collision destruction for pheromone beacons
        enableCollisionDestruction = false;
    }
}