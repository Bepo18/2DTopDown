using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private enum State { Roaming, Chasing, Attacking }
    public enum EnemyType { Slime, Skeleton, MiniBoss }

    [Header("Enemy Settings")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;

    private State state;
    private EnemyPathfinding enemyPathfinding;
    private Transform player;
    private bool canAttack = true;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        FindPlayerSafely();
        state = State.Roaming;
    }

    private void Start()
    {
        StartCoroutine(StateHandler());
    }

    // Try to find the player by tag safely (call whenever we detect null)
    private void FindPlayerSafely()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        player = (p != null) ? p.transform : null;
    }

    private IEnumerator StateHandler()
    {
        while (true)
        {
            // If player reference gets destroyed, try to find it again.
            if (player == null)
            {
                FindPlayerSafely();
            }

            float distanceToPlayer = float.PositiveInfinity;
            if (player != null)
                distanceToPlayer = Vector2.Distance(transform.position, player.position);

            switch (state)
            {
                case State.Roaming:
                    enemyPathfinding.MoveTo(GetRoamingDirection());
                    if (player != null && distanceToPlayer < detectionRange)
                        state = State.Chasing;
                    yield return new WaitForSeconds(2f);
                    break;

                case State.Chasing:
                    if (player != null)
                        enemyPathfinding.MoveTo((player.position - transform.position).normalized);
                    else
                        enemyPathfinding.MoveTo(GetRoamingDirection()); // fallback roaming when player missing

                    if (player != null && distanceToPlayer <= attackRange)
                        state = State.Attacking;
                    else if (player == null || distanceToPlayer > detectionRange * 1.5f)
                        state = State.Roaming;

                    yield return null;
                    break;

                case State.Attacking:
                    enemyPathfinding.MoveTo(Vector2.zero);
                    if (canAttack)
                        StartCoroutine(AttackRoutine());
                    yield return null;
                    break;
            }
        }
    }

    private Vector2 GetRoamingDirection()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        int hitCount = (enemyType == EnemyType.MiniBoss) ? 2 : 1;
        int damagePerHit = 1; // tweak per enemy if needed

        for (int i = 0; i < hitCount; i++)
        {
            // Always guard access to player
            if (player == null)
            {
                // Try re-finding; if still null, abort the attack
                FindPlayerSafely();
                if (player == null)
                    break;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange + 0.3f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerHit);
                    Debug.Log($"{enemyType} dealt {damagePerHit} damage to Player!");
                }
            }

            // small delay between boss hits, or single hit delay
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
        state = State.Chasing;
    }
}
