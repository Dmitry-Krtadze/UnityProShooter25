using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDamage : MonoBehaviour
{
    [SerializeField] private float attackRange = 1f; // Дистанция атаки (1 блок)
    [SerializeField] private float attackCooldown = 1f; // Периодичность атаки (1 секунда)

    private Animator en_Animator;
    private float lastAttackTime; // Время последней атаки
    private Transform player; // Ссылка на игрока
    private int zombDamage = 15; // Урон зомби
    private EnemyPatrol enemyPatrol; // Ссылка на компонент EnemyPatrol

    void Start()
    {
        // Проверяем наличие компонента Animator
        en_Animator = GetComponent<Animator>();
        if (en_Animator == null)
        {
            Debug.LogError("Animator component is missing on " + gameObject.name);
            enabled = false; // Отключаем скрипт
            return;
        }

        // Получаем компонент EnemyPatrol
        enemyPatrol = GetComponent<EnemyPatrol>();
        if (enemyPatrol == null)
        {
            Debug.LogError("EnemyPatrol component is missing on " + gameObject.name);
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
            Debug.Log("Player not found or destroyed.");
            return;
        }

        // Проверяем, проигрывается ли анимация атаки
        bool isAttacking = en_Animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        Debug.Log("Is attacking: " + isAttacking);

        // Проверяем, можно ли атаковать (прошло ли достаточно времени)
        bool canAttack = Time.time - lastAttackTime >= attackCooldown;
        Debug.Log("Can attack: " + canAttack);

        // Проверяем дистанцию до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && isAttacking && canAttack)
        {
            // Наносим урон
            IDamageble damageable = player.gameObject.GetComponent<IDamageble>();
            if (damageable != null)
            {
                damageable.TakeDamage(zombDamage, "zombie", true);
                Debug.Log("Hit player");
                lastAttackTime = Time.time; // Обновляем время последней атаки
            }
            else
            {
                Debug.Log("IDamageble component not found on player!");
            }
        }
    }
}