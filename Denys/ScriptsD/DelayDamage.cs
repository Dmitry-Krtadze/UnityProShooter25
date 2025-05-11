using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDamage : MonoBehaviour
{
    [SerializeField] private float attackRange = 1f; // ��������� ����� (1 ����)
    [SerializeField] private float attackCooldown = 1f; // ������������� ����� (1 �������)

    private Animator en_Animator;
    private float lastAttackTime; // ����� ��������� �����
    private Transform player; // ������ �� ������
    private int zombDamage = 15; // ���� �����
    private EnemyPatrol enemyPatrol; // ������ �� ��������� EnemyPatrol

    void Start()
    {
        // ��������� ������� ���������� Animator
        en_Animator = GetComponent<Animator>();
        if (en_Animator == null)
        {
            Debug.LogError("Animator component is missing on " + gameObject.name);
            enabled = false; // ��������� ������
            return;
        }

        // �������� ��������� EnemyPatrol
        enemyPatrol = GetComponent<EnemyPatrol>();
        if (enemyPatrol == null)
        {
            Debug.LogError("EnemyPatrol component is missing on " + gameObject.name);
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
            Debug.Log("Player not found or destroyed.");
            return;
        }

        // ���������, ������������� �� �������� �����
        bool isAttacking = en_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        Debug.Log("Is attacking: " + isAttacking);

        // ���������, ����� �� ��������� (������ �� ���������� �������)
        bool canAttack = Time.time - lastAttackTime >= attackCooldown;
        Debug.Log("Can attack: " + canAttack);

        // ��������� ��������� �� ������
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && isAttacking && canAttack)
        {
            // ������� ����
            IDamageble damageable = player.gameObject.GetComponent<IDamageble>();
            if (damageable != null)
            {
                damageable.TakeDamage(zombDamage, "zombie", true);
                Debug.Log("Hit player");
                lastAttackTime = Time.time; // ��������� ����� ��������� �����
            }
            else
            {
                Debug.Log("IDamageble component not found on player!");
            }
        }
    }
}