using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{

    public enum State { Idle, Chasing, Attacking };

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


    protected override void Start()
    {
        base.Start();
        _pathFinder = GetComponent<NavMeshAgent>();
        _skinMaterial = GetComponent<Renderer>().material;
        _originalColor = _skinMaterial.color;

        _currentState = State.Chasing;

        var player = GameObject.FindGameObjectWithTag("Player");
        _target = player.transform;

        _myCollisionRadius = GetComponent<CapsuleCollider>().radius;
        _targetCollisionRadius = _target.GetComponent<CapsuleCollider>().radius;

        StartCoroutine(UpdatePath());
    }

    void Update()
    {
        if (Time.time > _nextAttackTime)
        {
            //Vector3.Distance() slow https://www.youtube.com/watch?v=njqRlH3Hj3Q&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0&index=7
           // float sqrDstToTarget = (_target.position - transform.position).sqrMagnitude; // Faster Center
            float sqrDstToTarget = (_target.position - transform.position).sqrMagnitude; // Faster

            if (sqrDstToTarget < Mathf.Pow(_attackDistanceThreshold + _myCollisionRadius + _targetCollisionRadius, 2))
            {
                _nextAttackTime = Time.time + _timeBetweenAttacks;
                StartCoroutine(Attack());

            }
        }

    }

    IEnumerator Attack()
    {
        _currentState = State.Attacking;
        _pathFinder.enabled = false;
        // Leap
        Vector3 originalPosition = transform.position;
        Vector3 directionToTarget = (_target.position - transform.position).normalized;
        Vector3 attackPosition = _target.position - directionToTarget * (_myCollisionRadius  );

        float attackSpeed = 3;
        float percent = 0;

        _skinMaterial.color = Color.red;

        while (percent <= 1)
        {
            // Animate our launch

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;  // https://www.youtube.com/watch?v=njqRlH3Hj3Q&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0&index=7
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

        while (_target != null)
        {
            if (_currentState == State.Chasing)
            {
                Vector3 directionToTarget = (_target.position - transform.position).normalized;
                Vector3 targetPosition = _target.position - directionToTarget * (_myCollisionRadius + _targetCollisionRadius + _attackDistanceThreshold /2);
                if (!_dead)
                {
                    _pathFinder.SetDestination(targetPosition);
                }
            }

            yield return new WaitForSeconds(refreshRate);

        }
    }

}
