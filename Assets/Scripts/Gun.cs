using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode { Auto, Burst, Single }
public class Gun : MonoBehaviour
{

    public FireMode FireMode = FireMode.Auto;

    public Transform[] ProjectileSpawners;

    public Projectile Projectile;
    public float MsBetweenShots = 10;
    public float MuzzleVelocity = 100;
    public int burstCount;

    public Transform Shell;
    public Transform ShellEjector;

    MuzzleFlash _muzzleFlash;
    float _nextShotTime;

    bool _triggerReleasedSinceLastShot;
    int _shotsRemainingInBurst;

    private void Start()
    {
        _muzzleFlash = GetComponent<MuzzleFlash>();
        _shotsRemainingInBurst = burstCount;
    }

    void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            switch (FireMode)
            {
                case FireMode.Auto:
                    // nothing special
                    break;
                case FireMode.Burst:
                    if (_shotsRemainingInBurst == 0)
                        return; // Burst done
                    _shotsRemainingInBurst--;
                    break;
                case FireMode.Single:
                    if (!_triggerReleasedSinceLastShot)
                    {
                        return; // in single shot the user needs to release the trigger
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            _nextShotTime = Time.time + MsBetweenShots / 1000;
            foreach (var spawner in ProjectileSpawners)
            {
                Projectile newProjectile = Instantiate(Projectile, spawner.position, spawner.rotation) as Projectile;
                newProjectile.SetSpeed(MuzzleVelocity);


            }
            Instantiate(Shell, ShellEjector.position, ShellEjector.rotation); // https://youtu.be/e1XO53GA7xM?t=421
            _muzzleFlash.Activate();
        }
    }

    public void OnTriggerHold()
    {
        Shoot();
        _triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        _triggerReleasedSinceLastShot = true;
        _shotsRemainingInBurst = burstCount;


    }


}
