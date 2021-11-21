using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum FireMode { Auto, Burst, Single }
public class Gun : MonoBehaviour
{


    public FireMode FireMode = FireMode.Auto;
    public Transform[] ProjectileSpawners;

    public Projectile Projectile;
    public float MsBetweenShots = 10;
    public float MuzzleVelocity = 100;
    public int burstCount;
    public int projectilesPerMagazine;
   
    [Header("Effects")]
    public Transform Shell;
    public Transform ShellEjector;

    [Header("Recoil")]
    public Vector2 KickMinMax = new Vector2(.05f, .2f);
    public Vector2 RecoilAngleMinMax = new Vector2(3,5);
    public float RecoilMovementSettleTime = .1f;
    public float RecoilRotationSettleTime = .1f;

    MuzzleFlash _muzzleFlash;
    float _nextShotTime;

    bool _triggerReleasedSinceLastShot;
    int _shotsRemainingInBurst;

    Vector3 _recoilSmoothDownVelocity;
    float _recoilAngle;
    float _recoilRotationSmoothDampVelocity;

    int _projectilesInMagazine;

    private void Start()
    {
        _muzzleFlash = GetComponent<MuzzleFlash>();
        _shotsRemainingInBurst = burstCount;

        _projectilesInMagazine = projectilesPerMagazine; // Hier ben ik gebleven https://youtu.be/r8JTwe6dewU?t=861
    }

    private void LateUpdate()
    {
        // Animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref _recoilSmoothDownVelocity, RecoilMovementSettleTime);
       _recoilAngle = Mathf.SmoothDamp(_recoilAngle, 0, ref _recoilRotationSmoothDampVelocity, RecoilRotationSettleTime);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * _recoilAngle;
    }

    public void Aim(Vector3 aimPoint)
    {
        // Override transform done in the Update
        transform.LookAt(aimPoint);
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

            // Recoil
            transform.localPosition -= Vector3.forward * Random.Range(KickMinMax.x, KickMinMax.y);
            _recoilAngle += Random.Range(RecoilAngleMinMax.x, RecoilAngleMinMax.y);
            _recoilAngle = Mathf.Clamp(_recoilAngle, 0, 30);

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
