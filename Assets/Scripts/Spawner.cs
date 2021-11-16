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
    MapGenerator _mapGenerator;
    LivingEntity _playerEntity;
    Transform _playerTransform { get => _playerEntity.transform; }

    float _timeBetweenCampingChecks = 2f;
    float _nextCampingCheckTime;
    float _campThresholdDistance = 1.5f;
    Vector3 _campPositionOld;
    bool _isCamping;


    private void Start()
    {
        // Get player
        _playerEntity = FindObjectOfType<Player>();

        // Camping
        _nextCampingCheckTime = _timeBetweenCampingChecks + Time.time;
        _campPositionOld = _playerTransform.position;


        // Get player
        _mapGenerator = FindObjectOfType<MapGenerator>();

        NextWave();
    }

    void Update()
    {
        if (Time.time> _nextCampingCheckTime)
        {
            _nextCampingCheckTime = Time.time + _timeBetweenCampingChecks;

            _isCamping = (Vector3.Distance(_playerTransform.position, _campPositionOld)  < _campThresholdDistance);
            _campPositionOld = _playerTransform.position;
        }

        if (_enemiesRemainingToSpawn > 0 && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawns;
            StartCoroutine(SpawnEnemy());
        }
    }

    private IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1;
        float tileFlashSpeed = 4;
        var spawnTile = _mapGenerator.GetRandomOpenTile();
        if (_isCamping)
        {
            spawnTile = _mapGenerator.GetTileFromPosition(_playerTransform.position);
        }
        var tileMaterial = spawnTile.GetComponent<Renderer>().material;
        var initialColor = tileMaterial.color;
        var flashColor = Color.red;
        float spawnTimer = 0;
        while(spawnTimer < spawnDelay)
        {
            tileMaterial.color=Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }


        Enemy spawnedEnemy = Instantiate(Enemy,spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;

        //yield return null;
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
