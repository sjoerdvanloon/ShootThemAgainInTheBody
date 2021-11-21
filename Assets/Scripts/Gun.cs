using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform Muzzle;
 
    public Projectile Projectile;
    public float MsBetweenShots = 10;
    public float MuzzleVelocity = 100;

    public Transform Shell;
    public Transform ShellEjector;

    MuzzleFlash _muzzleFlash;
    float _nextShotTime;

    private void Start()
    {
        _muzzleFlash    = GetComponent<MuzzleFlash>();
    }

    public void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            _nextShotTime = Time.time + MsBetweenShots / 1000;
            Projectile newProjectile = Instantiate(Projectile, Muzzle.position, Muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(MuzzleVelocity);

            Instantiate(Shell, ShellEjector.position, ShellEjector.rotation); // https://youtu.be/e1XO53GA7xM?t=421

            // Muzzle flash
            _muzzleFlash.Activate();
        }
    }


}
