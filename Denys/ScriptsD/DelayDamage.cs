using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDamage : MonoBehaviour
{
    [SerializeField] private float attackRange = 1f; // ��������� ����� (1 ����)
    

    private Animator en_Animator;

    private Transform player; // ������ �� ������
  
    private EnemyPatrol enemyPatrol; // ������ �� ��������� EnemyPatrol
    bool canAttack;
    [SerializeField] private int zombDamage = 15; // ���� �����
    [SerializeField] private float attackCooldown = 1f; // �������� ����� ������� (1 �������)

    private float lastAttackTime; // ����� ��������� �����
    void Start()
    {
    lastAttackTime = -attackCooldown; // ����� ����� ����� ��������� �����
        // ��������� ������� ���������� Animator
        en_Animator = GetComponent<Animator>();
        if (en_Animator == null)
        {
            //Debug.LogError("Animator component is missing on " + gameObject.name);
            enabled = false; // ��������� ������
            return;
        }

        // �������� ��������� EnemyPatrol
        enemyPatrol = GetComponent<EnemyPatrol>();
        if (enemyPatrol == null)
        {
            //Debug.LogError("EnemyPatrol component is missing on " + gameObject.name);
            enabled = false; // ��������� ������
            return;
        }

        lastAttackTime = -attackCooldown; // ����� ����� ����� ��������� �����
    }

    void Update()
    {
        if (en_Animator == null || enemyPatrol == null) return; // ����������, ���� ���������� �� ����������������

        // ���������� ����� DetectPlayer �� EnemyPatrol
        enemyPatrol.DetectPlayer();

        // �������� ������ �� EnemyPatrol
        player = enemyPatrol.GetPlayer();

        // ���� ������ ��� ��� �� ���������, ������ �� ������
        if (player == null || player.gameObject == null)
        {
            //Debug.Log("Player not found or destroyed.");
            return;
        }

        // ���������, ������������� �� �������� �����
        bool isAttacking = en_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        //Debug.Log("Is attacking: " + isAttacking);

        // ���������, ����� �� ��������� (������ �� ���������� �������)
        bool canAttack = Time.time - lastAttackTime >= attackCooldown;
        //Debug.Log("Can attack: " + canAttack);

        // ��������� ��������� �� ������
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
           // Debug.Log("Can Attack ");
            canAttack = true;
        }else{
            canAttack = false;
        }
       
    }
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // ���������, ������ �� ���������� ������� � ��������� �����
            bool canAttack = Time.time - lastAttackTime >= attackCooldown;
            if (canAttack)
            {
                PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(zombDamage, "Zombie", true);
                    lastAttackTime = Time.time; // ��������� ����� ��������� �����
                }
            }
        }
    }
}