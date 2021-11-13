using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : LivingEntity
{
    NavMeshAgent _pathFinder;
    Transform _target;

    protected override void Start()
    {
        base.Start();
        _pathFinder = GetComponent<NavMeshAgent>();
        var player = GameObject.FindGameObjectWithTag("Player");
        _target = player.transform;

        StartCoroutine(UpdatePath());
    }

    void Update()
    {
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = .25f;

        while(_target != null)
        {
            Vector3 targetPosition = new Vector3(_target.position.x, 0, _target.position.z);
            if (!_dead)
            {
                _pathFinder.SetDestination(targetPosition);
            }

            yield return new WaitForSeconds(refreshRate);

        }
    }

}
