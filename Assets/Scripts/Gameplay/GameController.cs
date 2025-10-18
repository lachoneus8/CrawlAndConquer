using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour, IGameplayController
{
    public List<AllyUnit> allyUnits;
    public List<EnemyUnit> enemyUnits;
    public EnemyBoss enemyBoss;

    public float sensorTickTime;

    private float timeToNextSensorTick;
    private List<Entity> allVisibleEntities;
    private List<Entity> emptySmelledList;

    private void Start()
    {
        timeToNextSensorTick = sensorTickTime;
        
        // Pre-allocate lists for performance
        allVisibleEntities = new List<Entity>();
        emptySmelledList = new List<Entity>();
        
        // Build the list of all visible entities once
        RefreshVisibleEntities();
    }

    private void Update()
    {
        timeToNextSensorTick -= Time.deltaTime;
        if (timeToNextSensorTick < 0)
        {
            SensorTick();
            timeToNextSensorTick = sensorTickTime;
        }    
    }

    private void RefreshVisibleEntities()
    {
        allVisibleEntities.Clear();
        
        // Add all enemy units
        foreach (var enemy in enemyUnits)
        {
            if (enemy != null)
                allVisibleEntities.Add(enemy);
        }
        
        // Add boss if it exists
        if (enemyBoss != null)
            allVisibleEntities.Add(enemyBoss);
    }

    private void SensorTick()
    {
        // Refresh visible entities in case any were destroyed
        RefreshVisibleEntities();
        
        foreach (var ally in allyUnits)
        {
            if (ally == null) continue;
            
            List<Entity> sightedEntities = GetEntitiesInRange(ally, ally.sightRange, allVisibleEntities);
            
            // Apply senses - empty smelled list since enemies can't be smelled
            ally.ApplySenses(emptySmelledList, sightedEntities);
        }
    }

    private List<Entity> GetEntitiesInRange(AllyUnit ally, float range, List<Entity> entitiesToCheck)
    {
        List<Entity> entitiesInRange = new List<Entity>();
        float rangeSqr = range * range; // Use squared distance for performance
        
        foreach (var entity in entitiesToCheck)
        {
            if (entity == null) continue;
            
            float distanceSqr = (ally.transform.position - entity.transform.position).sqrMagnitude;
            if (distanceSqr <= rangeSqr)
            {
                entitiesInRange.Add(entity);
            }
        }
        
        return entitiesInRange;
    }

    public Points GetPoints()
    {
        throw new NotImplementedException();
    }
}
