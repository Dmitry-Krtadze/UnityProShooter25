using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RecoilController : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform; // ������ ������
    [SerializeField] private Transform weapon;          // ������ ��� ������-����������� ������
    [SerializeField] private PlayerController pc;       // ������ �� PlayerController

    [Header("Recoil Settings")]
    [SerializeField] private float returnSpeed = 5f;    // �������� �������� ������ �� ������
    [SerializeField] private float maxRecoil = 15f;     // ����������� ������

    [Header("ADS Settings")]
    [SerializeField] private Vector3 aimPositionOffset = new Vector3(0.1f, -0.1f, 0.2f); // �������� ������ ��� ������������
    [SerializeField] private float aimSpeed = 10f;      // �������� �������� ��������

    // ������� ����������� ������ (�� ������������ � �� ������)
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;

    private PhotonView photonView;

    // ������� ������ ��� ������������
    private Vector3 defaultWeaponPosition;

    // ������ ���������� ��� ���������/���������� ������������
    private bool isAiming = false;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // ���������� �������� ������� ������ (������ � ���������� ������)
        if (photonView.IsMine && weapon != null)
        {
            defaultWeaponPosition = weapon.localPosition;
        }
    }

    private void Update()
    {
        // ��������� ������ ������ � ���������� ������
        if (!photonView.IsMine) return;

        // ����������� ������ � PlayerController (���� �� �����������)
        if (pc == null)
        {
            pc = GetComponent<PlayerController>();
        }
        if (cameraTransform == null && pc != null)
        {
            cameraTransform = pc.playerCamera.transform;
        }

        // 1) ������������ ������������ �� ������� ��� (�� �������)
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = !isAiming;
        }

        // 2) ������ ���������� ������ � �������� ��������� �� ������
        //    currentRecoil ��������� � Vector3.zero
        currentRecoil = Vector3.Lerp(currentRecoil, Vector3.zero, returnSpeed * Time.deltaTime);

        // 3) ��������� ������ � ������ (�������� currentRecoil �� ��������� �����)
        //    �����: ������� PlayerController ������ ������ �� ����, � �� ������ ������
        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles -= currentRecoil;
        }

        // 4) ������������ ADS � ������� ������
        HandleAiming();
    }

    // ����� ��� ���������� ������ ��� ��������
    public void ApplyRecoil(string weaponType)
    {
        // ���� ������ �������� ���, ������ ���
        if (weaponType == "Melee") return;

        float verticalRecoil = 0f;
        float horizontalRecoil = 0f;

        if (weaponType == "Pistol")
        {
            verticalRecoil = Random.Range(3f, 5f);      // ��������� ������ �����
            horizontalRecoil = Random.Range(-0.5f, 0.5f);
        }
        else if (weaponType == "AK47")
        {
            verticalRecoil = Random.Range(10f, 20f);    // ������� ������ �����
            horizontalRecoil = Random.Range(-1f, 1f);
        }

        // ���� �������������, �������� ������ (������: � 2 ����)
        if (isAiming)
        {
            verticalRecoil *= 0.5f;
            horizontalRecoil *= 0.5f;
        }

        // ������������ ������ �� ���������
        verticalRecoil = Mathf.Clamp(verticalRecoil, 0f, maxRecoil);

        // ������������� ������� ������
        targetRecoil = new Vector3(verticalRecoil, horizontalRecoil, 0f);
        // ��������� ����������
        currentRecoil = targetRecoil;
    }

    private void HandleAiming()
    {
        if (weapon == null) return;

        // ������� ������� � ��������, ���� �������������, ����� ��������
        Vector3 targetPos = isAiming
            ? defaultWeaponPosition + aimPositionOffset
            : defaultWeaponPosition;

        // ������� ����������� ������ � ������� �������
        weapon.localPosition = Vector3.Lerp(
            weapon.localPosition,
            targetPos,
            Time.deltaTime * aimSpeed
        );
    }
}
