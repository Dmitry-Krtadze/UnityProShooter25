using Photon.Pun;
using UnityEngine;

public class ItemPickup : MonoBehaviourPunCallbacks
{
    public enum ItemType { HealthKit, AmmoKit, Shield }
    public ItemType itemType;
    public int value; // Значение (количество патронов, здоровья и т.п.)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Обработка подбора только на мастер-клиенте
            if (PhotonNetwork.IsMasterClient)
            {
                // Определяем случайное значение для эффекта
                value = Random.Range(20, 50);

                PhotonView playerPhotonView = other.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    switch (itemType)
                    {
                        case ItemType.HealthKit:
                            // Вызываем RPC у игрока для добавления здоровья
                            playerPhotonView.RPC("RPC_AddHealth", RpcTarget.All, value);
                            break;
                        case ItemType.AmmoKit:
                            // Вызываем RPC у игрока для добавления патронов
                            playerPhotonView.RPC("RPC_AddAmmo", RpcTarget.All, value);
                            break;
                        case ItemType.Shield:
                            // Вызываем RPC у игрока для активации щита
                            playerPhotonView.RPC("RPC_ActivateShield", RpcTarget.All);
                            break;
                    }
                }
                // Синхронизированное уничтожение предмета
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
