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
        public float MoveSpeed;
        public int hitsToKillPlayer; // Damage given
        public float Health;
        public Color SkinColor;
        public bool Infinite;
    }
    public bool DeveloperMode = false;

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
    private bool _isDisabled;

    public  System.Action<int> OnNewWave; 

    private void Start()
    {
        // Get player
        _playerEntity = FindObjectOfType<Player>();
        _playerEntity.OnDeath += () => OnPlayerDeath();

        // Camping
        _nextCampingCheckTime = _timeBetweenCampingChecks + Time.time;
        _campPositionOld = _playerTransform.position;


        // Get player
        _mapGenerator = FindObjectOfType<MapGenerator>();

        NextWave();
    }

    void Update()
    {

        if (DeveloperMode)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StopCoroutine(nameof(SpawnEnemy)); // only possible to stop coroutines with name
                foreach (var enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }

        if (_isDisabled)
            return;

        if (Time.time> _nextCampingCheckTime)
        {
            _nextCampingCheckTime = Time.time + _timeBetweenCampingChecks;

            _isCamping = (Vector3.Distance(_playerTransform.position, _campPositionOld)  < _campThresholdDistance);
            _campPositionOld = _playerTransform.position;
        }

        var spawnMoreEnemies = (_enemiesRemainingToSpawn > 0 || _currentWave.Infinite);
        if (spawnMoreEnemies  && Time.time > _nextSpawnTime)
        {
            _enemiesRemainingToSpawn--;
            _nextSpawnTime = Time.time + _currentWave.TimeBetweenSpawns;
            StartCoroutine(nameof(SpawnEnemy));
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


        var spawnedEnemy = Instantiate(Enemy,spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(_currentWave.MoveSpeed, _currentWave.hitsToKillPlayer, _currentWave.Health, _currentWave.SkinColor);

        if (tileMaterial.color != initialColor)
        {
            tileMaterial.color = initialColor;
        }
    }

    private void ResetPlayerPosition()
    {
        _playerTransform.position = _mapGenerator.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    private void OnPlayerDeath()
    {
        _isDisabled = true;
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

        if (_currentWaveNumber < Waves.Length)
        {
            print($"Wave {_currentWaveNumber}");

            _currentWaveNumber++;

            _currentWave = Waves[_currentWaveNumber - 1];
            _enemiesRemainingToSpawn = _currentWave.EnemyCount;
            _enemiesRemainingAlive = _enemiesRemainingToSpawn;

            if (OnNewWave != null)
            {
                OnNewWave(_currentWaveNumber);
            }
            ResetPlayerPosition();
        }
    }



}
