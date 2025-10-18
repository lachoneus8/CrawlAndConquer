using System.Collections.Generic;
using UnityEngine;

public class PlanningAgent : AllyUnit
{
    public enum EPlannerState
    {
        Exploration,
        Reconnaissance, 
        AttackCoordination,
        MovingToTarget
    }

    public float communicationRange = 7f;
    public float explorationDistance = 8f;
    public float coordinationRange = 15f; // Range for coordinating attacks
    
    private BattlefieldIntel myIntel;
    private List<BattlefieldIntel> sharedIntel;
    private EPlannerState currentState = EPlannerState.Exploration;
    private Vector3 explorationTarget;
    private Vector3 attackTarget;
    private float stateTimer;
    private float lastBehaviorDecision;
    private Vector3 originPosition;
    
    protected override void Start()
    {
        base.Start();
        originPosition = transform.position;
        GenerateExplorationTarget();
        stateTimer = 0f;
        lastBehaviorDecision = Time.time;
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;
        
        // Execute current state behavior
        switch (currentState)
        {
            case EPlannerState.Exploration:
                HandleExplorationState();
                break;
            case EPlannerState.Reconnaissance:
                HandleReconnaissanceState();
                break;
            case EPlannerState.AttackCoordination:
                HandleAttackCoordinationState();
                break;
            case EPlannerState.MovingToTarget:
                HandleMovingToTargetState();
                break;
        }
    }
    
    // Enhanced ApplySenses for planning agents
    public void ApplySenses(List<Entity> smelledEntities, List<Entity> sightedEntities, List<PlanningAgent> nearbyPlanners)
    {
        base.ApplySenses(smelledEntities, sightedEntities);
        
        // Update my intel based on what I see
        UpdateMyIntel(sightedEntities);
        
        // Share intel with nearby planning agents
        ShareIntelWithNearbyPlanners(nearbyPlanners);
        
        // Decide behavior based on combined intel (but not too frequently)
        if (Time.time - lastBehaviorDecision > 1f) // Reevaluate every second
        {
            DetermineBehaviorFromIntel();
            lastBehaviorDecision = Time.time;
        }
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
        Vector3 priorityTarget = Vector3.zero;
        bool hasValidTarget = false;
        
        // Consider my own intel
        if (myIntel != null)
        {
            totalThreat += myIntel.threatLevel;
            intelCount++;
            
            if (myIntel.hasBossBeenSeen)
            {
                priorityTarget = myIntel.lastKnownBossPosition;
                hasValidTarget = true;
            }
            else
            {
                priorityTarget = myIntel.lastKnownEnemyPosition;
                hasValidTarget = true;
            }
        }
        
        // Consider shared intel
        if (sharedIntel != null)
        {
            foreach (var intel in sharedIntel)
            {
                totalThreat += intel.threatLevel;
                intelCount++;
                
                // Prioritize boss locations from shared intel
                if (intel.hasBossBeenSeen && !hasValidTarget)
                {
                    priorityTarget = intel.lastKnownBossPosition;
                    hasValidTarget = true;
                }
            }
        }
        
        float avgThreat = intelCount > 0 ? totalThreat / intelCount : 0f;
        
        // Behavior decisions based on collective intelligence
        if (avgThreat > 0.7f)
        {
            // High threat: Most planners coordinate attack
            InitiateAttackCoordination(priorityTarget, hasValidTarget);
        }
        else if (avgThreat > 0.3f)
        {
            // Medium threat: Some attack, some continue reconnaissance
            InitiateMixedBehavior(priorityTarget, hasValidTarget);
        }
        else
        {
            // Low threat: Continue exploration
            InitiateExplorationBehavior();
        }
    }

    private void InitiateAttackCoordination(Vector3 target, bool hasValidTarget)
    {
        if (hasValidTarget)
        {
            attackTarget = target;
            currentState = EPlannerState.AttackCoordination;
            stateTimer = 2f; // Coordinate for 2 seconds before moving
            SetWalk(false);
            
            // TODO: Place attack pheromones here when pheromone system is implemented
            // PlaceAttackPheromones(attackTarget);
        }
        else
        {
            // No target to attack, switch to reconnaissance
            currentState = EPlannerState.Reconnaissance;
            GenerateReconnaissanceTarget();
        }
    }

    private void InitiateMixedBehavior(Vector3 target, bool hasValidTarget)
    {
        // Use agent's position to determine role in mixed behavior
        // Agents closer to enemies become attackers, others continue reconnaissance
        float myHash = Mathf.Abs(transform.position.GetHashCode()) % 100f;
        
        if (myHash < 60f && hasValidTarget) // 60% become attackers
        {
            attackTarget = target;
            currentState = EPlannerState.AttackCoordination;
            stateTimer = 1.5f;
            SetWalk(false);
        }
        else
        {
            // Continue reconnaissance
            currentState = EPlannerState.Reconnaissance;
            GenerateReconnaissanceTarget();
        }
    }

    private void InitiateExplorationBehavior()
    {
        if (currentState != EPlannerState.Exploration)
        {
            currentState = EPlannerState.Exploration;
            GenerateExplorationTarget();
        }
    }

    private void HandleExplorationState()
    {
        if (HasReachedTarget(explorationTarget))
        {
            GenerateExplorationTarget();
            stateTimer = Random.Range(1f, 3f); // Pause briefly at exploration points
            SetWalk(false);
        }
        else if (stateTimer <= 0)
        {
            MoveToTarget(explorationTarget);
            SetWalk(true);
        }
    }

    private void HandleReconnaissanceState()
    {
        // More focused movement toward areas where enemies were last seen
        if (HasReachedTarget(explorationTarget))
        {
            GenerateReconnaissanceTarget();
            stateTimer = Random.Range(0.5f, 1.5f); // Shorter pauses during reconnaissance
            SetWalk(false);
        }
        else if (stateTimer <= 0)
        {
            MoveToTarget(explorationTarget);
            SetWalk(true);
        }
    }

    private void HandleAttackCoordinationState()
    {
        if (stateTimer <= 0)
        {
            // Coordination time is over, move toward the attack target
            currentState = EPlannerState.MovingToTarget;
            SetWalk(true);
        }
        // During coordination, stay in place and "plan"
    }

    private void HandleMovingToTargetState()
    {
        if (HasReachedTarget(attackTarget, 2f)) // Get reasonably close to attack target
        {
            // Switch back to reconnaissance once we've reached the general area
            currentState = EPlannerState.Reconnaissance;
            GenerateReconnaissanceTarget();
        }
        else
        {
            MoveToTarget(attackTarget);
            SetWalk(true);
        }
    }

    private void GenerateExplorationTarget()
    {
        // Generate random exploration points around the battlefield
        Vector2 randomPoint = Random.insideUnitCircle * explorationDistance;
        explorationTarget = originPosition + new Vector3(randomPoint.x, randomPoint.y, 0);
    }

    private void GenerateReconnaissanceTarget()
    {
        Vector3 baseTarget = originPosition;
        
        // If we have intel, bias reconnaissance toward last known enemy positions
        if (myIntel != null)
        {
            baseTarget = Vector3.Lerp(originPosition, myIntel.lastKnownEnemyPosition, 0.7f);
        }
        else if (sharedIntel != null && sharedIntel.Count > 0)
        {
            // Use most recent shared intel
            var recentIntel = sharedIntel[sharedIntel.Count - 1];
            baseTarget = Vector3.Lerp(originPosition, recentIntel.lastKnownEnemyPosition, 0.5f);
        }
        
        // Add some randomness to the reconnaissance target
        Vector2 randomOffset = Random.insideUnitCircle * (explorationDistance * 0.6f);
        explorationTarget = baseTarget + new Vector3(randomOffset.x, randomOffset.y, 0);
    }

    // Visualization for debugging
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw communication range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, communicationRange);

        // Draw current target
        Gizmos.color = Color.green;
        if (currentState == EPlannerState.MovingToTarget)
        {
            Gizmos.DrawWireSphere(attackTarget, 0.5f);
            Gizmos.DrawLine(transform.position, attackTarget);
        }
        else
        {
            Gizmos.DrawWireSphere(explorationTarget, 0.3f);
            Gizmos.DrawLine(transform.position, explorationTarget);
        }

        // Draw state indicator
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.2f);
    }

    private Color GetStateColor()
    {
        switch (currentState)
        {
            case EPlannerState.Exploration: return Color.green;
            case EPlannerState.Reconnaissance: return Color.yellow;
            case EPlannerState.AttackCoordination: return Color.red;
            case EPlannerState.MovingToTarget: return Color.magenta;
            default: return Color.white;
        }
    }
}