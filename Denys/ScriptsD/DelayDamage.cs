using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDamage : MonoBehaviour
{
    [SerializeField] private float attackRange = 1f; // Дистанция атаки (1 блок)
    

    private Animator en_Animator;

    private Transform player; // Ссылка на игрока
  
    private EnemyPatrol enemyPatrol; // Ссылка на компонент EnemyPatrol
    bool canAttack;
    [SerializeField] private int zombDamage = 15; // Урон зомби
    [SerializeField] private float attackCooldown = 1f; // Задержка между атаками (1 секунда)

    private float lastAttackTime; // Время последней атаки
    void Start()
    {
    lastAttackTime = -attackCooldown; // Чтобы атака могла сработать сразу
        // Проверяем наличие компонента Animator
        en_Animator = GetComponent<Animator>();
        if (en_Animator == null)
        {
            //Debug.LogError("Animator component is missing on " + gameObject.name);
            enabled = false; // Отключаем скрипт
            return;
        }

        // Получаем компонент EnemyPatrol
        enemyPatrol = GetComponent<EnemyPatrol>();
        if (enemyPatrol == null)
        {
            //Debug.LogError("EnemyPatrol component is missing on " + gameObject.name);
            enabled = false; // Отключаем скрипт
            return;
        }

        lastAttackTime = -attackCooldown; // Чтобы атака могла сработать сразу
    }

    void Update()
    {
        if (en_Animator == null || enemyPatrol == null) return; // Пропускаем, если компоненты не инициализированы

        // Используем метод DetectPlayer из EnemyPatrol
        enemyPatrol.DetectPlayer();

        // Получаем игрока из EnemyPatrol
        player = enemyPatrol.GetPlayer();

        // Если игрока нет или он уничтожен, ничего не делаем
        if (player == null || player.gameObject == null)
        {
            //Debug.Log("Player not found or destroyed.");
            return;
        }

        // Проверяем, проигрывается ли анимация атаки
        bool isAttacking = en_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        //Debug.Log("Is attacking: " + isAttacking);

        // Проверяем, можно ли атаковать (прошло ли достаточно времени)
        bool canAttack = Time.time - lastAttackTime >= attackCooldown;
        //Debug.Log("Can attack: " + canAttack);

        // Проверяем дистанцию до игрока
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
            // Проверяем, прошло ли достаточно времени с последней атаки
            bool canAttack = Time.time - lastAttackTime >= attackCooldown;
            if (canAttack)
            {
                PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    playerController.TakeDamage(zombDamage, "Zombie", true);
                    lastAttackTime = Time.time; // Обновляем время последней атаки
                }
            }
        }
    }
}