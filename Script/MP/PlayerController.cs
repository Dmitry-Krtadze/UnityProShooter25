using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks, IDamageble
{

    


    [SerializeField] public GameObject playerCamera;
    [SerializeField]
    private float
        walkSpeed, sprintSpeed, mouseSensitivity,
        jumpForce, smoothTime;
    private float vertacalLookRotation;
    [SerializeField]  private bool isGround;
    private Vector3 smothMove;
    private Vector3 moveAmout;
    private Rigidbody rb;
    private PhotonView pnView;

    [SerializeField] Item[] items;
    private int itemIndex;
    private int prevItemIndex = -1;


    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] Slider hpBar;
    private PlayerManager playerManager;
    private bool isCursorLocked = true;
    [SerializeField]  GameObject FirstWeapon;
    private void Awake()
    {
        pnView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();

        currentHealth = maxHealth;
        hpBar.value = currentHealth;
        playerManager = PhotonView.Find((int)pnView.InstantiationData[0]).
            GetComponent<PlayerManager>();
        

    }
    private void Start()
    {
        if (!pnView.IsMine)
        {
            Destroy(playerCamera);
            Destroy(rb);
        }
        else
        {
            EquipItem(0);
        }

        LockCursor();
    }
    private void Update()
    {
        if (!pnView.IsMine)
        {
            return;
        }
        Look();
        Movement();
        Jump();
        SelectWeapon();
        UseItem();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Переключаем состояние курсора
            isCursorLocked = !isCursorLocked;

            if (isCursorLocked)
                LockCursor();
            else
                UnlockCursor();
        }

    }
    private void UseItem()
    {
        if (Input.GetMouseButtonDown(0))
        {
            items[itemIndex].Use();
            string weaponType = items[itemIndex].weaponType;
            GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true);

            
            GetComponent<RecoilController>().ApplyRecoil(weaponType);
        }
    }
    public void TakeDamage(float damage)
    {
        pnView.RPC("RPC_Damage", RpcTarget.All, damage);
    }

    [PunRPC]
    void RPC_Damage(float damage)
    {
        if (!pnView.IsMine) return;
        currentHealth -= damage;
        hpBar.value = currentHealth;
        if (currentHealth <= 0)
        {
            playerManager.Die();
        }
    }
    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") *
            mouseSensitivity);
        vertacalLookRotation += Input.GetAxisRaw("Mouse Y") *
            mouseSensitivity;
        vertacalLookRotation = Mathf.Clamp(vertacalLookRotation,
            -80f, 90f);
        playerCamera.transform.localEulerAngles = Vector3.left *
            vertacalLookRotation;
    }
    private void Movement()
    {
        Vector3 moveDir = new Vector3(
            Input.GetAxisRaw("Horizontal"), 0,
            Input.GetAxisRaw("Vertical")).normalized;
        moveAmout = Vector3.SmoothDamp(moveAmout, moveDir *
            (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed :
            walkSpeed), ref smothMove, smoothTime);
    }
    private void FixedUpdate()
    {
        if (!pnView.IsMine)
        {
            return;
        }
        rb.MovePosition(rb.position + transform.TransformDirection(
            moveAmout) * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jump");
        }
        
    }

    public void GroundState(bool iSGround)
    {
        this.isGround = iSGround;
    }
    private void SelectWeapon()
    {
        for(int i = 0;i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
    }
    private void EquipItem(int index)
    {
        if (index == prevItemIndex) return;
        itemIndex = index;
        items[itemIndex].itemGameObject.SetActive(true);

        if(prevItemIndex != -1)
        {
            items[prevItemIndex].itemGameObject.SetActive(false);
        }
        prevItemIndex = itemIndex;

        if (pnView.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("index", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)

    {
        if(!pnView.IsMine && targetPlayer == pnView.Owner)
        {
            EquipItem((int)changedProps["index"]);
        }
    }
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // Фиксируем курсор в центре
        Cursor.visible = false;                  // Скрываем курсор
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;  // Снимаем блокировку курсора
        Cursor.visible = true;                   // Делаем курсор видимым
    }
}
