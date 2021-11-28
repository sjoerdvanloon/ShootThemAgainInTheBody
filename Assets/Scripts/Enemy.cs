using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State { Idle, Chasing, Attacking };
    public ParticleSystem DeathEffect;

    NavMeshAgent _pathFinder;
    Transform _target;
    Material _skinMaterial;
    Color _originalColor;
    float _attackDistanceThreshold = .5f;
    float _timeBetweenAttacks = 1f;
    float _nextAttackTime;
    State _currentState;
    float _myCollisionRadius;
    float _targetCollisionRadius;
    LivingEntity _targetEntity;
    bool _hasTarget;
    float _damage = 1f;


     void Awake()
    {
        _pathFinder = GetComponent<NavMeshAgent>();
        _skinMaterial = GetComponent<Renderer>().material;

        var player = GameObject.FindGameObjectWithTag("Player");
        _hasTarget = (player != null);

        if (_hasTarget)
        {
            _target = player.transform;
            _targetEntity = player.GetComponent<LivingEntity>();
            //_targetEntity.OnDeath += OnTargetDeath; // Sebastian put this in, start, not completely sure why, for now I will just follow along

            _myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            _targetCollisionRadius = _target.GetComponent<CapsuleCollider>().radius;
        }
    }

   

    protected override void Start()
    {
        base.Start();

        //print($"{_hasTarget}");
     
        if (_hasTarget)
        {
            _currentState = State.Chasing;
            _targetEntity.OnDeath += OnTargetDeath;

            StartCoroutine(UpdatePath());
        }

    }

    internal void SetCharacteristics(float moveSpeed, int hitsToKillPlayer, float health, Color skinColor)
    {
        _pathFinder.speed = moveSpeed;

        if (_hasTarget)
        {
            _damage = Mathf.Ceil(_targetEntity.StartingHealth / hitsToKillPlayer);
        }

        StartingHealth = health;
        _originalColor = skinColor;
        _skinMaterial.color = skinColor;
    }


    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        //print($"Damage: {damage}");
        //print($"Health: {_health}");

        // Hit!
        AudioManager.Instance.PlaySound("impact", transform.position);

        // Die?
        var takingHitResultInDeath = (damage >= _health);

        //print($"Take hit {(takingHitResultInDeath ? "does" : "does not")} result in death");

        if (takingHitResultInDeath)
        {
            // AUdio
            AudioManager.Instance.PlaySound("enemydeath", transform.position);

            // Particle set
            //https://youtu.be/PAKYDX9gPNQ?t=677        }
            var deathEffect = Instantiate(DeathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection));
            var deathEffectShader = deathEffect.GetComponent<ParticleSystemRenderer>();
            deathEffectShader.material.color = _originalColor;
            ////print($"Start life time death effect: {startLifeTimeDeathEffect}"); // https://youtu.be/PAKYDX9gPNQ?t=758 other way, Sebastian way means that we dont have to get the compontent each time


            var startLifeTimeDeathEffect = DeathEffect.main.startLifetime.constantMax;
            Destroy(deathEffect, startLifeTimeDeathEffect);
        }

        base.TakeHit(damage, hitPoint, hitDirection);


    }

    void OnTargetDeath()
    {
        _hasTarget = false;
        _currentState = State.Idle;
    }

    void Update()
    {
        if (!_hasTarget)
            return; // No Target, don't attack

        if (Time.time <= _nextAttackTime)
            return; // Not ready to attack, don't attack

        // Attack
        //Vector3.Distance() slow https://www.youtube.com/watch?v=njqRlH3Hj3Q&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0&index=7
        float sqrDstToTarget = (_target.position - transform.position).sqrMagnitude; // Faster

        if (sqrDstToTarget < Mathf.Pow(_attackDistanceThreshold + _myCollisionRadius + _targetCollisionRadius, 2))
        {
            _nextAttackTime = Time.time + _timeBetweenAttacks;
            AudioManager.Instance.PlaySound("enemyattack", transform.position);

            StartCoroutine(Attack());

        }
    }

   

    IEnumerator Attack()
    {
        if (!_hasTarget)
            yield return null;

        _currentState = State.Attacking;
        _pathFinder.enabled = false;
        // Leap
        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        Vector3 attackPosition = _target.position - directionToTarget * (_myCollisionRadius);

        float attackSpeed = 3;
        float percent = 0;

        _skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent <= 1)
        {
            // Animate our launch

            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                _targetEntity.TakeDamage(_damage);
            }

            percent += Time.deltaTime * attackSpeed;
            var interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;  // https://www.youtube.com/watch?v=njqRlH3Hj3Q&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0&index=7
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

            yield return null;
        }

        _skinMaterial.color = _originalColor;
        _currentState = State.Chasing;

        _pathFinder.enabled = true;

    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while (_hasTarget)
        {
            if (_currentState == State.Chasing)
            {
                Vector3 directionToTarget = (_target.position - transform.position).normalized;
                Vector3 targetPosition = _target.position - directionToTarget * (_myCollisionRadius + _targetCollisionRadius + _attackDistanceThreshold / 2);
                if (!_dead)
                {
                    _pathFinder.SetDestination(targetPosition);
                }
            }

            yield return new WaitForSeconds(refreshRate);

        }
    }

}
