using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float enemyDmg = 10;

    public float enemyHealth = 100;

    PlayerController playerController;
    public GameObject player;

    void Start()
    {
        playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        Debug.Log("Enemy Health: " + enemyHealth);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            playerController.playerHealth -= enemyDmg;
        }
    }
}
