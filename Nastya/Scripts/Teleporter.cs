using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Teleporter targetTeleporter; // ��������� �� ����� ��������
    public float teleportDelay = 1.0f; // �������� ����� �������������
    private bool isTeleporting = false; // �� �������� ��� ��������

    private void OnCollisionEnter(Collision other)
    {
        if (!isTeleporting && targetTeleporter != null && other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(TeleportWithDelay(other.gameObject));
        }
    }

    private IEnumerator TeleportWithDelay(GameObject obj)
    {
        isTeleporting = true;
        targetTeleporter.DisableTemporarily(); // ����������� ������������ � ��������� �� ���

        yield return new WaitForSeconds(teleportDelay);

        if (targetTeleporter != null && obj != null)
        {
            obj.transform.position = targetTeleporter.transform.position;
        }

        isTeleporting = false;
    }

    public void DisableTemporarily()
    {
        StartCoroutine(DisableCoroutine());
    }

    private IEnumerator DisableCoroutine()
    {
        isTeleporting = true;
        yield return new WaitForSeconds(teleportDelay);
        isTeleporting = false;
    }
}

