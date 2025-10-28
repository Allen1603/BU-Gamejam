using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyCatch : MonoBehaviour
{
    [Header("References")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask playerMask;
    public Transform grabHoldPoint; // Empty child object (enemy’s hand or front area)

    [Header("Catch Settings")]
    public float sightRange = 8f;
    public float catchRange = 1.5f;
    public float catchCooldown = 3f;

    private bool playerInSightRange;
    private bool playerInCatchRange;
    private bool isCatching;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        // Look for the player
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, playerMask);
        playerInCatchRange = Physics.CheckSphere(transform.position, catchRange, playerMask);

        if (playerInSightRange && player != null && !isCatching)
            ChasePlayer();

        if (playerInCatchRange && !isCatching)
            CatchPlayer();
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
        FaceTarget(player);
    }

    private void CatchPlayer()
    {
        isCatching = true;
        agent.isStopped = true;
        FaceTarget(player);

        Debug.Log("Enemy caught the player!");

        // Stop player movement and attach to enemy
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
            controller.enabled = false;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        // Parent the player to the enemy grab point
        if (grabHoldPoint != null)
        {
            player.SetParent(grabHoldPoint);
            player.localPosition = Vector3.zero;
        }
        else
        {
            // Fallback: attach in front of the enemy
            player.SetParent(transform);
            player.localPosition = new Vector3(0, 1, 0.5f);
        }

        // Optional: trigger catch animation
        // Animator anim = GetComponent<Animator>();
        // anim.SetTrigger("Catch");

        Invoke(nameof(KillPlayer), 2f); // wait 2 seconds before reset
        Invoke(nameof(ResetCatch), catchCooldown);
    }

    private void KillPlayer()
    {
        // Unparent before reload
        if (player != null)
            player.SetParent(null);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void FaceTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
    }

    private void ResetCatch()
    {
        isCatching = false;
        agent.isStopped = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRange);
    }
}
