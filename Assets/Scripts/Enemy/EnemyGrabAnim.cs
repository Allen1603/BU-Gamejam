using UnityEngine;

public class EnemyGrabAnim : MonoBehaviour
{
    public Transform grabHoldPoint;

    private EnemyAI enemyAI;
    private Transform player;
    private Animator playerAnimator;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemyAI = GetComponentInParent<EnemyAI>();
        playerAnimator = player.GetComponentInChildren<Animator>();
    }

    public void SnapGrab()
    {
        if (player == null || grabHoldPoint == null || enemyAI == null) return;

        // Attach player to grab point
        player.SetParent(grabHoldPoint);
        player.localPosition = Vector3.zero;
        player.localRotation = Quaternion.identity;

        // Adjust height alignment
        CapsuleCollider col = player.GetComponent<CapsuleCollider>();
        if (col != null)
            player.localPosition += Vector3.up * (col.height * 0.5f);
        else
            player.localPosition += Vector3.up * 0.9f;

        playerAnimator.SetBool("IsGrabbed", true);

        // Schedule player kill and reset
        StartCoroutine(DelayedActions());
    }

    private System.Collections.IEnumerator DelayedActions()
    {
        yield return new WaitForSeconds(2f);
        enemyAI.KillPlayer();

        yield return new WaitForSeconds(enemyAI.catchCooldown);
        enemyAI.ResetCatch();
    }
}
