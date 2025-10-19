using System.Collections.Generic;
using UnityEngine;

public class EnemyBoss : EnemyUnit
{
    public enum EBossState
    {
        Growing,        // Seeking beetles to increase health
        MovingToAttack, // Moving towards Vector3.zero to prepare for attack
        Attacking       // At Vector3.zero, attacking any visible allies
    }

    [Header("Boss Health System")]
    public float maxHealth = 100f;
    public float currentHealth = 20f;
    public float healthThreshold = 60f; // Health needed before attacking
    public float healthMinimum = 40f; // Minimum health to stay in attack phases
    public float healthPerBeetle = 5f; // Health gained from each beetle collision
    public float damagePerHit = 10f; // Damage taken from ally collisions

    [Header("Boss Scaling")]
    public float minScale = 0.5f;
    public float maxScale = 3f;
    public float scaleSpeed = 2f; // How fast the boss scales

    [Header("Boss Behavior")]
    public float attackMoveSpeed = 3f; // Speed when attacking (faster than normal)
    public float searchRadius = 8f; // Distance from origin to search for beetles
    public float searchSpeed = 30f; // Degrees per second when searching
    
    private EBossState currentState = EBossState.Growing;
    private Vector3 targetScale;
    private bool isDead = false;
    private float originalMoveSpeed; // Store original move speed
    private float searchAngle = 0f; // Current angle for circular search pattern

    // Events for health bar and other systems
    public System.Action<float, float> OnHealthChanged; // currentHealth, maxHealth
    public System.Action OnBossDeath;

    protected override void Start()
    {
        base.Start();
        
        // Store original move speed
        originalMoveSpeed = moveSpeed;
        
        // Initialize search angle based on current position
        Vector3 fromOrigin = transform.position - Vector3.zero;
        if (fromOrigin.magnitude > 0.1f)
        {
            searchAngle = Mathf.Atan2(fromOrigin.y, fromOrigin.x) * Mathf.Rad2Deg;
        }
        
        // Initialize health and scale
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateTargetScale();
        transform.localScale = Vector3.one * CalculateScale();
        targetScale = transform.localScale;
        
        // Disable collision destruction for the boss
        enableCollisionDestruction = false;
        
        // Notify health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Update()
    {
        if (isDead) return;

        // Smoothly scale the boss based on health
        UpdateScaling();
        
        // Handle state-based behavior
        switch (currentState)
        {
            case EBossState.Growing:
                HandleGrowingState();
                break;
            case EBossState.MovingToAttack:
                HandleMovingToAttackState();
                break;
            case EBossState.Attacking:
                HandleAttackingState();
                break;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Shouldn't get here until health = 0");
    }

    public override void ApplySenses(List<Entity> sightedEntities)
    {
        base.ApplySenses(sightedEntities);
        
        if (isDead) return;

        // Process behavior based on current state
        if (currentState == EBossState.Attacking)
        {
            ProcessAttackBehavior();
        }
        // In Growing and MovingToAttack states, beetles will come to us or we're focused on reaching origin
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        Entity otherEntity = other.GetComponent<Entity>();
        if (otherEntity == null) return;

        // Handle beetle collisions (gain health)
        if (otherEntity is BeetleEnemy beetle)
        {
            AbsorbBeetle(beetle);
        }
        // Handle ally collisions (take damage)
        else if (otherEntity is AllyUnit ally)
        {
            TakeDamageFromAlly(ally);
        }
    }

    private void AbsorbBeetle(BeetleEnemy beetle)
    {
        // Gain health from beetle
        float healthGain = healthPerBeetle;
        currentHealth = Mathf.Min(currentHealth + healthGain, maxHealth);
        
        // Update scale target
        UpdateTargetScale();
        
        // Destroy the beetle
        GameController gameController = FindFirstObjectByType<GameController>();
        if (gameController != null)
        {
            gameController.OnEntityDestroyed(beetle);
        }
        Destroy(beetle.gameObject);
        
        // Check if we should transition to attacking state
        if (currentState == EBossState.Growing && currentHealth >= healthThreshold)
        {
            TransitionToMovingToAttack();
        }
        
        // Notify health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Boss absorbed beetle! Health: {currentHealth}/{maxHealth}");
    }

    private void TakeDamageFromAlly(AllyUnit ally)
    {
        // Take damage from ally collision
        currentHealth = Mathf.Max(currentHealth - damagePerHit, 0);
        
        // Update scale target
        UpdateTargetScale();
        
        // Destroy the ally (mutual destruction for allies)
        GameController gameController = FindFirstObjectByType<GameController>();
        if (gameController != null)
        {
            gameController.OnEntityDestroyed(ally);
        }
        Destroy(ally.gameObject);
        
        // Check if boss is dead
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // If health drops below threshold while in attack phases, go back to growing
            if ((currentState == EBossState.MovingToAttack || currentState == EBossState.Attacking) 
                && currentHealth < healthMinimum)
            {
                TransitionToGrowing();
            }
        }
        
        // Notify health change
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"Boss took damage from ally! Health: {currentHealth}/{maxHealth}");
    }

    private void HandleGrowingState()
    {
        // Use slower speed for growing state
        moveSpeed = originalMoveSpeed;
        
        if (sightRangeEntities != null && sightRangeEntities.Count > 0)
        {
            BeetleEnemy closestBeetle = GetClosestBeetle(sightRangeEntities);
            if (closestBeetle != null)
            {
                // Found a beetle! Move towards it
                MoveToTarget(closestBeetle.transform.position);
                SetWalk(true);
                return;
            }
        }
        
        // No beetles in sight - search for them by moving in a circle around origin
        SearchForBeetles();
    }

    private void SearchForBeetles()
    {
        // Update search angle to create circular movement
        searchAngle += searchSpeed * Time.deltaTime;
        if (searchAngle >= 360f)
        {
            searchAngle -= 360f;
        }
        
        // Calculate target position on circle around origin
        float angleInRadians = searchAngle * Mathf.Deg2Rad;
        Vector3 targetPosition = Vector3.zero + new Vector3(
            Mathf.Cos(angleInRadians) * searchRadius,
            Mathf.Sin(angleInRadians) * searchRadius,
            0f
        );
        
        // Move towards the target position on the circle
        MoveToTarget(targetPosition);
        SetWalk(true);
    }

    private void HandleMovingToAttackState()
    {
        // Use faster speed for moving to attack position
        moveSpeed = attackMoveSpeed;
        
        // Move towards Vector3.zero (world origin)
        Vector3 targetPosition = Vector3.zero;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        
        if (distanceToTarget > 0.5f)
        {
            // Still moving towards origin
            MoveToTarget(targetPosition);
            SetWalk(true);
        }
        else
        {
            // Reached origin, transition to attacking state
            TransitionToAttacking();
            SetWalk(false);
        }
    }

    private void HandleAttackingState()
    {
        // At Vector3.zero, stay in position and attack visible allies
        // Boss remains stationary at origin unless attacking a specific ally
        SetWalk(false);
        
        // Process attack behavior is handled in ApplySenses -> ProcessAttackBehavior
    }

    private void ProcessAttackBehavior()
    {
        // When in attacking state at origin, move towards visible allies to attack them
        if (sightRangeEntities != null && sightRangeEntities.Count > 0)
        {
            AllyUnit closestAlly = GetClosestAlly(sightRangeEntities);
            if (closestAlly != null)
            {
                // Move towards the closest ally to attack
                MoveToTarget(closestAlly.transform.position);
                SetWalk(true);
            }
        }
        else
        {
            // No allies visible, stay at origin
            SetWalk(false);
        }
    }

    private BeetleEnemy GetClosestBeetle(List<Entity> entities)
    {
        BeetleEnemy closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in entities)
        {
            if (entity == null) continue;

            if (entity is BeetleEnemy beetle)
            {
                float distance = Vector3.Distance(transform.position, beetle.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = beetle;
                }
            }
        }

        return closest;
    }

    private AllyUnit GetClosestAlly(List<Entity> entities)
    {
        AllyUnit closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in entities)
        {
            if (entity is AllyUnit ally)
            {
                float distance = Vector3.Distance(transform.position, ally.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = ally;
                }
            }
        }

        return closest;
    }

    private void TransitionToMovingToAttack()
    {
        currentState = EBossState.MovingToAttack;
        Debug.Log("Boss is now MOVING TO ATTACK! Heading to origin.");
    }

    private void TransitionToAttacking()
    {
        currentState = EBossState.Attacking;
        Debug.Log("Boss has reached origin and is now ATTACKING!");
    }

    private void TransitionToGrowing()
    {
        currentState = EBossState.Growing;
        
        // Reset search angle to current position when transitioning back to growing
        Vector3 fromOrigin = transform.position - Vector3.zero;
        if (fromOrigin.magnitude > 0.1f)
        {
            searchAngle = Mathf.Atan2(fromOrigin.y, fromOrigin.x) * Mathf.Rad2Deg;
        }
        
        Debug.Log("Boss is now GROWING! Seeking beetles for health.");
    }

    private void UpdateTargetScale()
    {
        float healthRatio = currentHealth / maxHealth;
        float targetScaleValue = Mathf.Lerp(minScale, maxScale, healthRatio);
        targetScale = Vector3.one * targetScaleValue;
    }

    private float CalculateScale()
    {
        float healthRatio = currentHealth / maxHealth;
        return Mathf.Lerp(minScale, maxScale, healthRatio);
    }

    private void UpdateScaling()
    {
        // Smoothly scale towards target scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleSpeed * Time.deltaTime);
    }

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        SetWalk(false);
        
        // Notify systems of boss death
        OnBossDeath?.Invoke();
        
        // Notify game controller
        GameController gameController = FindFirstObjectByType<GameController>();
        if (gameController != null)
        {
            gameController.OnEntityDestroyed(this);
        }
        
        Debug.Log("Boss has been defeated!");
        
        // Destroy the boss GameObject
        Destroy(gameObject, 0.5f); // Small delay for any death effects
    }

    // Public methods for external systems
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsInAttackMode()
    {
        return currentState == EBossState.MovingToAttack || currentState == EBossState.Attacking;
    }

    public bool IsAlive()
    {
        return !isDead;
    }

    // Check if a beetle should be attracted to this boss
    public bool ShouldAttractBeetles()
    {
        return currentState == EBossState.Growing && !isDead;
    }

    // For debugging and visualization
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw search circle when in growing state
        if (currentState == EBossState.Growing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Vector3.zero, searchRadius);
            
            // Draw current search target
            float angleInRadians = searchAngle * Mathf.Deg2Rad;
            Vector3 searchTarget = Vector3.zero + new Vector3(
                Mathf.Cos(angleInRadians) * searchRadius,
                Mathf.Sin(angleInRadians) * searchRadius,
                0f
            );
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, searchTarget);
            Gizmos.DrawWireSphere(searchTarget, 0.3f);
        }

        // Draw target (origin when moving to attack or attacking)
        if (currentState == EBossState.MovingToAttack || currentState == EBossState.Attacking)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, Vector3.zero);
            
            // Different color for different attack states
            if (currentState == EBossState.MovingToAttack)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(Vector3.zero, 1f);
            }
            else
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one * 2f);
            }
        }

        // Health visualization
        Gizmos.color = Color.green;
        Vector3 healthBarPos = transform.position + Vector3.up * 2f;
        float healthRatio = currentHealth / maxHealth;
        Gizmos.DrawLine(healthBarPos, healthBarPos + Vector3.right * healthRatio * 3f);
    }
}