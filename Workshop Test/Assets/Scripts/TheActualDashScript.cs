using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheActualDashScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCam;
    private Rigidbody rb;
    private PlayerController pm;

    [Header("Dashing")]
    [SerializeField] private float dashForce;
    [SerializeField] private float dashUpwardForce;
    [SerializeField] private float dashDuration;

    [Header("Cooldown")]
    [SerializeField] public float baseSkillCooldown;
    [SerializeField] private float newBaseSkillCooldown;
    [SerializeField] private float originalBaseSkillCooldown;
    public float dashCdTimer;

    [Header("Input")]
    private ThirdPersonActionsAsset thirdPersonActionsAsset;

    [Header("Enemy Collider Reference")]
    Collider enemyCollider;
    Enemy enemy;
    public LayerMask intendedLayer;
    [SerializeField] private float blitzRadius = 5f;
    [SerializeField] private float baseSkillDamage = 10f;

    private int collisionCount;

    // start of input
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
    // end of input

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerController>();

        // enemy collider
        enemyCollider = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Collider>();
        enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Enemy>();
    }

    private void Update()
    {
        if (collisionCount == 0)
                baseSkillCooldown = originalBaseSkillCooldown;

        if (thirdPersonActionsAsset.Player.Dash.triggered)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, blitzRadius, intendedLayer))
                collisionCount = 1;
            else
                collisionCount = 0;

            Dash();
        }

        if (dashCdTimer > 0)
            dashCdTimer -= Time.deltaTime;
    }

    private void Dash()
    {
        if (dashCdTimer > 0)
            return;
        else
        {
            dashCdTimer = baseSkillCooldown;
        }
        pm.dashing = true;
        enemyCollider.isTrigger = true;
        pm.isInvincible = true;
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;

        rb.AddForce(forceToApply, ForceMode.Impulse);

        delayedForceToApply = forceToApply;

        Invoke(nameof(delayedForceToApply), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            baseSkillCooldown = newBaseSkillCooldown;
            enemy.enemyHealth -= baseSkillDamage;
            collisionCount = 1;
        }
    }

    private Vector3 delayedForceToApply;
    private void DelayedForceToApply()
    {
        rb.AddForce(delayedForceToApply, ForceMode.Impulse);
    }

    private void ResetDash()
    {
        pm.dashing = false;
        enemyCollider.isTrigger = false;
    }    
}
