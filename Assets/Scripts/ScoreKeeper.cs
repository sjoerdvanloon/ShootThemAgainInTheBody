using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int Score { get; private set; }
    public float StreakExpiryTime = 1;
    float _lastEnemyKilledTime;
    int _streakCount;

    void Start()
    {
        var player = FindObjectOfType<Player>();
        player.OnDeath += OnPlayerDeath;
        Enemy.OnDeathStatic += OnEnemyKilled;
    }

    void OnEnemyKilled()
    {
        if (Time.time < _lastEnemyKilledTime + StreakExpiryTime)
        {
            _streakCount++;
        }
        else
        {
            _streakCount = 0;
        }

        _lastEnemyKilledTime = Time.time;

        Score += 5 + (int)Mathf.Pow(2, _streakCount);
    }

    void OnPlayerDeath(DamageType type)
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
