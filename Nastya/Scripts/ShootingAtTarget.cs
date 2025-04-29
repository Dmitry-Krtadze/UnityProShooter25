using UnityEngine;

public class TargetFall : MonoBehaviour
{
    public float fallDelay = 0.5f; // ����� �������� ����� ��������
    private bool isHit = false; // ��������, ���� �� ������ ��������
    private Rigidbody rb;  // Rigidbody ��� �������

    // ��������� ��� �������
    private bool isRaising = false;
    private float raiseTime = 0f;
    private Vector3 initialPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // ������� ��������� ������, ����� ������ �� ������ �����
        }
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isHit)
        {
            // ������� ������
            if (rb != null)
            {
                rb.isKinematic = false; // �������� ������
                rb.AddForce(Vector3.down * 10, ForceMode.Impulse); // ��������� ���� ��� �������
            }
            isHit = false;

            // �������� ������ ����� ��������� �����
            float randomTime = Random.Range(1f, 5f);
            Invoke("RaiseTarget", randomTime);
        }

        // ������� ��������
        if (isRaising)
        {
            raiseTime += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, initialPosition, raiseTime / 2f); // ��������� ������ �������
            if (raiseTime >= 2f)
            {
                isRaising = false;
            }
        }
    }

    // ����� ��� ����, ����� ������ "���������", ��� � ��������
    public void OnHit()
    {
        //Debug.Log("fall");
        if (!isHit)
        {
            isHit = true;
        }
    }

    // ����� ��� ������� ������ ����� ���������� �������
    private void RaiseTarget()
    {
        isRaising = true;
    }
}
