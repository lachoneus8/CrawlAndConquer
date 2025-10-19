using System;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : Entity
{
    public UnityEvent<Transform, GameObject> OnSpawned = new UnityEvent<Transform, GameObject>();

    public float spawnInterval = 5f;
    public GameObject prefabToSpawn;

    private float nextSpawnTime = 0f;

    private void Start()
    {
        nextSpawnTime = spawnInterval;
    }

    private void Update()
    {
        nextSpawnTime -= Time.deltaTime;
        if (nextSpawnTime <= 0f)
        {
            SpawnEntity();
            nextSpawnTime = spawnInterval;
        }
    }

    private void SpawnEntity()
    {
        OnSpawned.Invoke(transform, prefabToSpawn);
    }
}
