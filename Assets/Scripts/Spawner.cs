using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int EnemyCount;
        public float TimeBetweenSpawns;
    }

    public Wave[] Waves;
    public Enemy Enemy;

    int _enemiesRemainingToSpawn;
    float _nextSpawnTime;
    Wave _currentWave;
    int _currentWaveNumber;
    int _enemiesRemainingAlive;

    private void Start()
    {
        NextWave();
    }

    void Update()
    {
        if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawns;
            Enemy spawnedEnemy = Instantiate(Enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath;
        }
    }

    private void OnEnemyDeath()
    {
        //print("Enemy died");
        _enemiesRemainingAlive--;

        if (_enemiesRemainingAlive == 0)
        {
            NextWave();
        }

    }

    void NextWave()
    {
        _currentWaveNumber++;
        print($"Wave {_currentWaveNumber}");

        if (_currentWaveNumber-1 < Waves.Length)
        {

            _currentWave = Waves[_currentWaveNumber - 1];
            _enemiesRemainingToSpawn = _currentWave.EnemyCount;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;
        }
    }



}
