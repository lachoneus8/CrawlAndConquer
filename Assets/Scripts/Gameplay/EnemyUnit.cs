using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Entity
{
    public float sightRange = 12f;
    public float moveSpeed = 1.8f;

    private Animator animator;
    protected List<Entity> sightRangeEntities;

    public virtual void ApplySenses(List<Entity> sightedEntities)
    {
        sightRangeEntities = sightedEntities;
    }

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        sightRangeEntities = new List<Entity>();
    }

    // Base movement function that can be called from derived classes
    protected void MoveToTarget(Vector3 targetPosition)
    {
        var dir = (targetPosition - transform.position).normalized;

        // Rotate to face movement direction (top-down view, rotate around Z-axis)
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    // Check if we've reached a target position
    protected bool HasReachedTarget(Vector3 targetPosition, float threshold = 0.1f)
    {
        return Vector3.Distance(transform.position, targetPosition) < threshold;
    }

    protected void SetWalk(bool isWalking)
    {
        if (animator != null)
        {
            animator.SetBool("Walk", isWalking);
        }
    }
}