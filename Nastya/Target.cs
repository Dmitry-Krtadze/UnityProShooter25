using UnityEngine;

public class Target : MonoBehaviour
{
    public enum TargetType { Static, Moving }  // ��� ������ (����������� ��� ����������)

    public TargetType targetType = TargetType.Static;  // ��� ������ (���������� � ����������)

    public float fallDuration = 1f;  // ������������ �������
    public float raiseDuration = 2f;  // ������������ �������
    public float minRaiseTime = 1f;  // ����������� ����� �� �������
    public float maxRaiseTime = 5f;  // ������������ ����� �� �������

    public Transform movePoint1;  // ������ ����� ��� �������� �������� ������
    public Transform movePoint2;  // ������ ����� ��� �������� �������� ������
    public float moveSpeedMin = 1f;  // ����������� �������� ��������
    public float moveSpeedMax = 5f;  // ������������ �������� ��������

    private Quaternion initialRotation;  // ��������� ����������
    private Quaternion fallenRotation;  // ���������� � �������
    private bool isFalling = false;  // ���� �������
    private bool isRaising = false;  // ���� �������
    private float fallStartTime;  // ����� ������ �������
    private float raiseStartTime;  // ����� ������ �������

    private Transform parentTransform;  // ������������ ������ ��� ��������
    private bool isMoving = true;  // ���� �������� ��� ���������� �������
    private float currentSpeed;  // ������� �������� ��������
    private Vector3 targetPosition;  // ������� ������� ������� ��� ��������

    void Start()
    {
        parentTransform = transform.parent;  // ��������� ������ �� ������������ ������
        initialRotation = parentTransform.rotation;  // �������� ������� ������������� �������
        fallenRotation = Quaternion.Euler(0, 90, 90);  // ������� ��� �������

        // �������� �� ������� ������������� �������
        if (parentTransform == null)
        {
            Debug.LogError("����������� ������������ ������ ��� ��������!");
        }

        // ���� ������ ����������, �������� ��������� �������� � ��������� �����
        if (targetType == TargetType.Moving)
        {
            currentSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
            targetPosition = movePoint1.position;  // �������� �������� �� ������ �����
        }
    }

    void Update()
    {
        // ��������� ������� � ������� ������
        if (isFalling)
        {
            float t = (Time.time - fallStartTime) / fallDuration;  // �����, ��������� � ������ �������
            parentTransform.rotation = Quaternion.Slerp(initialRotation, fallenRotation, t);  // ������� ������� � �������

            if (t >= 1f)
            {
                isFalling = false;
                float randomRaiseTime = Random.Range(minRaiseTime, maxRaiseTime);  // ��������� ����� ��� �������
                Invoke("StartRaising", randomRaiseTime);  // ��������� ������ ����� ��������
            }
        }

        if (isRaising)
        {
            float t = (Time.time - raiseStartTime) / raiseDuration;  // �����, ��������� � ������ �������
            parentTransform.rotation = Quaternion.Slerp(fallenRotation, initialRotation, t);  // ������� ������� � ��������� ���������

            if (t >= 1f)
            {
                isRaising = false;
            }
        }

        // ���� ������ ��������
        if (targetType == TargetType.Moving && isMoving)
        {
            // ������� ������������ ������ ����� ����� �������
            parentTransform.position = Vector3.MoveTowards(parentTransform.position, targetPosition, currentSpeed * Time.deltaTime);

            // ����� ������������ ������ ������ ������� �����, ������ ������� �������
            if (Vector3.Distance(parentTransform.position, targetPosition) < 0.1f)
            {
                targetPosition = (targetPosition == movePoint1.position) ? movePoint2.position : movePoint1.position;
            }
        }
    }

    // ����� ��� ����, ����� ������ "���������", ��� � ��������
    public void OnHit()
    {
        if (targetType == TargetType.Moving && isMoving)
        {
            isMoving = false;  // ������������� ��������
            // ��������� ��������� ��������, ������ ��� ������ ����� ����� ���������
            float randomDelay = Random.Range(1f, 3f);  // ��������� �������� ����� ���������������
            Invoke("ResumeMovement", randomDelay);  // �������������� ��������
        }

        if (!isFalling && !isRaising)
        {
            isFalling = true;  // �������� �������
            fallStartTime = Time.time;  // ���������� ����� ������ �������
        }
    }

    // �������������� �������� ����� "�����������"
    private void ResumeMovement()
    {
        currentSpeed = Random.Range(moveSpeedMin, moveSpeedMax);  // �������� ��������� ��������
        isMoving = true;  // ��������������� ��������
    }

    // ��������� ������ ������
    private void StartRaising()
    {
        isRaising = true;
        raiseStartTime = Time.time;  // ���������� ����� ������ �������
    }
}
