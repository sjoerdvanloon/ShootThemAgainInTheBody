using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask CollisionMask;
    public Color TrailColor;

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
            OnHitObject(initialCollisions[0], transform.position);

        }

        GetComponentInChildren<TrailRenderer>().startColor = TrailColor;

       // GetComponent<TrailRenderer>().material.SetColor("_Color", TrailColor); // This only works when you add it as a component under the game object it self
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
            OnHitObject(hit.collider, hit.point);
        }
    }

    void OnHitObject(Collider collider, Vector3 hitPoint)
    {
        IDamagable damagable = collider.GetComponent<IDamagable>();
        damagable?.TakeHit(_damage, hitPoint, transform.forward);

        GameObject.Destroy(gameObject);
    }
}
