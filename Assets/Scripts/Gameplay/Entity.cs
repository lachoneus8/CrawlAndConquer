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
            if (gameController != null)
            {
                gameController.OnEntityDestroyed(this);
                gameController.OnEntityDestroyed(otherEntity);
            }

            // Destroy both entities
            Destroy(this.gameObject);
            Destroy(otherEntity.gameObject);
        }
    }
}