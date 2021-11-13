using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask CollisionMask;

    float _speed = 10;
    float _damage = 1;
    float _lifeTime = 3f;
    float _skinWidth = .1f;

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
        Destroy(gameObject,_lifeTime); // Destroy it after some time

        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, CollisionMask);

        if (initialCollisions.Length > 0)
        {
            // Colliding with one
            OnHitObject(initialCollisions[0]);

        }
    }

    // Update is called once per frame
    void Update()
    {
        // Ray cast
        float moveDistance = _speed * Time.deltaTime;
        CheckCollections(moveDistance);

        // Send bullit
        transform.Translate(Vector3.forward * moveDistance);
    }

    private void CheckCollections(float moveDistance)
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, moveDistance + _skinWidth, CollisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit);
        }
    }

    private void OnHitObject(RaycastHit hit)
    {
        // Register hit
        //print(hit.collider.gameObject.name);
        IDamagable damagable = hit.collider.GetComponent<IDamagable>();
        damagable?.TakeHit(_damage, hit);

        GameObject.Destroy(gameObject);
    }
    void OnHitObject(Collider collider)
    {
        IDamagable damagable = collider.GetComponent<IDamagable>();
        damagable?.TakeDamage(_damage);

        GameObject.Destroy(gameObject);
    }
}
