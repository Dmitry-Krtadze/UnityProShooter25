using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;
    private float searchTimer = 0f;
    private float searchInterval = 1f;

    public sunScript sun; // <-- перетяни объект Sun с компонентом sunScript сюда в инспекторе

    [SerializeField] private float teleportDistance = 20f;
    [SerializeField] private Vector3 teleportPosition = new Vector3(54.7999992f, 73.6800003f, -40.5999985f);

    private bool hasTeleported = false;

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
            float distance = Vector3.Distance(transform.position, player.position);

            if ( distance < teleportDistance)
            {
                transform.position = teleportPosition;
      
                if (sun != null)
                {
                    sun.isChasingPlayer = false; // включаем преследование
                }
            }

            // поворот как в оригинале
            Vector3 direction = player.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation * Quaternion.Euler(0, -90, 0);

            
        }
    }
}
