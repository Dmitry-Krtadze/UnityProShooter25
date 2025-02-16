using Photon.Pun;
using UnityEngine;

public class ItemPickup : MonoBehaviourPunCallbacks
{
    public enum ItemType { HealthKit, AmmoKit, Shield }
    public ItemType itemType;


    public int value;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && photonView.IsMine)
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            Item item = other.GetComponentInChildren<ShootGun>();

            if (playerController != null)
            {
                value = Random.Range(20, 50); // Значение (количество патронов или здоровья)
                switch (itemType)
                {
                    case ItemType.HealthKit:
                        playerController.AddHealth(value);
                        break;
                    case ItemType.AmmoKit:
                        item.AddAmmo(value);
                        break;
                    case ItemType.Shield:
                        playerController.ActivateShield();
                        break;
                }

                // Синхронизируем удаление предмета между клиентами
                photonView.RPC("DestroyItem", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    private void DestroyItem()
    {
        Destroy(gameObject);
    }
}
