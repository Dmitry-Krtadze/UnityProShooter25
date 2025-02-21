using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks, IDamageble
{

    

    //Переменные для перемещения, камеры
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

    //Масив для оружия
    [SerializeField] Item[] items;
    private int itemIndex;
    private int prevItemIndex = -1;

    //Переменные для хп
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    //хпБар сверху игрока
    [SerializeField] Slider hpBar;
    [SerializeField] private Slider hpBarPlayer;


    private PlayerManager playerManager;
    private bool isCursorLocked = true;
    [SerializeField]  GameObject FirstWeapon;




    [SerializeField] private Text ammoText;




    [SerializeField] private GameObject ShieldObj;


    SoundGodScript soundGod;



    private void Awake()
    {
        soundGod = FindObjectOfType<SoundGodScript>();  

        pnView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        
        currentHealth = maxHealth;
        hpBar.maxValue = maxHealth;
        hpBar.value = currentHealth;

        playerManager = PhotonView.Find((int)pnView.InstantiationData[0])
            .GetComponent<PlayerManager>();

        ammoText = GetComponentInChildren<Text>();
        hpBarPlayer = GameObject.FindGameObjectWithTag("hpBarPlayer").GetComponent<Slider>();


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




    private float reloadStartTime = 0f;
    private bool isReloading = false;
    private Item currentItem;

    private void StartReload(Item item)
    {
        if (isReloading)
            return;

        currentItem = item;
        isReloading = true;

        if (currentItem.weaponType == "Pistol")
        {
            GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "PistolReload"));
        } else if (currentItem.weaponType == "AK47")
        {
            GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoReload"));
        }
        

        reloadStartTime = Time.time;
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
        if (!isReloading) { 
            UseItem();
        }
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

        if (shieldActive)
        {
            ShieldObj.SetActive(true);
        }
        else
        {
            ShieldObj.SetActive(false);
        }
        if (isReloading)
        {
            if (Time.time >= reloadStartTime + currentItem.reloadTime)
            {
                int ammoNeeded = currentItem.maxAmmo - currentItem.currentAmmo;
                int ammoToLoad = Mathf.Min(ammoNeeded, currentItem.reserveAmmo);

                currentItem.currentAmmo += ammoToLoad;
                currentItem.reserveAmmo -= ammoToLoad;

                isReloading = false;

                UpdateAmmoUI();
            }
        }
    }
    private void UseItem()
    {
        Item currentItem = items[itemIndex];
        if (currentItem.isReloading) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentItem.currentAmmo > 0 )
            {
                items[itemIndex].Use();
                currentItem.currentAmmo--;
                currentItem = items[itemIndex]; // Обновление состояния
                string weaponType = currentItem.weaponType;

                GetComponent<RecoilController>().ApplyRecoil(weaponType);

                // Передаём имя звука для выстрела
                if (currentItem.weaponType == "Pistol")
                {
                    GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "PistolShoot"));
                }
                else if (currentItem.weaponType == "AK47")
                {
                    GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoShoot"));
                }

                UpdateAmmoUI();
                GetComponentInChildren<WeaponKickback>().ApplyRecoil(weaponType);
            }
            else
            {
                // Передаём имя звука для пустого магазина
                if (currentItem.weaponType == "Pistol")
                {
                    GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "PistolEmpty"));
                }
                else if (currentItem.weaponType == "AK47")
                {
                    GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoEmpty"));
                };
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentItem.currentAmmo < currentItem.maxAmmo && currentItem.reserveAmmo > 0)
        {
            StartReload(currentItem);
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
            hpBarPlayer.value = health;

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
        // Получаем направление движения
        Vector3 moveDir = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")).normalized;

        // Проверяем, есть ли активный ввод
        if (moveDir != Vector3.zero)
        {
            // Если есть ввод, рассчитываем скорость
            float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
            Vector3 targetVelocity = transform.TransformDirection(moveDir) * targetSpeed;

            // Плавное изменение скорости
            moveAmout = Vector3.Lerp(moveAmout, targetVelocity, smoothTime * Time.deltaTime);

            // Останавливаем корутину на случай, если снова пошёл ввод
            StopAllCoroutines();
        }
        else
        {
            // Если ввода нет, запускаем корутину на обнуление скорости
            if (moveAmout != Vector3.zero)
            {
                StartCoroutine(ResetSpeed());
            }
        }
    }

    private IEnumerator ResetSpeed()
    {
        yield return new WaitForSeconds(0.2f);
        moveAmout = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!pnView.IsMine) return;
        rb.MovePosition(rb.position + moveAmout * Time.fixedDeltaTime);
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





    public void AddHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100); // Максимум 100 здоровья
        Debug.Log($"Health: {currentHealth}");
    }

    public bool shieldActive = false;

    public void ActivateShield()
    {
        if (!shieldActive)
        {
            shieldActive = true;
            Debug.Log("Shield Activated");

            // Отключение щита через 10 секунд
            Invoke(nameof(DeactivateShield), 10f);
        }
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        Debug.Log("Shield Deactivated");
    }



















  
}
