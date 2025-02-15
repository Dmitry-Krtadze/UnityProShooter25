using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class RecoilController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform cameraTransform; // Камера игрока
    [SerializeField] private Transform weapon;
    [SerializeField] public PlayerController pc;
    [SerializeField] private float returnSpeed = 5f;    // Скорость возврата камеры
    [SerializeField] private float maxRecoil = 15f;    // Ограничение отдачи

    private Vector3 currentRecoil; // Текущая накопленная отдача
    private Vector3 targetRecoil;  // Желаемая отдача
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        weapon = GetComponent<Transform>();
    }
    private void Update()
    {
       
        if (photonView.IsMine)
        {
            pc = GetComponent<PlayerController>();
            cameraTransform = pc.playerCamera.GetComponent<Transform>();
            // Плавно возвращаем камеру в исходное положение
            currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
            weapon.rotation = Quaternion.Lerp(currentRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
            // Применяем отдачу только по вертикали и горизонтали
            cameraTransform.localEulerAngles -= currentRecoil;
        }
    }

    public void ApplyRecoil(string weaponType)
    {
        if (weaponType == "Melee") return; // Оружие ближнего боя не имеет отдачи

        float verticalRecoil = 0f;
        float horizontalRecoil = 0f;

        if (weaponType == "Pistol")
        {
            verticalRecoil = Random.Range(3f, 5f);   // Маленькая отдача вверх
            horizontalRecoil = Random.Range(-0.5f, 0.5f); // Легкий дрейф в стороны
        }
        else if (weaponType == "AK47")
        {
            verticalRecoil = Random.Range(10f, 20f);   // Сильная отдача вверх
            horizontalRecoil = Random.Range(-1f, 1f); // Более резкие отклонения
        }

        // Ограничиваем отдачу по вертикали
        verticalRecoil = Mathf.Clamp(verticalRecoil, 0f, maxRecoil);

        // Применяем полученную отдачу
        targetRecoil = new Vector3(verticalRecoil, horizontalRecoil, 0f);
        currentRecoil = targetRecoil;  // Немедленно применяем отдачу
    }
}
