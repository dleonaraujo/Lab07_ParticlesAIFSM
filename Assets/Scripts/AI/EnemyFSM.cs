using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase, Attack, Flee }
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Referencias")]
    public Transform player;
    public ParticleSystem chaseTrailPS;
    public ParticleSystem attackImpactPS;
    public ParticleSystem fleePS;

    [Header("Parámetros de detección")]
    public float detectionRange = 6f;
    public float attackRange = 1.2f;
    public float fleeHealthThreshold = 0.25f;

    [Header("Parámetros de movimiento")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3f;
    public float fleeSpeed = 4f;
    public float patrolDistance = 3f;

    [Header("Parámetros de ataque")]
    public float attackCooldown = 1.5f;
    public float attackDamage = 10f;

    [Header("Vida")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Pathfinding")]
    public AStarPathfinder pathfinder;
    private List<Vector3> currentPath;
    private int pathIndex;
    public float pathUpdateInterval = 0.3f;
    private float lastPathUpdate;

    private Vector3 patrolOrigin;
    private int patrolDirection = 1;
    private float lastAttackTime;
    private EnemyState previousState;

    void Start()
    {
        currentHealth = maxHealth;
        patrolOrigin = transform.position;
        previousState = EnemyState.Patrol;
        StopAllParticles();
    }

    void Update()
    {
        EvaluateTransitions();
        if (currentState != previousState)
            OnStateChanged(previousState, currentState);
        ExecuteState();
        previousState = currentState;
    }

    void EvaluateTransitions()
    {
        float healthRatio = currentHealth / maxHealth;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (healthRatio <= fleeHealthThreshold)
        {
            currentState = EnemyState.Flee;
            return;
        }
        if (distToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
            return;
        }
        if (distToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
            return;
        }
        currentState = EnemyState.Patrol;
    }

    void OnStateChanged(EnemyState from, EnemyState to)
    {
        StopAllParticles();

        switch (to)
        {
            case EnemyState.Chase:
                if (chaseTrailPS != null) chaseTrailPS.Play();
                break;
            case EnemyState.Attack:
                if (attackImpactPS != null) attackImpactPS.Play();
                break;
            case EnemyState.Flee:
                if (fleePS != null) fleePS.Play();
                break;
        }
    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:  DoPatrol();  break;
            case EnemyState.Chase:   DoChase();   break;
            case EnemyState.Attack:  DoAttack();  break;
            case EnemyState.Flee:    DoFlee();    break;
        }
    }

    void DoPatrol()
    {
        float targetX = patrolOrigin.x + patrolDirection * patrolDistance;
        Vector3 target = new Vector3(targetX, transform.position.y, 0);
        transform.position = Vector3.MoveTowards(
            transform.position, target, patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
            patrolDirection *= -1;
    }

    void DoChase()
    {
        if (pathfinder == null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            return;
        }

        if (Time.time - lastPathUpdate > pathUpdateInterval)
        {
            currentPath = pathfinder.FindPath(transform.position, player.position);
            pathIndex = 0;
            lastPathUpdate = Time.time;
        }

        if (currentPath != null && pathIndex < currentPath.Count)
        {
            Vector3 target = currentPath[pathIndex];
            transform.position = Vector3.MoveTowards(
                transform.position, target, chaseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.1f)
                pathIndex++;
        }
    }

    void DoAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("Enemigo ataca al jugador por " + attackDamage + " puntos de daño.");

            if (attackImpactPS != null)
            {
                attackImpactPS.Stop();
                attackImpactPS.Play();
            }
        }
    }

    void DoFlee()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        transform.position += dir * fleeSpeed * Time.deltaTime;
    }

    void StopAllParticles()
    {
        if (chaseTrailPS != null) chaseTrailPS.Stop();
        if (attackImpactPS != null) attackImpactPS.Stop();
        if (fleePS != null) fleePS.Stop();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log("Enemigo recibe " + amount + " de daño. Vida restante: " + currentHealth);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}