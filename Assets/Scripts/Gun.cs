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
    public bool ShellsEnabled = true;
    public Transform Shell;
    public Transform ShellEjector;
    public bool MuzzleFlashedEnabled = true;
    public AudioClip ShootAudio;
    public AudioClip ReloadAudio;

    MuzzleFlash _muzzleFlash;

    [Header("Kick")]
    public bool KickEnabled = true;
    public Vector2 KickMinMax = new Vector2(.05f, .2f);

    [Header("Recoil")]
    public bool RecoilEnabled = true;
    public Vector2 RecoilAngleMinMax = new Vector2(3, 5);
    public float RecoilMovementSettleTime = .1f;
    public float RecoilRotationSettleTime = .1f;

    Vector3 _recoilSmoothDownVelocity;
    float _recoilAngle;
    float _recoilRotationSmoothDampVelocity;

    [Header("Reloading")]
    public float ReloadTime = 0.3f;
    public float MaxReloadAngle = 30f;
    public bool LogReload = false;

    float _reloadAngle;



    float _nextShotTime;
    bool _triggerReleasedSinceLastShot;
    int _shotsRemainingInBurst;
    int _projectilesRemainingInMagazine;
    bool _isCurrentlyReloading;

    private void Start()
    {
        _muzzleFlash = GetComponent<MuzzleFlash>();
        _shotsRemainingInBurst = burstCount;

        _projectilesRemainingInMagazine = projectilesPerMagazine; // Hier ben ik gebleven https://youtu.be/r8JTwe6dewU?t=861
    }

    private void LateUpdate()
    {
        // Animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref _recoilSmoothDownVelocity, RecoilMovementSettleTime);
        _recoilAngle = Mathf.SmoothDamp(_recoilAngle, 0, ref _recoilRotationSmoothDampVelocity, RecoilRotationSettleTime);
        transform.localEulerAngles = Vector3.left * (_recoilAngle + _reloadAngle); // Fix from comment + my own special sauce, which hopefully keeps working, because then I understand stuff :D

        var autoReload = (!_isCurrentlyReloading && _projectilesRemainingInMagazine == 0);
        if (autoReload)
        {
            Reload();
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        // Override transform done in the Update
        transform.LookAt(aimPoint);
    }

    void Shoot()
    {
        var barrelEmpty = Time.time > _nextShotTime;
        var magazineEmpty = _projectilesRemainingInMagazine == 0;
        //    print($"Try to shoot and magazine is empty: {magazineEmpty} and barrel is empty: {barrelEmpty}");
        if (!_isCurrentlyReloading && barrelEmpty && !magazineEmpty)
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

            // print("Time for a bullit");

            _nextShotTime = Time.time + MsBetweenShots / 1000;

            foreach (var spawner in ProjectileSpawners)
            {
                if (_projectilesRemainingInMagazine == 0)
                {
                    break; // Empty mag, so stop
                }
                _projectilesRemainingInMagazine--;
                Projectile newProjectile = Instantiate(Projectile, spawner.position, spawner.rotation) as Projectile;
                newProjectile.SetSpeed(MuzzleVelocity);
            }

            //print($"number of projectiles {_projectilesRemainingInMagazine}");

            if (ShellsEnabled)
            {
                Instantiate(Shell, ShellEjector.position, ShellEjector.rotation); // https://youtu.be/e1XO53GA7xM?t=421
            }

            if (MuzzleFlashedEnabled)
            {
                _muzzleFlash.Activate();
            }

            // Recoil
            if (KickEnabled)
            {
                transform.localPosition -= Vector3.forward * Random.Range(KickMinMax.x, KickMinMax.y);
            }
            if (RecoilEnabled)
            {
                _recoilAngle += Random.Range(RecoilAngleMinMax.x, RecoilAngleMinMax.y);
                _recoilAngle = Mathf.Clamp(_recoilAngle, 0, 30);
            }

            // Audio
            AudioManager.Instance.PlaySound(ShootAudio, transform.position);

        }
    }

    public void Reload()
    {
        if (!_isCurrentlyReloading)
        {
            var magazinFull = (projectilesPerMagazine == _projectilesRemainingInMagazine);
            if (!magazinFull)
            {
                printif(LogReload, "Reload gun");

                StartCoroutine(AnimateReload());
                AudioManager.Instance.PlaySound(ReloadAudio, transform.position);
            }
        }
    }


    private void printif(bool predicate, string message)
    {
        if (predicate == true)
            print(message);
    }

    IEnumerator AnimateReload()
    {
        _isCurrentlyReloading = true;

        yield return new WaitForSeconds(0.2f); // Delay

        float reloadSpeed = 1 / ReloadTime;
        float percent = 0;
        Vector3 initialRotation = transform.localEulerAngles;

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;

            var interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, MaxReloadAngle, interpolation);
            _reloadAngle = reloadAngle;
            //transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

            yield return null; // Wait until next frame
        }

        _isCurrentlyReloading = false;

        _projectilesRemainingInMagazine = projectilesPerMagazine; // reset magazinee
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
