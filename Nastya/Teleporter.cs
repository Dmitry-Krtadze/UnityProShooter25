using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Teleporter targetTeleporter; // Посилання на інший телепорт
    public float teleportDelay = 1.0f; // Затримка перед телепортацією
    private bool isTeleporting = false; // Запобігає зацикленій телепортації

    private void OnCollisionEnter(Collision other)
    {
        if (!isTeleporting && targetTeleporter != null)
        {
            StartCoroutine(TeleportWithDelay(other.gameObject));
        }
    }

    private IEnumerator TeleportWithDelay(GameObject obj)
    {
        isTeleporting = true;
        yield return new WaitForSeconds(teleportDelay);

        if (targetTeleporter != null && obj != null)
        {
            obj.transform.position = targetTeleporter.transform.position;
        }

        isTeleporting = false;
    }
}
