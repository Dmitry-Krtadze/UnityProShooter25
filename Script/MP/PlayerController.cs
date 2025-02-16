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
    [SerializeField] private Slider hpBarPlayer;
    private PlayerManager playerManager;
    private bool isCursorLocked = true;
    [SerializeField]  GameObject FirstWeapon;




    [SerializeField] private Text ammoText;



    public AudioClip[,] weaponSounds = new AudioClip[2, 3]; // [Оружие, Тип звука]


    private void Awake()
    {
        pnView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        
        currentHealth = maxHealth;
        hpBar.maxValue = maxHealth;
        hpBar.value = currentHealth;

        playerManager = PhotonView.Find((int)pnView.InstantiationData[0])
            .GetComponent<PlayerManager>();

        ammoText = GetComponentInChildren<Text>();
        hpBarPlayer = GameObject.FindGameObjectWithTag("hpBarPlayer").GetComponent<Slider>();


        // Загрузка звуков для оружий
        weaponSounds[0, 0] = Resources.Load<AudioClip>("Sounds/PistolShoot");
        weaponSounds[0, 1] = Resources.Load<AudioClip>("Sounds/PistolEmpty");
        weaponSounds[0, 2] = Resources.Load<AudioClip>("Sounds/PistolReload");

        weaponSounds[1, 0] = Resources.Load<AudioClip>("Sounds/AutoShoot");
        weaponSounds[1, 1] = Resources.Load<AudioClip>("Sounds/AutoEmpty");
        weaponSounds[1, 2] = Resources.Load<AudioClip>("Sounds/AutoReload");
    }



    private void Start()
    {
        if (!pnView.IsMine)
        {
            Destroy(playerCamera);
            Destroy(rb);
            Destroy(ammoText);
        }
        else
        {
            EquipItem(0);
            UpdateAmmoUI();
        }

        LockCursor();
 

    }
    private IEnumerator Reload(Item currentItem)
    {

        currentItem.isReloading = true;

        GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, weaponSounds[itemIndex, 2]);

        yield return new WaitForSeconds(currentItem.reloadTime);
        
        int ammoNeeded = currentItem.maxAmmo - currentItem.currentAmmo;
        int ammoToLoad = Mathf.Min(ammoNeeded, currentItem.reserveAmmo);

        currentItem.currentAmmo += ammoToLoad;
        currentItem.reserveAmmo -= ammoToLoad;

        
        currentItem.isReloading = false;
        UpdateAmmoUI();
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
        hpBar.value = currentHealth;
        hpBarPlayer.value = currentHealth;
    }
    private void UseItem()
    {
        Item currentItem = items[itemIndex];
        if (currentItem.isReloading) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentItem.currentAmmo > 0)
            {
                items[itemIndex].Use();
                currentItem.currentAmmo--;
                currentItem = items[itemIndex]; // Обновление состояния
                string weaponType = currentItem.weaponType;

                GetComponent<RecoilController>().ApplyRecoil(weaponType);

                // Передаём имя звука для выстрела
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, weaponSounds[itemIndex, 0]);

                UpdateAmmoUI();
            }
            else
            {
                // Передаём имя звука для пустого магазина
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, weaponSounds[itemIndex, 1]);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentItem.currentAmmo < currentItem.maxAmmo && currentItem.reserveAmmo > 0)
        {
            StartCoroutine(Reload(currentItem));
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
        
        if (currentHealth <= 0)
        {
            playerManager.Die();
        }

        pnView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);
    }

    [PunRPC]
    void RPC_UpdateHealth(float health)
    {
        if (!pnView.IsMine) // Обновляем здоровье на других клиентах
        {
            hpBar.value = health;
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

        if (prevItemIndex != -1)
        {
            items[prevItemIndex].itemGameObject.SetActive(false);
        }
        prevItemIndex = itemIndex;

 
        if (pnView.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("index", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            UpdateAmmoUI();
        }
        pnView.RPC("RPC_SyncWeapon", RpcTarget.All, itemIndex);
    }





    [PunRPC]
    void RPC_SyncWeapon(int index)
    {
        if (!pnView.IsMine) // Для всех клиентов, кроме владельца
        {
            EquipItem(index); // Экипируем оружие, которое указано на других клиентах
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


    private void UpdateAmmoUI()
    {
        if (!pnView.IsMine) return;
        Item currentItem = items[itemIndex];
        ammoText.text = currentItem.currentAmmo + " / " + currentItem.reserveAmmo;
    }

}
