using UnityEngine;
using Photon.Pun;

public class WeaponKickback : MonoBehaviour
{
    public Vector3 recoilOffset;
    public float recoilSpeed = 10f;
    public float rotationAmount = 2f; // ������������ ���� �������� ��� ������
    public float rotationSpeed = 10f; // �������� �������� ��������

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 currentOffset;
    private Quaternion currentRotation;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GameObject.FindGameObjectWithTag("Player").GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localRotation;
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            // ������� ����������� � �������� ������� � ��������
            currentOffset = Vector3.Lerp(currentOffset, Vector3.zero, recoilSpeed * Time.deltaTime);
            transform.localPosition = originalPosition + currentOffset;

            currentRotation = Quaternion.Lerp(currentRotation, Quaternion.identity, rotationSpeed * Time.deltaTime);
            transform.localRotation = originalRotation * currentRotation;
        }
    }

    public void ApplyRecoil(string weaponType)
    {
        // ������������� ��������� ������ � ����������� �� ���� ������
        if (weaponType == "Pistol")
        {
            recoilOffset = new Vector3(0, 0.1f, -0.1f);
        }
        else if (weaponType == "AK47")
        {
            recoilOffset = new Vector3(0, 0.2f, -0.2f);
        }

        currentOffset += recoilOffset;

        // ��������� �������
        float randomX = Random.Range(-rotationAmount, rotationAmount);
        float randomY = Random.Range(-rotationAmount, rotationAmount);
        currentRotation *= Quaternion.Euler(randomX, randomY, 0);
    }
}
