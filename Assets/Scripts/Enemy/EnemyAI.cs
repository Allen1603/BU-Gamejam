using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask groundMask;
    public LayerMask playerMask;
    public Transform grabHoldPoint;

    [Header("Patrolling")]
    public float walkPointRange = 5f;
    public float idleTime = 2f;
    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool isIdling;

    [Header("Chase Settings")]
    public float sightRange = 8f;
    public float loseSightDelay = 3f;
    private float loseSightTimer;

    [Header("Catch Settings")]
    public float catchRange = 1.5f;
    public float catchCooldown = 3f;
    private bool isCatching;

    [Header("Dazzle Settings")]
    public float dazzleResistance = 100f;   // "Health" before disappearing
    public float retreatSpeed = 1.5f;       // Backward speed when dazzled
    public float dazzleDecayDelay = 0.5f;   // Time before it stops retreating
    private bool isRetreating;
    private bool isDying;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    private bool playerInSightRange;
    private bool playerInCatchRange;
    private Coroutine retreatRoutine;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (isDying || isRetreating) return;

        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        playerInCatchRange = Physics.CheckSphere(transform.position, catchRange, playerMask);

        if (isCatching) return;

        if (!playerInSightRange && !playerInCatchRange)
        {
            if (loseSightTimer > 0)
            {
                loseSightTimer -= Time.deltaTime;
                ChaseLastKnownPosition();
            }
            else
            {
                Patrolling();
            }
        }
        else if (playerInSightRange && !playerInCatchRange)
        {
            ChasePlayer();
            loseSightTimer = loseSightDelay;
        }
        else if (playerInCatchRange)
        {
            CatchPlayer();
        }
    }

    // ---------------- PATROLLING ----------------
    private void Patrolling()
    {
        if (isIdling) return;

        if (!walkPointSet)
            SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        if (distanceToWalkPoint.magnitude < 1f)
        {
            walkPointSet = false;
            StartCoroutine(IdleBeforeNextPatrol());
        }
    }

    private IEnumerator IdleBeforeNextPatrol()
    {
        isIdling = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(idleTime);
        agent.isStopped = false;
        isIdling = false;
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 potentialPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(potentialPoint, -transform.up, 2f, groundMask))
        {
            walkPoint = potentialPoint;
            walkPointSet = true;
        }
    }

    // ---------------- CHASING ----------------
    private void ChasePlayer()
    {
        if (player == null) return;

        agent.isStopped = false;
        agent.SetDestination(player.position);
        FaceTarget(player);
    }

    private void ChaseLastKnownPosition()
    {
        if (player == null) return;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    // ---------------- CATCHING ----------------
    private void CatchPlayer()
    {
        isCatching = true;
        agent.isStopped = true;
        FaceTarget(player);

        Debug.Log("Enemy caught the player!");

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        if (grabHoldPoint != null)
        {
            player.SetParent(grabHoldPoint);
            player.localPosition = Vector3.zero;
        }
        else
        {
            player.SetParent(transform);
            player.localPosition = new Vector3(0, 1, 0.5f);
        }

        Invoke(nameof(KillPlayer), 2f);
        Invoke(nameof(ResetCatch), catchCooldown);
    }

    private void KillPlayer()
    {
        if (player != null)
            player.SetParent(null);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ResetCatch()
    {
        isCatching = false;
        agent.isStopped = false;
    }

    // ---------------- DAZZLE EFFECT ----------------
    public void ApplyDazzle(float amount)
    {
        if (isDying) return;

        dazzleResistance -= amount;
        if (retreatRoutine == null)
            retreatRoutine = StartCoroutine(Retreat());

        if (dazzleResistance <= 0)
            StartCoroutine(Disappear());
    }

    private IEnumerator Retreat()
    {
        isRetreating = true;
        agent.isStopped = true;

        float timer = 0f;

        while (timer < dazzleDecayDelay)
        {
            if (player == null) break;

            Vector3 dir = (transform.position - player.position).normalized;
            dir.y = 0;
            transform.position += dir * retreatSpeed * Time.deltaTime;

            FaceTarget(player);
            timer += Time.deltaTime;
            yield return null;
        }

        isRetreating = false;
        agent.isStopped = false;
        retreatRoutine = null;
    }

    private IEnumerator Disappear()
    {
        isDying = true;
        agent.isStopped = true;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        float fadeTime = 1.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            foreach (Renderer r in renderers)
            {
                if (r.material.HasProperty("_Color"))
                {
                    Color c = r.material.color;
                    c.a = alpha;
                    r.material.color = c;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    // ---------------- UTILITY ----------------
    private void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, walkPointRange);
    }
}
