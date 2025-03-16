using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageble
{
    // Переменные для перемещения, камеры
    [SerializeField] public GameObject playerCamera;
    [SerializeField] private float walkSpeed, sprintSpeed, mouseSensitivity, jumpForce, smoothTime;
    private float verticalLookRotation;
    [SerializeField] private bool isGround = true;
    private Vector3 moveAmount; // Общая скорость, включая вертикальную составляющую
    public PhotonView pnView;

    // Используем CharacterController вместо Rigidbody
    private CharacterController characterController;

    // Массив для оружия
    [SerializeField] Item[] items;
    private int itemIndex;
    private int prevItemIndex = -1;

    // Переменные для хп
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    // Хп-бар сверху игрока
    [SerializeField] Slider hpBar;

    private PlayerManager playerManager;
    private bool isCursorLocked = true;
    [SerializeField] GameObject FirstWeapon;

    [SerializeField] public Text ammoText;
    [SerializeField] public Text hpBarPlayer;

    private float reloadStartTime = 0f;
    private bool isReloading = false;
    private Item currentItem;

    [SerializeField] private GameObject ShieldObj;
    SoundGodScript soundGod;
    LeaderBoardManager LeaderBoardManager;
    LeaderboardUI LeaderboardUI;

    [SerializeField] private float bobbingSpeed = 7f;      // Скорость пошатывания
    [SerializeField] private float bobbingAmountY = 0.05f;   // Амплитуда пошатывания вверх-вниз
    [SerializeField] private float bobbingAmountX = 0.03f;   // Амплитуда пошатывания влево-вправо
    [SerializeField] private float sprintMultiplier = 1.8f;  // Ускорение при спринте
    private float defaultCamPosY;
    private float defaultCamPosX;
    private float bobbingTimer = 0f;
    private bool isSprinting => Input.GetKey(KeyCode.LeftShift) && (new Vector3(moveAmount.x, 0, moveAmount.z).magnitude > 0.1f);

    // Новые переменные для эффектов выстрела
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject[] impactEffectPrefabs;
    [SerializeField] private GameObject bulletHolePrefab;
    [SerializeField] private float range = 100f;
    private Transform activeMuzzle;

    [SerializeField] private float autoFireRate = 6f; // количество выстрелов в секунду
    private float nextFireTime = 0f;

    // Параметры прыжка и гравитации
    private float verticalVelocity;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float bobbingSmoothSpeed = 5f;


    [SerializeField] private float landingShakeDuration = 0.3f; // базовая длительность тряски при приземлении
    [SerializeField] private float maxLandingShakeMagnitude = 0.3f; // максимальное смещение камеры при длинном полете
    private float landingShakeTimer = 0f;
    private float landingShakeMagnitude = 0f;
    private float airTime = 0f; // сколько времени игрок находится в воздухе
    private bool wasGrounded = true; // для отслеживания перехода из состояния "на земле"

    private void Awake()
    {
        soundGod = FindObjectOfType<SoundGodScript>();
        pnView = GetComponent<PhotonView>();
        // Получаем CharacterController (убедитесь, что он добавлен к объекту)
        characterController = GetComponent<CharacterController>();

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
            // Отключаем управление для чужих игроков
            if (ammoText != null) ammoText.gameObject.SetActive(false);
            if (hpBarPlayer != null) hpBarPlayer.gameObject.SetActive(false);
        }
        else
        {
            playerCamera.SetActive(true);
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
        }
        else if (currentItem.weaponType == "AK47")
        {
            GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoReload"));
        }

        reloadStartTime = Time.time;
    }

    private void Update()
    {
        if (!pnView.IsMine)
            return;

        Look();
        Movement(); // Обрабатываем движение и прыжок в одном методе
        SelectWeapon();

        if (!isReloading)
        {
            UseItem();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isCursorLocked = !isCursorLocked;
            if (isCursorLocked)
                LockCursor();
            else
                UnlockCursor();
        }

        hpBar.value = currentHealth;
        hpBarPlayer.text = "HP " + currentHealth.ToString() + "/ 100";

        ShieldObj.SetActive(shieldActive);

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

        if (!characterController.isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (!wasGrounded)
            {
                // Игрок только что приземлился:
                // Чем дольше в воздухе, тем сильнее эффект (с ограничением)
                landingShakeMagnitude = Mathf.Lerp(0f, maxLandingShakeMagnitude, Mathf.Clamp01(airTime / 2f));
                landingShakeTimer = landingShakeDuration;
                airTime = 0f;
            }
        }
        wasGrounded = characterController.isGrounded;
    }

    // Объединяем логику горизонтального движения и прыжка
    private void Movement()
    {
        // Получаем горизонтальное направление ввода
        Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        Vector3 horizontalMove = transform.TransformDirection(inputDir) * targetSpeed;

        // Обработка прыжка и гравитации:
        if (characterController.isGrounded)
        {
            // Если нажата клавиша прыжка, устанавливаем вертикальную скорость
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpForce;
            }
            else
            {
                // Если на земле, устанавливаем небольшое отрицательное значение для "прилипания"
                verticalVelocity = -1f;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Объединяем горизонтальную и вертикальную составляющие
        moveAmount = horizontalMove;
        moveAmount.y = verticalVelocity;

        characterController.Move(moveAmount * Time.deltaTime);
    }

    private void UseItem()
    {
        Item currentItem = items[itemIndex];
        if (isReloading || currentItem.isReloading) return;

        // Для пистолета – одиночный выстрел
        if (currentItem.weaponType == "Pistol")
        {
            if (Input.GetMouseButtonDown(0))
            {
                ExecuteShot(currentItem);
                LeaderBoardManager.Instance.photonView.RPC("RPC_AddScore", RpcTarget.MasterClient, PhotonNetwork.NickName, 1);
            }
        }
        // Для автомата – очередной выстрел при удержании кнопки
        else if (currentItem.weaponType == "AK47")
        {
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + (1f / autoFireRate);
                ExecuteShot(currentItem);
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && currentItem.currentAmmo < currentItem.maxAmmo && currentItem.reserveAmmo > 0)
        {
            StartReload(currentItem);
        }
    }

    private void ExecuteShot(Item currentItem)
    {
        if (currentItem.currentAmmo > 0)
        {
            items[itemIndex].Use();
            currentItem.currentAmmo--;
            string weaponType = currentItem.weaponType;
            GetComponent<RecoilController>().ApplyRecoil(weaponType);

            if (weaponType == "Pistol")
            {
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "PistolShoot"));
            }
            else if (weaponType == "AK47")
            {
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoShoot"));
            }

            UpdateAmmoUI();
            GetComponentInChildren<WeaponKickback>().ApplyRecoil(weaponType);

            // Луч из текущей позиции и направления камеры
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                if (activeMuzzle != null)
                {
                    pnView.RPC("RPC_SpawnMuzzleFlash", RpcTarget.All, activeMuzzle.position, activeMuzzle.rotation);
                }
                pnView.RPC("RPC_SpawnImpactEffect", RpcTarget.All, hit.point, hit.normal);
            }
        }
        else
        {
            if (currentItem.weaponType == "Pistol")
            {
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "PistolEmpty"));
            }
            else if (currentItem.weaponType == "AK47")
            {
                GetComponentInChildren<UniversalSoundPlayer>().PlaySound(true, soundGod.GetSound("PlayerWeapon", "AutoEmpty"));
            }
        }
    }

    [PunRPC]
    void RPC_SpawnMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        GameObject flash = Instantiate(muzzleFlashPrefab, position, rotation);
        if (activeMuzzle != null)
        {
            flash.transform.SetParent(activeMuzzle);
            flash.transform.localPosition = Vector3.zero;
            flash.transform.localRotation = Quaternion.identity;
        }
        Destroy(flash, 0.5f);
    }

    [PunRPC]
    void RPC_SpawnImpactEffect(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (impactEffectPrefabs != null && impactEffectPrefabs.Length > 0)
        {
            int randomImpactIndex = Random.Range(0, impactEffectPrefabs.Length);
            Instantiate(impactEffectPrefabs[randomImpactIndex], hitPoint, Quaternion.LookRotation(hitNormal));
        }
        if (bulletHolePrefab != null)
        {
            GameObject bulletHole = Instantiate(bulletHolePrefab,
                                                  hitPoint + hitNormal * 0.01f,
                                                  Quaternion.LookRotation(hitNormal));
            Destroy(bulletHole, 10f);
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
            LeaderBoardManager.Instance.photonView.RPC("RPC_AddDeath", RpcTarget.MasterClient, PhotonNetwork.NickName);
            LeaderBoardManager.Instance.photonView.RPC("RPC_AddKill", RpcTarget.MasterClient, attacker);
            playerManager.Die();
        }

        pnView.RPC("RPC_UpdateHealth", RpcTarget.All, currentHealth);
    }

    [PunRPC]
    void RPC_UpdateHealth(float health)
    {
        if (!pnView.IsMine)
        {
            hpBar.value = health;
            hpBarPlayer.text = "HP " + currentHealth.ToString() + "/ 100";
        }
    }

    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 90f);
        playerCamera.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void SelectWeapon()
    {
        for (int i = 0; i < items.Length; i++)
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
        activeMuzzle = items[itemIndex].itemGameObject.transform.Find("Muzzle");
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
        if (!pnView.IsMine)
        {
            EquipItem(index);
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!pnView.IsMine && targetPlayer == pnView.Owner)
        {
            EquipItem((int)changedProps["index"]);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        Debug.Log($"Health: {currentHealth}");
    }

    public bool shieldActive = false;

    public void ActivateShield()
    {
        if (!shieldActive)
        {
            shieldActive = true;
            Debug.Log("Shield Activated");
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
    }

    [PunRPC]
    public void RPC_AddAmmo(int amount)
    {
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
        // Рассчитываем базовый эффект боббинга (используем только горизонтальное движение)
        Vector3 horizontalMove = new Vector3(moveAmount.x, 0, moveAmount.z);
        Vector3 targetPos;
        if (horizontalMove.magnitude > 0.1f && characterController.isGrounded)
        {
            float speed = isSprinting ? bobbingSpeed * sprintMultiplier : bobbingSpeed;
            float amountY = isSprinting ? bobbingAmountY * sprintMultiplier : bobbingAmountY;
            float amountX = isSprinting ? bobbingAmountX * sprintMultiplier : bobbingAmountX;

            bobbingTimer += Time.deltaTime * speed;
            float bobY = defaultCamPosY + Mathf.Sin(bobbingTimer) * amountY;
            float bobX = defaultCamPosX + Mathf.Cos(bobbingTimer * 0.5f) * amountX;
            targetPos = new Vector3(bobX, bobY, playerCamera.transform.localPosition.z);
        }
        else
        {
            bobbingTimer = 0f;
            targetPos = new Vector3(defaultCamPosX, defaultCamPosY, playerCamera.transform.localPosition.z);
        }

        // Если есть эффект приземления, добавляем случайное смещение
        if (landingShakeTimer > 0)
        {
            Vector3 landingShakeOffset = new Vector3(
                Random.Range(-landingShakeMagnitude, landingShakeMagnitude),
                Random.Range(-landingShakeMagnitude, landingShakeMagnitude),
                0);
            targetPos += landingShakeOffset;
            landingShakeTimer -= Time.deltaTime;
        }

        // Плавно интерполируем положение камеры к целевому значению
        float smoothX = Mathf.Lerp(playerCamera.transform.localPosition.x, targetPos.x, Time.deltaTime * 5f);
        float smoothY = Mathf.Lerp(playerCamera.transform.localPosition.y, targetPos.y, Time.deltaTime * 5f);
        playerCamera.transform.localPosition = new Vector3(smoothX, smoothY, targetPos.z);
    }
}
