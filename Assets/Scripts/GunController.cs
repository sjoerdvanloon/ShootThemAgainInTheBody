using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{

    public Transform WeaponHold;
    public Gun[] AllGuns;

    public float GunHeight { get => WeaponHold.position.y; }
    Gun _equippedGun;


    void Start()
    {

    }



    public void Aim(Vector3 aimPoint) => _equippedGun?.Aim(aimPoint);

    public void EquipGun(Gun gunToEquip)
    {
        if (_equippedGun != null)
        {
            Destroy(_equippedGun.gameObject);
        }
        _equippedGun = Instantiate(gunToEquip, WeaponHold.position, WeaponHold.rotation) as Gun;
        _equippedGun.transform.parent = WeaponHold;
    }

    public void Reload() => _equippedGun?.Reload();

    public void OnTriggerHold()
    {
        _equippedGun?.OnTriggerHold();

    }

    public void OnTriggerRelease()
    {
        _equippedGun?.OnTriggerRelease();
      
    }



}
