using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RecoilController : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // Камера игрока
    [SerializeField] private Transform weapon;          // Оружие или объект-«держатель» оружия
    [SerializeField] private PlayerController pc;       // Ссылка на PlayerController

    [Header("Recoil Settings")]
    [SerializeField] private float returnSpeed = 5f;    // Скорость возврата камеры от отдачи
    [SerializeField] private float maxRecoil = 15f;     // Ограничение отдачи

    [Header("ADS Settings")]
    [SerializeField] private Vector3 aimPositionOffset = new Vector3(0.1f, -0.1f, 0.2f); // Смещение оружия при прицеливании
    [SerializeField] private float aimSpeed = 10f;      // Скорость плавного перехода

    // Текущая накопленная отдача (мы «накладываем» её на камеру)
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    private PhotonView photonView;

    // Позиция оружия без прицеливания
    private Vector3 defaultWeaponPosition;

    // Булева переменная для включения/выключения прицеливания
    private bool isAiming = false;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Запоминаем исходную позицию оружия (только у локального игрока)
        if (photonView.IsMine && weapon != null)
        {
            defaultWeaponPosition = weapon.localPosition;
        }
    }

    private void Update()
    {
        // Обработку делаем только у локального игрока
        if (!photonView.IsMine) return;

        // Подтягиваем камеру и PlayerController (если не установлены)
        if (pc == null)
        {
            pc = GetComponent<PlayerController>();
        }
        if (cameraTransform == null && pc != null)
        {
            cameraTransform = pc.playerCamera.transform;
        }

        // 1) Переключение прицеливания по нажатию ПКМ (не зажатию)
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = !isAiming;
        }

        // 2) Плавно возвращаем камеру в исходное положение от отдачи
        //    currentRecoil стремится к Vector3.zero
        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, returnSpeed * Time.deltaTime);

        // 3) Применяем отдачу к камере (вычитаем currentRecoil из локальных углов)
        //    Важно: сначала PlayerController крутит камеру по мыши, а мы вносим правки
        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles -= currentRecoil;
        }

        // 4) Обрабатываем ADS — смещаем оружие
        HandleAiming();
    }

    // Метод для применения отдачи при выстреле
    public void ApplyRecoil(string weaponType)
    {
        // Если оружие ближнего боя, отдачи нет
        if (weaponType == "Melee") return;

        float verticalRecoil = 0f;
        float horizontalRecoil = 0f;

        if (weaponType == "Pistol")
        {
            verticalRecoil = Random.Range(3f, 5f);      // Маленькая отдача вверх
            horizontalRecoil = Random.Range(-0.5f, 0.5f);
        }
        else if (weaponType == "AK47")
        {
            verticalRecoil = Random.Range(10f, 20f);    // Сильная отдача вверх
            horizontalRecoil = Random.Range(-1f, 1f);
        }

        // Если прицеливаемся, уменьшим отдачу (пример: в 2 раза)
        if (isAiming)
        {
            verticalRecoil *= 0.5f;
            horizontalRecoil *= 0.5f;
        }

        // Ограничиваем отдачу по вертикали
        verticalRecoil = Mathf.Clamp(verticalRecoil, 0f, maxRecoil);

        // Устанавливаем целевую отдачу
        targetRecoil = new Vector3(verticalRecoil, horizontalRecoil, 0f);
        // Применяем немедленно
        currentRecoil = targetRecoil;
    }

    private void HandleAiming()
    {
        if (weapon == null) return;

        // Целевая позиция — смещение, если прицеливаемся, иначе исходная
        Vector3 targetPos = isAiming
            ? defaultWeaponPosition + aimPositionOffset
            : defaultWeaponPosition;

        // Плавное перемещение оружия к целевой позиции
        weapon.localPosition = Vector3.Lerp(
            weapon.localPosition,
            targetPos,
            Time.deltaTime * aimSpeed
        );
    }
}
