using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashScript : MonoBehaviour
{
    // cooldown 8 seconds 
    [SerializeField] private float baseSkillCooldown = 8f;

    // unlock at 5
    //float speedLevel = 5;

    // damage 20
    [SerializeField] private float baseSkillDamage = 10f;

    // range 5
    [SerializeField] private float blitzRadius = 5f;

    // bool to check if the character is dashing
    [SerializeField] private bool isDashing = false;

    // dash time
    [SerializeField] private float dashTime = 35f;

    // description
    // dash in faced direction
    // get player look rotation

    // enemy that is in dash path gets damage
    // check collision

    // hits enemy cooldown = 4 seconds
    /* if (hit enemy)
        {
            baseSkillCooldown = 4f;
        }
    */

    // get enemy damage
    Enemy enemy;
    public GameObject enemyObject;

    // public float initialEnemyDmg;
    public GameObject playerObject;

    Rigidbody rb;

    public Transform raycastTransform;

    public LayerMask intendedLayer;

    private ThirdPersonActionsAsset thirdPersonActionsAsset;

    private void Awake()
    {
        thirdPersonActionsAsset = new ThirdPersonActionsAsset();
    }

    private void OnEnable()
    {
        thirdPersonActionsAsset.Enable();
    }

    private void OnDisable()
    {
        thirdPersonActionsAsset.Disable();
    }

    void Start()
    {
        enemy = enemyObject.GetComponent<Enemy>();

        // initialEnemyDmg = enemy.enemyDmg;

        isDashing = false;
        rb = playerObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (thirdPersonActionsAsset.Player.Dash.triggered)
        {
            isDashing = true;
        }

        StartCoroutine(DashingTime());

        if (isDashing == true)
        {
            CheckingDash();
        }
            
        if (isDashing == false)
        {
            StopDashing();
        }
    }

    IEnumerator DashingTime()
    {
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
    }

    void CheckingDash()
    {
        playerObject.GetComponent<Collider>().enabled = false;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        Vector3 forwardPosition = playerObject.transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(raycastTransform.transform.position, forwardPosition, blitzRadius, intendedLayer))
        {
            enemy.enemyHealth -= baseSkillDamage;
        }

        //enemy.enemyDmg = 0; 
    }

    void StopDashing()
    {
        playerObject.GetComponent<Collider>().enabled = true;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.freezeRotation = true;
        //enemy.enemyDmg = initialEnemyDmg;
    }
}
