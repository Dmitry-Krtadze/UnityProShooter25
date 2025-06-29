using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ShootGun : Gun
{
    [SerializeField] Camera myCam;
    PlayerController playerC;
    public GunInfo gunInfo;
    public override void Use()
    {
        Shoot();
    }

    private void Shoot()
    {
        gunInfo = (GunInfo)itemInfo;
        if (gunInfo.isReloading) return;

        if (gunInfo.currentAmmo > 0)
        {
            //gunInfo.currentAmmo--;
            Ray ray = myCam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            ray.origin = myCam.transform.position;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hit.collider.gameObject.GetComponent<IDamageble>()?.
                    TakeDamage(gunInfo.Damage, PhotonNetwork.NickName, false);
            }
        }
        else
        {
            // Звук пустого магазина (если нужно)
        }
    }
    


}
