using System.Collections.Generic;
using UnityEngine;

public class BeetleEnemy : EnemyUnit
{
    public enum EBeetleState
    {
        Idle,
        Wander,
        MoveTo,
        Attack,
        SeekBoss    // New state for seeking the boss
    }

    public float idleTimeAvg = 3f;
    public float idleTimeVar = 1.5f;
    public float wanderDistance = 4f;
    public float maxDistanceFromOrigin = 8f;

    private Vector3 originPosition;
    private Vector3 curMoveTarget;
    private float curStateTime;
    private EBeetleState curState = EBeetleState.Idle;

    protected override void Start()
    {
        base.Start();
        originPosition = Vector3.zero; 
        curStateTime = Random.Range(0, idleTimeAvg + idleTimeVar);
    }

    private void Update()
    {
        curStateTime -= Time.deltaTime;

        switch (curState)
        {
            case EBeetleState.Idle:
                HandleIdleState();
                break;
            case EBeetleState.Wander:
                HandleWanderState();
                break;
            case EBeetleState.MoveTo:
                HandleMoveToState();
                break;
            case EBeetleState.Attack:
                HandleAttackState();
                break;
            case EBeetleState.SeekBoss:
                HandleSeekBossState();
                break;
        }
    }

    public override void ApplySenses(List<Entity> sightedEntities)
    {
        base.ApplySenses(sightedEntities);
        
        // Aggressive behavior: React to visible entities
        ProcessBehavior();
    }

    private void ProcessBehavior()
    {
        // Priority 1: Seek boss if it's visible and in growing mode
        EnemyBoss boss = GetVisibleBoss(sightRangeEntities);
        if (boss != null && boss.ShouldAttractBeetles())
        {
            SetMoveTarget(boss.transform.position);
            curState = EBeetleState.SeekBoss;
            SetWalk(true);
            return;
        }

        // Priority 2: Attack visible allies
        if (sightRangeEntities != null && sightRangeEntities.Count > 0)
        {
            Entity closestAlly = GetClosestAlly(sightRangeEntities);
            if (closestAlly != null)
            {
                SetMoveTarget(closestAlly.transform.position);
                curState = EBeetleState.MoveTo;
                SetWalk(true);
                return;
            }
        }

        // Priority 3: Continue current behavior (wander/idle) if not seeking boss or allies
        if (curState == EBeetleState.SeekBoss)
        {
            // Boss is no longer visible or attracting, return to wandering
            curState = EBeetleState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
            SetWalk(false);
        }
    }

    private EnemyBoss GetVisibleBoss(List<Entity> entities)
    {
        if (entities == null) return null;

        foreach (Entity entity in entities)
        {
            if (entity is EnemyBoss boss)
            {
                return boss;
            }
        }
        return null;
    }

    private Entity GetClosestAlly(List<Entity> entities)
    {
        Entity closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in entities)
        {
            if (entity == null) continue;
            
            // Only target allies (AllyUnit and its derivatives)
            if (entity is AllyUnit)
            {
                float distance = Vector3.Distance(transform.position, entity.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }

    private void HandleIdleState()
    {
        if (curStateTime <= 0)
        {
            curState = EBeetleState.Wander;
            GenerateWanderTarget();
            SetWalk(true);
        }
    }

    private void HandleWanderState()
    {
        if (HasReachedTarget(curMoveTarget))
        {
            curState = EBeetleState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
            SetWalk(false);
        }
        else
        {
            MoveToTarget(curMoveTarget);
        }
    }

    private void HandleMoveToState()
    {
        if (HasReachedTarget(curMoveTarget))
        {
            // Check if we're close enough to attack
            if (sightRangeEntities != null && sightRangeEntities.Count > 0)
            {
                Entity closestAlly = GetClosestAlly(sightRangeEntities);
                if (closestAlly != null && Vector3.Distance(transform.position, closestAlly.transform.position) < 1f)
                {
                    curState = EBeetleState.Attack;
                    curStateTime = 1f; // Attack duration
                    SetWalk(false);
                    return;
                }
            }

            // No valid target, return to wandering
            curState = EBeetleState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
            SetWalk(false);
        }
        else
        {
            MoveToTarget(curMoveTarget);
        }
    }

    private void HandleSeekBossState()
    {
        // Move towards the boss
        if (HasReachedTarget(curMoveTarget, 0.5f)) // Larger threshold for boss
        {
            // We've reached the boss, collision should handle the rest
            SetWalk(false);
        }
        else
        {
            MoveToTarget(curMoveTarget);
        }
    }

    private void HandleAttackState()
    {
        // TODO: Implement actual attack behavior (damage, animation, etc.)
        // For now, just attack for a brief duration then return to wandering
        if (curStateTime <= 0)
        {
            curState = EBeetleState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
        }
    }

    private void GenerateWanderTarget()
    {
        Vector3 randomTarget;
        int attempts = 0;
        const int maxAttempts = 10;

        do
        {
            // Generate random point within wander distance
            Vector2 randomPoint = Random.insideUnitCircle * wanderDistance;
            randomTarget = transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);
            attempts++;
        }
        while (Vector3.Distance(originPosition, randomTarget) > maxDistanceFromOrigin && attempts < maxAttempts);

        // If we couldn't find a valid target within max attempts, move towards origin
        if (attempts >= maxAttempts)
        {
            Vector3 directionToOrigin = (originPosition - transform.position).normalized;
            randomTarget = transform.position + directionToOrigin * (wanderDistance * 0.5f);
        }

        curMoveTarget = randomTarget;
    }

    public void SetMoveTarget(Vector3 target)
    {
        curMoveTarget = target;
    }

    // Optional: Visualize the beetle's origin and max wander distance in editor
    private void OnDrawGizmosSelected()
    {
        // Draw origin position
        Gizmos.color = Color.red;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(originPosition, 0.5f);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        // Draw max wander distance
        Gizmos.color = Color.yellow;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(originPosition, maxDistanceFromOrigin);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, maxDistanceFromOrigin);
        }

        // Draw sight range (used for boss detection)
        Gizmos.color = Color.cyan;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(transform.position, sightRange);
        }

        // Draw current target
        if (Application.isPlaying && (curState == EBeetleState.Wander || curState == EBeetleState.SeekBoss))
        {
            Gizmos.color = curState == EBeetleState.SeekBoss ? Color.red : Color.green;
            Gizmos.DrawWireSphere(curMoveTarget, 0.3f);
            Gizmos.DrawLine(transform.position, curMoveTarget);
        }
    }
}