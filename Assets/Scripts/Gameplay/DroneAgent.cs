using System.Collections.Generic;
using UnityEngine;

public class DroneAgent : AllyUnit
{
    public enum EDroneState
    {
        Idle,
        Patrol,
        MoveTo,
        Attack
    }

    public float idleTimeAvg = 2f;
    public float idleTimeVar = 1f;
    public float patrolDistance = 3f;

    private Vector3 curPatrolCenter;
    private Vector3 curMoveTarget;
    private float curStateTime;
    private EDroneState curState = EDroneState.Idle;

    protected override void Start()
    {
        curPatrolCenter = transform.position;
        curStateTime = Random.Range(0, idleTimeAvg + idleTimeVar);

        base.Start();
    }

    private void Update()
    {
        curStateTime -= Time.deltaTime;

        switch (curState)
        {
            case EDroneState.Idle:
                HandleIdleState();
                break;
            case EDroneState.Patrol:
                HandlePatrolState();
                break;
            case EDroneState.MoveTo:
                HandleMoveToState();
                break;
            case EDroneState.Attack:
                HandleAttackState();
                break;
        }
    }

    public override void ApplySenses(List<Entity> smelledEntities, List<Entity> sightedEntities)
    {
        base.ApplySenses(smelledEntities, sightedEntities);
        
        // Reflex behavior: React to immediate stimuli
        ProcessReflexBehavior();
    }

    private void ProcessReflexBehavior()
    {
        // Priority 1: Attack visible enemies
        if (sightRangeEntities != null && sightRangeEntities.Count > 0)
        {
            Entity closestEnemy = GetClosestEnemy(sightRangeEntities);
            if (closestEnemy != null)
            {
                SetMoveTarget(closestEnemy.transform.position);
                curState = EDroneState.MoveTo;
                SetWalk(true);
                return;
            }
        }

        // Priority 2: Follow pheromone trails (smelled entities)
        if (smellRangeEntities != null && smellRangeEntities.Count > 0)
        {
            PheromoneBeacon closestPheromone = GetClosestPheromone(smellRangeEntities);
            if (closestPheromone != null)
            {
                SetMoveTarget(closestPheromone.transform.position);
                curState = EDroneState.MoveTo;
                SetWalk(true);
                return;
            }
        }

        // Priority 3: Continue current behavior (patrol/idle)
    }

    private PheromoneBeacon GetClosestPheromone(List<Entity> entities)
    {
        PheromoneBeacon closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in entities)
        {
            if (entity is PheromoneBeacon pheromone)
            {
                float distance = Vector3.Distance(transform.position, pheromone.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = pheromone;
                }
            }
        }

        return closest;
    }

    private Entity GetClosestEnemy(List<Entity> enemies)
    {
        Entity closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = enemy;
            }
        }

        return closest;
    }

    private void HandleIdleState()
    {
        if (curStateTime <= 0)
        {
            curState = EDroneState.Patrol;
            Vector2 randomPoint = Random.insideUnitCircle * patrolDistance;
            curMoveTarget = curPatrolCenter + new Vector3(randomPoint.x, randomPoint.y, 0);
            SetWalk(true);
        }
    }

    private void HandlePatrolState()
    {
        if (HasReachedTarget(curMoveTarget))
        {
            curState = EDroneState.Idle;
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
                Entity closestEnemy = GetClosestEnemy(sightRangeEntities);
                if (closestEnemy != null && Vector3.Distance(transform.position, closestEnemy.transform.position) < 1f)
                {
                    curState = EDroneState.Attack;
                    SetWalk(false);
                    return;
                }
            }

            // No valid target, return to patrol
            curState = EDroneState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
            SetWalk(false);
        }
        else
        {
            MoveToTarget(curMoveTarget);
        }
    }

    private void HandleAttackState()
    {
        // TODO: Implement attack behavior
        // For now, just return to patrol after a brief delay
        if (curStateTime <= 0)
        {
            curState = EDroneState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
        }
    }

    public void SetMoveTarget(Vector3 target)
    {
        curMoveTarget = target;
    }
}