using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;
    private float searchTimer = 0f;
    private float searchInterval = 1f; // шукати кожну секунду

    void Update()
    {
        if (player == null)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= searchInterval)
            {
                GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
                if (foundPlayer != null)
                {
                    player = foundPlayer.transform;
                }
                searchTimer = 0f;
            }
        }
        else
        {
            // ќбчислюЇмо напр€мок ≥ компенсуЇмо поворот (€кщо обличч€ дивитьс€ вздовж ос≥ X)
            Vector3 direction = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(0, -90, 0);
        }
    }
}
