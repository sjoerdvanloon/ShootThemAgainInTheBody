using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    [Header("Game over")]
    public Image FadePlane;
    public float FadeTime = 1f;
    public Color FadeToColor = new Color(0, 0, 0, .95f);
    public GameObject GameOverUI;
    public GameObject GameOverScoreUI;

    [Header("New wave banner")]
    public RectTransform NewWaveBanner;
    public Text NewWaveTitle;
    public Text NewWaveEnemyCount;
    public float NewWavePauzedelay = 1.5f;
    public float NewWaveSpeed = 3;
    public float NewWaveStartDelay = 1f;
    [Header("Score")]
    public GameObject ScoreUI;
    [Header("Health")]
    public GameObject HealthUI;

    private Text Score => ScoreUI.GetComponentInChildren<Text>();
    private Text GameOverScore => GameOverScoreUI.GetComponentInChildren<Text>();
    private RectTransform HealthBacking => HealthUI.GetComponentInChildren<RectTransform>();
    private RectTransform HealthBar => HealthBacking.GetComponentInChildren<RectTransform>();


    Spawner _spawner;
    Player _player;

    // Start is called before the first frame update
    void Start()
    {
        _player = FindObjectOfType<Player>();
        _player.OnDeath += OnGameOver;
    }

    void Awake()
    {
        _spawner = FindObjectOfType<Spawner>();
        _spawner.OnNewWave += OnNewWave;


    }
    void Update()
    {
        Score.GetComponentInChildren<Text>().text = ScoreKeeper.Score.ToString("D6");
        float healthPercent = 0;
        if (_player != null)
        {
            healthPercent = _player.Health / _player.StartingHealth;
        }
        HealthBar.localScale = new Vector3(healthPercent, 0.66f, 0);
    }



    void OnNewWave(int waveNumber)
    {
        ShowNewWaveBanner(waveNumber);
    }

    void ShowNewWaveBanner(int waveNumber)
    {
        var wave = _spawner.Waves[waveNumber - 1];
        string[] numbers = new string[] { "One", "Two", "Three", "Four", "Five" };
        var newWaveText = $"- Wave {numbers[waveNumber - 1]} -";
        var enemyCountText = $"Enemies: {(wave.Infinite ? "Infinite" : wave.EnemyCount.ToString())}";

        NewWaveTitle.text = newWaveText;
        NewWaveEnemyCount.text = enemyCountText;

        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner()
    {

        yield return new WaitForSeconds(NewWaveStartDelay); // Makes it wonky

        float delayTime = NewWavePauzedelay;
        float speed = NewWaveSpeed;
        float animatePercent = 0;
        int dir = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;

            if (animatePercent >= 1)
            {
                animatePercent = 1;
                if (Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }

            NewWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-484, -200, animatePercent);
            //print(animatePercent);
            yield return null;
        }

    }

    void OnGameOver(DamageType type)
    {
        Cursor.visible = true; // Tip from TheCoolSquare
        StartCoroutine(Fade(Color.clear, FadeToColor, FadeTime));
        GameOverScore.text = Score.text;
        ScoreUI.SetActive(false);
        HealthUI.SetActive(false);
        GameOverUI.SetActive(true);

    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        var speed = 1 / time;
        float percent = 0;

        while (percent < speed)
        {
            percent += Time.deltaTime * speed;
            FadePlane.color = Color.Lerp(from, to, percent);
            yield return null; // Next frame
        }

    }

    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
