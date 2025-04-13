using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Teleporter targetTeleporter; // Посилання на інший телепорт
    public float teleportDelay = 1.0f; // Затримка перед телепортацією
    private bool isTeleporting = false; // Чи активний цей телепорт

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
        targetTeleporter.DisableTemporarily(); // Забороняємо телепортацію в цільовому на час

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

