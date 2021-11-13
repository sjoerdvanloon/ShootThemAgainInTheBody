using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class LivingEntity : MonoBehaviour, IDamagable
{
    public float StartingHealth;
    protected float _health;
    protected float _maxHealth;
    protected bool _dead;

    protected virtual void Start()
    {
        _health = StartingHealth;
    }


    public void TakeHit(float damage, RaycastHit hit)
    {
        _health -= damage;
        if (_health <= 0  && !_dead)
        {
            Die();
        }
    }

    private void Die()
    {
        _dead = true;
        GameObject.Destroy(gameObject);
    }
}
