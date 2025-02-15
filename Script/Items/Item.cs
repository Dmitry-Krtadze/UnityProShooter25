using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public ItemInfo itemInfo;
    public GameObject itemGameObject;
    public string weaponType;

    [SerializeField] public int maxAmmo;
    [SerializeField] public int currentAmmo;
    [SerializeField] public int reserveAmmo;
    [SerializeField] public float reloadTime;
    public bool isReloading;

    public abstract void Use();
}
