using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{

    public Image FadePlane;
    public GameObject GameOverUI;

    // Start is called before the first frame update
    void Start()
    {
        var player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }

    void OnGameOver()
    {
        Cursor.visible = true; // Tip from TheCoolSquare
        StartCoroutine(Fade(Color.clear, Color.black, 1));
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
       // Application.LoadLevel("Game"); // Obsolete
       SceneManager.LoadScene("Game");
    }
}
