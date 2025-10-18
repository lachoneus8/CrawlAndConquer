using System.Collections.Generic;
using UnityEngine;

public class AllyUnit : Entity
{
    public float smellRange = 5f;
    public float sightRange = 2f;
    public float moveSpeed = 2f;

    protected List<Entity> smellRangeEntities;
    protected List<Entity> sightRangeEntities;

    public virtual void ApplySenses(List<Entity> smelledEntities, List<Entity> sightedEntities)
    {
        smellRangeEntities = smelledEntities;
        sightRangeEntities = sightedEntities;
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
}
