using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AllyUnit : Entity
{
    public enum EAllyState
    {
        Idle,
        Patrol,
        MoveTo,
        Attack
    }

    public float smellRange;
    public float sightRange;

    public float idleTimeAvg;
    public float idleTimeVar;

    public float patrolDistance;
    public float moveSpeed;

    private Animator animator;

    private List<Entity> smellRangeEntities;
    private List<Entity> sightRangeEntities;

    public Vector3 curPatrolCenter;
    public Vector3 curMoveTarget;

    private float curStateTime;

    private EAllyState curState = EAllyState.Idle;

    public void ApplySenses(List<Entity> smelledEntities, List<Entity> sightedEntities)
    {
        smellRangeEntities = smelledEntities;
        sightRangeEntities = sightedEntities;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        curPatrolCenter = transform.position;

        curStateTime = Random.Range(0, idleTimeAvg + idleTimeVar);
    }

    private void Update()
    {
        curStateTime -= Time.deltaTime;

        switch (curState)
        {
            case EAllyState.Idle:
                HandleIdleState();
                break;
            case EAllyState.Patrol:
                HandlePatrolState();
                break;
            case EAllyState.MoveTo:
                
                break;
            case EAllyState.Attack:
                
                break;
        }
    }

    private void HandlePatrolState()
    {
        var dist = (transform.position - curMoveTarget).magnitude;
        if (dist < .1f)
        {
            curState = EAllyState.Idle;
            curStateTime = Random.Range(idleTimeAvg - idleTimeVar, idleTimeAvg + idleTimeVar);
            SetWalk(false);
        }
        else
        {
            var dir = (curMoveTarget - transform.position).normalized;
            
            // Rotate to face movement direction (top-down view, rotate around Z-axis)
            if (dir != Vector3.zero)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
            
            transform.position += dir * moveSpeed * Time.deltaTime;
        }
    }

    private void HandleIdleState()
    {
        if (curStateTime <= 0)
        {
            curState = EAllyState.Patrol;
            Vector2 randomPoint = Random.insideUnitCircle * patrolDistance;
            curMoveTarget = curPatrolCenter + new Vector3(randomPoint.x, randomPoint.y, 0);
            SetWalk(true);
        }
    }

    private void SetWalk(bool isWalking)
    {
        animator.SetBool("Walk", isWalking);
    }
}
