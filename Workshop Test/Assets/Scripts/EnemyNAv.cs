using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNAv : MonoBehaviour
{
    public UnityEngine.AI.NavMeshAgent navMeshAgent;
    private bool playerSeen;
    private bool patrolStart = false;
    private float patrolCooldown = 1f;
    private bool isRotatingLeft = false;
    private bool isRotatingRight = false;
    private bool isWalking = false;
    private bool canChase = true;
    private Collider[] playerColliders;
    //private Collider[] playerCollidersRear;
    public LayerMask playerLayer;
    public LayerMask obstacleLayer;
    private Transform player;
    [SerializeField] private float fieldOfView = 60f;
    [SerializeField] private float detectionRange = 25f;
    [SerializeField] private float detectionBuffer = 2f;
    [SerializeField] private float rearDetectionRange = -25f;
    [SerializeField] private float rearDetectionBuffer = -2f;
    IEnumerator patrol;
    IEnumerator restartPatrol;
    IEnumerator chasePlayer;
    // public GameObject meleeEnemy;
    // public GameObject rangedEnemy;
    // public GameObject parent;
    // Start is called before the first frame update
    void Start()
    {
        // meleeEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        // rangedEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        patrol = this.Patrol();
        restartPatrol = this.RestartPatrol();
        chasePlayer = this.ChasePlayer();
        // meleeEnemy.transform.parent = parent.transform;
        // rangedEnemy.transform.parent = parent.transform;
    }

    // Update is called once per frame
    void Update()
    {
        playerColliders = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        //playerCollidersRear = Physics.OverlapSphere(transform.position, rearDetectionRange, playerLayer);
        foreach (var playerCollider in playerColliders)
        {
            player = playerCollider.transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float dstToPlayer = Vector3.Distance(transform.position, player.position);
            var newRotation = Quaternion.LookRotation(dirToPlayer);

            if (Vector3.Angle(transform.forward, dirToPlayer) < fieldOfView && !Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleLayer) || dstToPlayer <= (navMeshAgent.stoppingDistance + detectionBuffer) && !Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleLayer))
            {
                //Debug.Log("Spotted!");
                playerSeen = true;
                StopCoroutine(patrol);
                patrol = Patrol();
                StopCoroutine(restartPatrol);
                restartPatrol = RestartPatrol();
                patrolStart = false;
                patrolCooldown = 1f;
                navMeshAgent.isStopped = false;
                Debug.Log("Player Detected!"); // Output a debug message when the player is seen.
                Gizmos.color = Color.red;
                if (canChase)
                {
                    StartCoroutine(ChasePlayer());
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 1f);
            }

            else
            {
                StopCoroutine(ChasePlayer());
                chasePlayer = ChasePlayer();
                canChase = true;
                playerSeen = false;
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }
        }

        /*foreach (var playerCollider in playerCollidersRear)
        {
            player = playerCollider.transform;
            Vector3 dirToPlayer = (player.position - transform.position).normalized * -1;
            float dstToPlayer = Vector3.Distance(transform.position, player.position);
            var newRotation = Quaternion.LookRotation(dirToPlayer);

            if (Vector3.Angle(transform.forward, dirToPlayer) < fieldOfView && !Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleLayer) || dstToPlayer <= (navMeshAgent.stoppingDistance + rearDetectionBuffer) && !Physics.Raycast(transform.position, dirToPlayer, dstToPlayer, obstacleLayer))
            {
                //Debug.Log("Spotted!");
                playerSeen = true;
                StopCoroutine(patrol);
                patrol = Patrol();
                StopCoroutine(restartPatrol);
                restartPatrol = RestartPatrol();
                patrolStart = false;
                patrolCooldown = 1f;
                navMeshAgent.isStopped = false;
                Debug.Log("Player Detected Behind!"); // Output a debug message when the player is seen.
                Gizmos.color = Color.green;
                if (canChase)
                {
                    StartCoroutine(ChasePlayer());
                }
                // Rotate 180 degrees when player is detected behind
                //Rotate180Degrees();
                transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 1f);
            }
            else
            {
                StopCoroutine(ChasePlayer());
                chasePlayer = ChasePlayer();
                canChase = true;
                playerSeen = false;
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
            }
        }*/

        if (playerSeen == false && patrolStart == false)
        {
            patrolCooldown -= Time.deltaTime;
            if (patrolCooldown <= 0)
            {
                StartCoroutine("Patrol");
            }
        }

        if (isRotatingRight == true)
        {
            transform.Rotate(transform.up * Time.deltaTime * 50f);
        }

        if (isRotatingLeft == true)
        {
            transform.Rotate(transform.up * Time.deltaTime * -50f);
        }
        if (isRotatingRight == false)
        {
            transform.Rotate(transform.up * Time.deltaTime * -50f);
        }

        if (isRotatingLeft == false)
        {
            transform.Rotate(transform.up * Time.deltaTime * 50f);
        }

        if (isWalking == true)
        {
            if (!Physics.Raycast(transform.position, transform.forward, 2f, obstacleLayer))
            {
                transform.position += transform.forward * 5f * Time.deltaTime;
            }
            else
            {
                StartCoroutine("RestartPatrol");
            }
        }
    }

    private void Rotate180Degrees()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y + 180f, currentRotation.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // Draw the FOV area
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the FOV cone
        float halfFOV = fieldOfView / 2;
        Quaternion leftRot = Quaternion.Euler(0, -halfFOV, 0);
        Quaternion rightRot = Quaternion.Euler(0, halfFOV, 0);
        Vector3 leftDir = leftRot * transform.forward;
        Vector3 rightDir = rightRot * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + leftDir * detectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * detectionRange);

        Gizmos.DrawLine(transform.position, transform.position + leftDir * rearDetectionRange);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * rearDetectionRange);
    }
    IEnumerator ChasePlayer()
    {
        canChase = false;
        navMeshAgent.SetDestination(player.position);
        Debug.Log("chasing");
        yield return new WaitForSeconds(0.5f);
        canChase = true;
    }
    IEnumerator Patrol()
    {
        patrolStart = true;
        int rotTime = Random.Range(1, 3);
        int rotateWait = Random.Range(1, 3);
        int rotateLorR = Random.Range(1, 2);
        int walkWait = Random.Range(1, 3);
        int walkTime = Random.Range(1, 4);

        yield return new WaitForSeconds(walkWait);
        isWalking = true;
        yield return new WaitForSeconds(walkTime);
        isWalking = false;
        yield return new WaitForSeconds(rotateWait);
        if (rotateLorR == 1)
        {
            isRotatingRight = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingRight = false;
        }
        if (rotateLorR == 2)
        {
            isRotatingLeft = true;
            yield return new WaitForSeconds(rotTime);
            isRotatingLeft = false;
        }
        patrolStart = false;
        patrolCooldown = 1f;
    }
    IEnumerator RestartPatrol()
    {
        StopCoroutine(patrol);
        patrol = Patrol();
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180f * Time.deltaTime, transform.rotation.z);
        yield return new WaitForSeconds(1f);
        patrolStart = false;
        patrolCooldown = 1f;
    }
}
