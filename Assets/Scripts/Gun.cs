using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform Muzzle;
    public Projectile Projectile;
    public float MsBetweenShots = 10;
    public float MuzzleVelocity = 100;

    float _nextShotTime;

    public void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            _nextShotTime = Time.time + MsBetweenShots / 1000;
            Projectile newProjectile = Instantiate(Projectile, Muzzle.position, Muzzle.rotation) as Projectile;
            newProjectile.SetSpeed(MuzzleVelocity);
        }
    }
}
