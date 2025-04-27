using UnityEngine;

public class Target : MonoBehaviour
{
    public enum TargetType { Static, Moving }  // Тип мишени (статическая или движущаяся)

    public TargetType targetType = TargetType.Static;  // Тип мишени (выбирается в инспекторе)

    public float fallDuration = 1f;  // Длительность падения
    public float raiseDuration = 2f;  // Длительность подъема
    public float minRaiseTime = 1f;  // Минимальное время до подъема
    public float maxRaiseTime = 5f;  // Максимальное время до подъема

    public Transform movePoint1;  // Первая точка для движения родителя мишени
    public Transform movePoint2;  // Вторая точка для движения родителя мишени
    public float moveSpeedMin = 1f;  // Минимальная скорость движения
    public float moveSpeedMax = 5f;  // Максимальная скорость движения

    private Quaternion initialRotation;  // Начальная ориентация
    private Quaternion fallenRotation;  // Ориентация в падении
    private bool isFalling = false;  // Флаг падения
    private bool isRaising = false;  // Флаг подъема
    private float fallStartTime;  // Время начала падения
    private float raiseStartTime;  // Время начала подъема

    private Transform parentTransform;  // Родительский объект для поворота
    private bool isMoving = true;  // Флаг движения для движущихся мишеней
    private float currentSpeed;  // Текущая скорость движения
    private Vector3 targetPosition;  // Текущая целевая позиция для движения

    void Start()
    {
        parentTransform = transform.parent;  // Сохраняем ссылку на родительский объект
        initialRotation = parentTransform.rotation;  // Исходный поворот родительского объекта
        fallenRotation = Quaternion.Euler(0, 90, 90);  // Поворот для падения

        // Проверка на наличие родительского объекта
        if (parentTransform == null)
        {
            Debug.LogError("Отсутствует родительский объект для вращения!");
        }

        // Если мишень движущаяся, выбираем случайную скорость и начальную точку
        if (targetType == TargetType.Moving)
        {
            currentSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
            targetPosition = movePoint1.position;  // Начинаем движение от первой точки
        }
    }

    void Update()
    {
        // Обработка падения и подъема мишени
        if (isFalling)
        {
            float t = (Time.time - fallStartTime) / fallDuration;  // Время, прошедшее с начала падения
            parentTransform.rotation = Quaternion.Slerp(initialRotation, fallenRotation, t);  // Плавный переход к падению

            if (t >= 1f)
            {
                isFalling = false;
                float randomRaiseTime = Random.Range(minRaiseTime, maxRaiseTime);  // Случайное время для подъема
                Invoke("StartRaising", randomRaiseTime);  // Запускаем подъем после задержки
            }
        }

        if (isRaising)
        {
            float t = (Time.time - raiseStartTime) / raiseDuration;  // Время, прошедшее с начала подъема
            parentTransform.rotation = Quaternion.Slerp(fallenRotation, initialRotation, t);  // Плавный переход к исходному положению

            if (t >= 1f)
            {
                isRaising = false;
            }
        }

        // Если мишень движется
        if (targetType == TargetType.Moving && isMoving)
        {
            // Двигаем родительский объект между двумя точками
            parentTransform.position = Vector3.MoveTowards(parentTransform.position, targetPosition, currentSpeed * Time.deltaTime);

            // Когда родительский объект достиг целевой точки, меняем целевую позицию
            if (Vector3.Distance(parentTransform.position, targetPosition) < 0.1f)
            {
                targetPosition = (targetPosition == movePoint1.position) ? movePoint2.position : movePoint1.position;
            }
        }
    }

    // Метод для того, чтобы мишень "запомнила", что её поразили
    public void OnHit()
    {
        if (targetType == TargetType.Moving && isMoving)
        {
            isMoving = false;  // Останавливаем движение
            // Запускаем случайную задержку, прежде чем мишень снова начнёт двигаться
            float randomDelay = Random.Range(1f, 3f);  // Рандомная задержка перед восстановлением
            Invoke("ResumeMovement", randomDelay);  // Восстановление движения
        }

        if (!isFalling && !isRaising)
        {
            isFalling = true;  // Начинаем падение
            fallStartTime = Time.time;  // Запоминаем время начала падения
        }
    }

    // Восстановление движения после "перезарядки"
    private void ResumeMovement()
    {
        currentSpeed = Random.Range(moveSpeedMin, moveSpeedMax);  // Выбираем случайную скорость
        isMoving = true;  // Восстанавливаем движение
    }

    // Запускаем подъем мишени
    private void StartRaising()
    {
        isRaising = true;
        raiseStartTime = Time.time;  // Запоминаем время начала подъема
    }
}
