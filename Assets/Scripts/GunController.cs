using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{

    public Transform WeaponHold;
    public Gun StartingGun;

    public float GunHeight { get => WeaponHold.position.y; }


    void Start()
    {
        if (StartingGun != null)
            EquipGun(StartingGun);
    }
    Gun _equippedGun;

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

    public void OnTriggerHold()
    {
        _equippedGun?.OnTriggerHold();

    }

    public void OnTriggerRelease()
    {
        _equippedGun?.OnTriggerRelease();
      
    }



}
