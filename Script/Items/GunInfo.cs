using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Add Gun")]
public class GunInfo : ItemInfo
{

    public float Damage;


    [SerializeField] public int maxAmmo;
    [SerializeField] public int currentAmmo;
    [SerializeField] public int reserveAmmo;
    [SerializeField] public float reloadTime;
    public bool isReloading;
}
