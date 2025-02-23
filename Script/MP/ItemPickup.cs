using Photon.Pun;
using UnityEngine;

public class ItemPickup : MonoBehaviourPunCallbacks
{
    public enum ItemType { HealthKit, AmmoKit, Shield }
    public ItemType itemType;
    public int value; // �������� (���������� ��������, �������� � �.�.)

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ��������� ������� ������ �� ������-�������
            if (PhotonNetwork.IsMasterClient)
            {
                // ���������� ��������� �������� ��� �������
                value = Random.Range(20, 50);

                PhotonView playerPhotonView = other.GetComponent<PhotonView>();
                if (playerPhotonView != null)
                {
                    switch (itemType)
                    {
                        case ItemType.HealthKit:
                            // �������� RPC � ������ ��� ���������� ��������
                            playerPhotonView.RPC("RPC_AddHealth", RpcTarget.All, value);
                            break;
                        case ItemType.AmmoKit:
                            // �������� RPC � ������ ��� ���������� ��������
                            playerPhotonView.RPC("RPC_AddAmmo", RpcTarget.All, value);
                            break;
                        case ItemType.Shield:
                            // �������� RPC � ������ ��� ��������� ����
                            playerPhotonView.RPC("RPC_ActivateShield", RpcTarget.All);
                            break;
                    }
                }
                // ������������������ ����������� ��������
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
