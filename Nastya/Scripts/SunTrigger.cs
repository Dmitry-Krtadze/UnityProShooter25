using UnityEngine;

public class SunTrigger : MonoBehaviour
{
    public CanvasGroup canvasGroup; // ���������� ����� ���������
    public float fadeSpeed = 1f; // �������� �����
    private bool isFadingIn = false;

    private void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f; // ��������� ���������
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (isFadingIn && canvasGroup != null && canvasGroup.alpha < 1f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;

            if (canvasGroup.alpha >= 1f)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // ���������� ��� ������
        {
            isFadingIn = true;
        }
    }
}