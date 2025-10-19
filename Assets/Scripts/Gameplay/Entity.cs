using UnityEngine;

public class Entity : MonoBehaviour
{
    [Header("Collision Detection")]
    public bool enableCollisionDestruction = true;

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!enableCollisionDestruction) return;

        // Check for mutual destruction between allies and enemies
        HandleCollisionDestruction(other);
    }

    private void HandleCollisionDestruction(Collider2D other)
    {
        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) return;

        bool shouldDestroy = false;

        // Ally vs Enemy collision
        if (this is AllyUnit && otherEntity is EnemyUnit)
        {
            shouldDestroy = true;
        }
        // Enemy vs Ally collision
        else if (this is EnemyUnit && otherEntity is AllyUnit)
        {
            shouldDestroy = true;
        }

        if (shouldDestroy)
        {
            // Notify GameController before destruction
            GameController gameController = FindFirstObjectByType<GameController>();
            
            // Destroy any entity if they aren't the boss
            if (this is not EnemyBoss)
            {
                Destroy(this.gameObject);
                gameController.OnEntityDestroyed(this);
            }

            if (otherEntity is not EnemyBoss)
            {
                Destroy(otherEntity.gameObject);
                gameController.OnEntityDestroyed(otherEntity);
            }
        }
    }
}