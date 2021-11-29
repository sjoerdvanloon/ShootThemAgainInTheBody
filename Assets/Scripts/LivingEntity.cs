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
    public float Health => _health;

    public event Action<DamageType> OnDeath;

    protected virtual void Start()
    {
        _health = StartingHealth;
    }


    public  virtual void TakeHit(Damage damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        TakeDamage(damage);
    }

    public virtual void TakeDamage(Damage damage)
    {
        _health -= damage.Amount;
        if (_health <= 0 && !_dead)
        {
            Die(damage.Type);
        }
    }

    [ContextMenu("SelfDestruct")]
    public virtual void Die(DamageType deathType)
    {
        _dead = true;

        OnDeath?.Invoke(deathType);

        GameObject.Destroy(gameObject);
    }
}
