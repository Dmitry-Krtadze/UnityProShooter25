using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks, IDamageble
{

    

    //���������� ��� �����������, ������
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
    public PhotonView pnView;

    //����� ��� ������
    [SerializeField] Item[] items;
    private int itemIndex;
    private int prevItemIndex = -1;

    //���������� ��� ��
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    //����� ������ ������
    [SerializeField] Slider hpBar;
    


    private PlayerManager playerManager;
    private bool isCursorLocked = true;
    [SerializeField]  GameObject FirstWeapon;




    [SerializeField] public Text ammoText;
    [SerializeField] public Text hpBarPlayer;

    private float reloadStartTime = 0f;
    private bool isReloading = false;
    private Item currentItem;




    [SerializeField] private GameObject ShieldObj;


    SoundGodScript soundGod;

    LeaderBoardManager LeaderBoardManager;
    LeaderboardUI LeaderboardUI;



    [SerializeField] private float bobbingSpeed = 7f; // �������� �����������
    [SerializeField] private float bobbingAmountY = 0.05f; // ��������� ����������� �����-����
    [SerializeField] private float bobbingAmountX = 0.03f; // ��������� ����������� �����-������
    [SerializeField] private float sprintMultiplier = 1.8f; // ��������� ��� �������
    private float defaultCamPosY;
    private float defaultCamPosX;
    private float bobbingTimer = 0f;
    private bool isSprinting => Input.GetKey(KeyCode.LeftShift) && moveAmout.magnitude > 0.1f;




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



    }



    private void Start()
    {
      

        if (!pnView.IsMine)
        {
            Destroy(playerCamera);
            rb.isKinematic = true; // ��������� ������ ������ �������� Rigidbody

            // ������������ UI
            if (ammoText != null) ammoText.gameObject.SetActive(false);
            if (hpBarPlayer != null) hpBarPlayer.gameObject.SetActive(false);

        }
        else
        {
            playerCamera.SetActive(true);
            // ���������� UI ������ ��� ���������� ������
            if (ammoText != null)
            {
                ammoText.gameObject.SetActive(true);
                UpdateAmmoUI();
            }
            if (hpBarPlayer != null)
            {
                hpBarPlayer.gameObject.SetActive(true);
                hpBarPlayer.text = $"HP {currentHealth}/100";
            }
            if (playerCamera != null)
            {
                defaultCamPosY = playerCamera.transform.localPosition.y;
                defaultCamPosX = playerCamera.transform.localPosition.x;
            }
            EquipItem(0);
            
            UpdateAmmoUI();
        }

        LockCursor();
        LeaderBoardManager = GameObject.FindGameObjectWithTag("LeaderBoardManager").GetComponent<LeaderBoardManager>();



        LeaderBoardManager.Instance.photonView.RPC("RequestLeaderboardFromMaster", RpcTarget.MasterClient);
    }

    



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
            // ����������� ��������� �������
            isCursorLocked = !isCursorLocked;

            if (isCursorLocked)
                LockCursor();
            else
                UnlockCursor();
        }
        hpBar.value = currentHealth;

        hpBarPlayer.text = "HP " + currentHealth.ToString() + "/ 100";

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
                currentItem.isReloading = false;

                UpdateAmmoUI();
            }
        }
        
        CameraBobbing();
    }
    private void UseItem()
    {
        
        Item currentItem = items[itemIndex];
        if (isReloading) return;
        if (currentItem.isReloading) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentItem.currentAmmo > 0 )
            {
                if (!PhotonNetwork.IsMasterClient)
                {
                    LeaderBoardManager.Instance.photonView.RPC("RPC_AddScore", RpcTarget.MasterClient, PhotonNetwork.NickName, 1);
                }
                else
                {
                    LeaderBoardManager.Instance.RPC_AddScore(PhotonNetwork.NickName, 1);
                }




                items[itemIndex].Use();
                currentItem.currentAmmo--;
                currentItem = items[itemIndex]; // ���������� ���������
                string weaponType = currentItem.weaponType;

                GetComponent<RecoilController>().ApplyRecoil(weaponType);

                // ������� ��� ����� ��� ��������
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
                // ������� ��� ����� ��� ������� ��������
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


    [PunRPC]
    void RPC_AddScore(string playerName, int scoreToAdd)
    {
        LeaderBoardManager.PlayerStats currentStats = LeaderBoardManager.GetPlayerStats(playerName);
        int newScore = currentStats.score + scoreToAdd;
        LeaderBoardManager.UpdateLeaderboard(playerName, currentStats.kills, currentStats.deaths, newScore);
        LeaderBoardManager.SyncLeaderboard();
    }


    public void TakeDamage(float damage, string attacker)
    {
        pnView.RPC("RPC_Damage", RpcTarget.Others, damage, attacker);
    }

    [PunRPC]
    void RPC_Damage(float damage, string attacker)
    {
        if (!pnView.IsMine) return;

        currentHealth -= damage;
        hpBarPlayer.text = "HP " + currentHealth.ToString() + "/ 100";
        Debug.Log(attacker);

        if (currentHealth <= 0)
        {
            // ��������� ����������: ������ �������� ������, � ��������� � ��������
            LeaderBoardManager.Instance.photonView.RPC("RPC_AddDeath", RpcTarget.MasterClient, PhotonNetwork.NickName);
            LeaderBoardManager.Instance.photonView.RPC("RPC_AddKill", RpcTarget.MasterClient, attacker);

            playerManager.Die();
        }

        pnView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);
    }

    [PunRPC]
    void RPC_UpdateHealth(float health)
    {
        if (!pnView.IsMine) // ��������� �������� �� ������ ��������
        {
            hpBar.value = health;
            hpBarPlayer.text = "HP " + currentHealth.ToString() + "/ 100";

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
        // �������� ����������� ��������
        Vector3 moveDir = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0,
            Input.GetAxisRaw("Vertical")).normalized;

        // ���������, ���� �� �������� ����
        if (moveDir != Vector3.zero)
        {
            // ���� ���� ����, ������������ ��������
            float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
            Vector3 targetVelocity = transform.TransformDirection(moveDir) * targetSpeed;

            // ������� ��������� ��������
            moveAmout = Vector3.Lerp(moveAmout, targetVelocity, smoothTime * Time.deltaTime);

            // ������������� �������� �� ������, ���� ����� ����� ����
            StopCoroutine(ResetSpeed());
        }
        else
        {
            // ���� ����� ���, ��������� �������� �� ��������� ��������
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
        if (!pnView.IsMine) // ��� ���� ��������, ����� ���������
        {
            EquipItem(index); // ��������� ������, ������� ������� �� ������ ��������
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
        Cursor.lockState = CursorLockMode.Locked; // ��������� ������ � ������
        Cursor.visible = false;                  // �������� ������
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;  // ������� ���������� �������
        Cursor.visible = true;                   // ������ ������ �������
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
        currentHealth = Mathf.Clamp(currentHealth, 0, 100); // �������� 100 ��������
        Debug.Log($"Health: {currentHealth}");
    }

    public bool shieldActive = false;

    public void ActivateShield()
    {
        if (!shieldActive)
        {
            shieldActive = true;
            Debug.Log("Shield Activated");

            // ���������� ���� ����� 10 ������
            Invoke(nameof(DeactivateShield), 10f);
        }
    }

    private void DeactivateShield()
    {
        shieldActive = false;
        Debug.Log("Shield Deactivated");
    }
    [PunRPC]
    public void RPC_AddHealth(int amount)
    {
        AddHealth(amount);
        // ���������� UI, ���� ���������
    }

    [PunRPC]
    public void RPC_AddAmmo(int amount)
    {
        // ��������������, ��� ������� ������������� ������� � ������
        Item currentWeapon = items[itemIndex];
        currentWeapon.AddAmmo(amount);
        UpdateAmmoUI();
    }

    [PunRPC]
    public void RPC_ActivateShield()
    {
        ActivateShield();
    }



    private void CameraBobbing()
    {
        if (moveAmout.magnitude > 0.1f && isGround)
        {
            float speed = isSprinting ? bobbingSpeed * sprintMultiplier : bobbingSpeed;
            float amountY = isSprinting ? bobbingAmountY * sprintMultiplier : bobbingAmountY;
            float amountX = isSprinting ? bobbingAmountX * sprintMultiplier : bobbingAmountX;

            bobbingTimer += Time.deltaTime * speed;
            float newY = defaultCamPosY + Mathf.Sin(bobbingTimer) * amountY;
            float newX = defaultCamPosX + Mathf.Cos(bobbingTimer * 0.5f) * amountX;

            playerCamera.transform.localPosition = new Vector3(newX, newY, playerCamera.transform.localPosition.z);
        }
        else
        {
            bobbingTimer = 0f;
            playerCamera.transform.localPosition = new Vector3(defaultCamPosX, defaultCamPosY, playerCamera.transform.localPosition.z);
        }
    }




}
