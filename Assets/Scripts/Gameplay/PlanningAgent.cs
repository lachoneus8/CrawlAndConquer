using System.Collections.Generic;
using UnityEngine;

public class PlanningAgent : AllyUnit
{
    public float communicationRange = 7f;
    private BattlefieldIntel myIntel;
    private List<BattlefieldIntel> sharedIntel;
    
    // Enhanced ApplySenses for planning agents
    public void ApplySenses(List<Entity> smelledEntities, List<Entity> sightedEntities, List<PlanningAgent> nearbyPlanners)
    {
        base.ApplySenses(smelledEntities, sightedEntities);
        
        // Update my intel based on what I see
        UpdateMyIntel(sightedEntities);
        
        // Share intel with nearby planning agents
        ShareIntelWithNearbyPlanners(nearbyPlanners);
        
        // Decide behavior based on combined intel
        DetermineBehaviorFromIntel();
    }
    
    private void UpdateMyIntel(List<Entity> sightedEntities)
    {
        if (sightedEntities.Count > 0)
        {
            Vector3 avgEnemyPos = Vector3.zero;
            bool bossSpotted = false;
            Vector3 bossPos = Vector3.zero;
            
            foreach (var entity in sightedEntities)
            {
                avgEnemyPos += entity.transform.position;
                if (entity is EnemyBoss)
                {
                    bossSpotted = true;
                    bossPos = entity.transform.position;
                }
            }
            
            avgEnemyPos /= sightedEntities.Count;
            myIntel = new BattlefieldIntel(avgEnemyPos, sightedEntities.Count, bossSpotted, bossPos);
        }
    }
    
    private void ShareIntelWithNearbyPlanners(List<PlanningAgent> nearbyPlanners)
    {
        foreach (var planner in nearbyPlanners)
        {
            if (planner != this && myIntel != null)
            {
                planner.ReceiveIntel(myIntel);
            }
        }
    }
    
    public void ReceiveIntel(BattlefieldIntel intel)
    {
        if (sharedIntel == null) sharedIntel = new List<BattlefieldIntel>();
        
        // Only keep recent intel (last 10 seconds)
        sharedIntel.RemoveAll(i => Time.time - i.timeStamp > 10f);
        
        // Add new intel
        sharedIntel.Add(intel);
    }
    
    private void DetermineBehaviorFromIntel()
    {
        float totalThreat = 0f;
        int intelCount = 0;
        
        // Consider my own intel
        if (myIntel != null)
        {
            totalThreat += myIntel.threatLevel;
            intelCount++;
        }
        
        // Consider shared intel
        if (sharedIntel != null)
        {
            foreach (var intel in sharedIntel)
            {
                totalThreat += intel.threatLevel;
                intelCount++;
            }
        }
        
        float avgThreat = intelCount > 0 ? totalThreat / intelCount : 0f;
        
        // Behavior decisions based on collective intelligence
        if (avgThreat > 0.7f)
        {
            // High threat: Most planners coordinate attack
            // TODO: Implement attack coordination behavior
        }
        else if (avgThreat > 0.3f)
        {
            // Medium threat: Some attack, some continue reconnaissance  
            // TODO: Implement mixed behavior
        }
        else
        {
            // Low threat: Continue exploration
            // TODO: Implement exploration behavior
        }
    }
}