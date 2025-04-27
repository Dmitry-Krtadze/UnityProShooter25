using UnityEngine;

public class TargetFall : MonoBehaviour
{
    public float fallDelay = 0.5f; // Время задержки перед падением
    private bool isHit = false; // Проверка, была ли мишень поражена
    private Rigidbody rb;  // Rigidbody для падения

    // Параметры для подъема
    private bool isRaising = false;
    private float raiseTime = 0f;
    private Vector3 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Сначала выключаем физику, чтобы мишень не падала сразу
        }
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isHit)
        {
            // Падение мишени
            if (rb != null)
            {
                rb.isKinematic = false; // Включаем физику
                rb.AddForce(Vector3.down * 10, ForceMode.Impulse); // Добавляем силу для падения
            }
            isHit = false;

            // Поднятие мишени через случайное время
            float randomTime = Random.Range(1f, 5f);
            Invoke("RaiseTarget", randomTime);
        }

        // Плавное поднятие
        if (isRaising)
        {
            raiseTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, initialPosition, raiseTime / 2f); // Поднимаем мишень обратно
            if (raiseTime >= 2f)
            {
                isRaising = false;
            }
        }
    }

    // Метод для того, чтобы мишень "запомнила", что её поразили
    public void OnHit()
    {
        //Debug.Log("fall");
        if (!isHit)
        {
            isHit = true;
        }
    }

    // Метод для подъема мишени после случайного времени
    private void RaiseTarget()
    {
        isRaising = true;
    }
}
