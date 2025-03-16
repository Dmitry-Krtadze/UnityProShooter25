using Photon.Pun;
using UnityEngine;

public class ExampleNetworkedObject : MonoBehaviourPun
{
    private PhotonView _photonView;
    private Rigidbody _rb;
    [SerializeField] private float moveSpeed = 5f;

    void Start()
    {
        // ������������� �����������
        _photonView = GetComponent<PhotonView>();
        _rb = GetComponent<Rigidbody>();

        // �������� ��� ���������� ������
        if (_photonView != null && _photonView.IsMine)
        {
            // ������ ����� SetupLocalPlayer()
        }
    }

    void Update()
    {
        // �������� �� null ����� ��������������
        if (_photonView == null || !_photonView.IsMine) return;

        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed;
        _rb.velocity = movement;
    }

    [PunRPC]
    public void TakeDamageRPC(int damage)
    {
        // ������� ������ �����
        if (_photonView != null && _photonView.IsMine)
        {
            Health -= damage;
            if (Health <= 0)
            {
                DestroyNetworkObject();
            }
        }
    }

    void DestroyNetworkObject()
    {
        if (_photonView != null && _photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // ������� ������
        _photonView = null;
        _rb = null;

        // �������������� �������� ��� �����������
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.RemoveRPCs(_photonView);
        }
    }

    // ������ �������� � ���������
    private int _health = 100;
    public int Health
    {
        get => _health;
        set
        {
            if (_photonView != null && _photonView.IsMine)
            {
                _health = Mathf.Clamp(value, 0, 100);
            }
        }
    }
}