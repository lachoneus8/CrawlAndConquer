using System;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour, IGameplayController
{
    public List<DroneAgent> droneAgents;
    public List<PlanningAgent> planningAgents;
    public EnemyBoss enemyBoss;
    public List<BeetleEnemy> beetleEnemies;

    public float sensorTickTime;

    private float timeToNextSensorTick;
    private List<Entity> allVisibleEntities;
    private List<Entity> emptySmelledList;

    private List<AllyUnit> allyUnits;
    private List<EnemyUnit> enemyUnits;

    // Destruction tracking
    private List<Entity> entitiesToRemove = new List<Entity>();

    private void Start()
    {
        timeToNextSensorTick = sensorTickTime;
        
        // Pre-allocate lists for performance
        allVisibleEntities = new List<Entity>();
        emptySmelledList = new List<Entity>();

        allyUnits = new List<AllyUnit>();
        allyUnits.AddRange(droneAgents);
        allyUnits.AddRange(planningAgents);

        enemyUnits = new List<EnemyUnit>(); 
        enemyUnits.AddRange(beetleEnemies);
        if (enemyBoss != null)
            enemyUnits.Add(enemyBoss);

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
        
        // Process entity removal at end of frame
        ProcessEntityRemoval();
    }

    public void OnEntityDestroyed(Entity entity)
    {
        // Queue entity for removal to avoid modifying lists during iteration
        if (!entitiesToRemove.Contains(entity))
        {
            entitiesToRemove.Add(entity);
        }
    }

    private void ProcessEntityRemoval()
    {
        if (entitiesToRemove.Count == 0) return;

        foreach (Entity entity in entitiesToRemove)
        {
            RemoveEntityFromLists(entity);
        }

        entitiesToRemove.Clear();
    }

    private void RemoveEntityFromLists(Entity entity)
    {
        if (entity == null) return;

        // Remove from ally lists
        if (entity is DroneAgent drone)
        {
            droneAgents.Remove(drone);
            allyUnits.Remove(drone);
        }
        else if (entity is PlanningAgent planner)
        {
            planningAgents.Remove(planner);
            allyUnits.Remove(planner);
        }
        else if (entity is AllyUnit ally)
        {
            allyUnits.Remove(ally);
        }

        // Remove from enemy lists
        if (entity is BeetleEnemy beetle)
        {
            beetleEnemies.Remove(beetle);
            enemyUnits.Remove(beetle);
        }
        else if (entity is EnemyBoss boss)
        {
            if (enemyBoss == boss)
            {
                enemyBoss = null;
            }
            enemyUnits.Remove(boss);
        }
        else if (entity is EnemyUnit enemy)
        {
            enemyUnits.Remove(enemy);
        }

        // Remove from visible entities list
        allVisibleEntities.Remove(entity);
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
        
        // Handle drone agents (reflex agents)
        foreach (var drone in droneAgents.ToArray()) // Use ToArray to avoid modification during iteration
        {
            if (drone == null) continue;
            
            List<Entity> sightedEntities = GetEntitiesInRange(drone, drone.sightRange, allVisibleEntities);
            drone.ApplySenses(emptySmelledList, sightedEntities);
        }
        
        // Handle planning agents with enhanced communication
        foreach (var planner in planningAgents.ToArray())
        {
            if (planner == null) continue;
            
            List<Entity> sightedEntities = GetEntitiesInRange(planner, planner.sightRange, allVisibleEntities);
            List<PlanningAgent> nearbyPlanners = GetNearbyPlanners(planner);
            
            planner.ApplySenses(emptySmelledList, sightedEntities, nearbyPlanners);
        }

        // Handle any remaining ally units that aren't drones or planners
        foreach (var ally in allyUnits.ToArray())
        {
            if (ally == null || ally is DroneAgent || ally is PlanningAgent) continue;
            
            List<Entity> sightedEntities = GetEntitiesInRange(ally, ally.sightRange, allVisibleEntities);
            ally.ApplySenses(emptySmelledList, sightedEntities);
        }

        // Handle beetle enemies
        foreach (var beetle in beetleEnemies.ToArray())
        {
            if (beetle == null) continue;
            
            List<Entity> sightedAllies = GetAlliesInRange(beetle, beetle.sightRange);
            beetle.ApplySenses(sightedAllies);
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

    private List<Entity> GetAlliesInRange(BeetleEnemy beetle, float range)
    {
        List<Entity> alliesInRange = new List<Entity>();
        float rangeSqr = range * range; // Use squared distance for performance
        
        foreach (var ally in allyUnits)
        {
            if (ally == null) continue;
            
            float distanceSqr = (beetle.transform.position - ally.transform.position).sqrMagnitude;
            if (distanceSqr <= rangeSqr)
            {
                alliesInRange.Add(ally);
            }
        }
        
        return alliesInRange;
    }

    public Points GetPoints()
    {
        throw new NotImplementedException();
    }

    private List<PlanningAgent> GetNearbyPlanners(PlanningAgent planner)
    {
        List<PlanningAgent> nearbyPlanners = new List<PlanningAgent>();
        float commRangeSqr = planner.communicationRange * planner.communicationRange;
        
        foreach (var otherPlanner in planningAgents)
        {
            if (otherPlanner == null || otherPlanner == planner) continue;
            
            float distanceSqr = (planner.transform.position - otherPlanner.transform.position).sqrMagnitude;
            if (distanceSqr <= commRangeSqr)
            {
                nearbyPlanners.Add(otherPlanner);
            }
        }
        
        return nearbyPlanners;
    }

    // Optional: Methods to check game state
    public bool AreAllAlliesDead()
    {
        return droneAgents.Count == 0 && planningAgents.Count == 0;
    }

    public bool AreAllEnemiesDead()
    {
        return beetleEnemies.Count == 0 && enemyBoss == null;
    }

    public int GetAllyCount()
    {
        return droneAgents.Count + planningAgents.Count;
    }

    public int GetEnemyCount()
    {
        int count = beetleEnemies.Count;
        if (enemyBoss != null) count++;
        return count;
    }

    public bool IsGameOver()
    {
        throw new NotImplementedException();
    }

    public int GetNumPlaceableBuildings()
    {
        throw new NotImplementedException();
    }

    public void AddPlaceableBuilding()
    {
        throw new NotImplementedException();
    }

    public bool IsSpawnbuildingAvailible()
    {
        throw new NotImplementedException();
        /*
        if (NumPlaceableBuildings <= 0)
        {
            NumPlaceableBuildings = 0;
            return false;
        }
        NumPlaceableBuildings--;
        return true;*/
    }

    public bool TrySpawnbuilding(Vector3 SpawnPosition)
    {
        throw new NotImplementedException();
    }
}
