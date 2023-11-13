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
    Collider collider;
    Enemy enemy;

    public Transform raycastTransform;

    public LayerMask intendedLayer;
    [SerializeField] private float blitzRadius = 5f;
    [SerializeField] private float baseSkillDamage = 10f;

    bool collideEnemy;

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
        collider = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Collider>();
        enemy = GameObject.FindGameObjectWithTag("Enemy").GetComponent<Enemy>();
    }

    private void Update()
    {
        if (thirdPersonActionsAsset.Player.Dash.triggered)
        {
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
        collider.isTrigger = true;
        pm.isInvincible = true;
        Vector3 forceToApply = orientation.forward * dashForce + orientation.up * dashUpwardForce;

        rb.AddForce(forceToApply, ForceMode.Impulse);

        delayedForceToApply = forceToApply;

        Invoke(nameof(delayedForceToApply), 0.025f);

        Invoke(nameof(ResetDash), dashDuration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy")
        {
            baseSkillCooldown = newBaseSkillCooldown;
            enemy.enemyHealth -= baseSkillDamage;
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
        collider.isTrigger = false;
    }    
}
