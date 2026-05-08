using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI coinCountText;
    [SerializeField] GameObject      gameOverPanel;
    [SerializeField] TextMeshProUGUI finalScoreText;

    public int Score     { get; private set; }
    public int CoinCount { get; private set; } = 10;

    int activeCoins;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        RefreshUI();
    }

    public void AddScore(int points, Vector3 pos, Color color)
    {
        Score += points;
        ScorePopup.Spawn(points, pos, color);
        StartCoroutine(PulseText(scoreText.transform));
        RefreshUI();
    }

    public bool TrySpendCoin()
    {
        if (CoinCount <= 0) return false;
        CoinCount--;
        StartCoroutine(PulseText(coinCountText.transform));
        RefreshUI();
        return true;
    }

    public void RegisterCoinSpawned() => activeCoins++;

    public void RegisterCoinLanded()
    {
        activeCoins = Mathf.Max(0, activeCoins - 1);
        if (CoinCount == 0 && activeCoins == 0)
            ShowGameOver();
    }

    public void RestartGame() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    void ShowGameOver()
    {
        if (finalScoreText) finalScoreText.text = $"Score: {Score}";
        if (gameOverPanel)  gameOverPanel.SetActive(true);
        GameSFX.Instance?.PlayGameOver();
    }

    void RefreshUI()
    {
        if (scoreText)     scoreText.text     = $"Score: {Score}";
        if (coinCountText) coinCountText.text = $"Coins: {CoinCount}";
    }

    IEnumerator PulseText(Transform t)
    {
        if (t == null) yield break;
        float elapsed = 0f;
        const float dur = 0.22f;
        while (elapsed < dur)
        {
            float s = 1f + Mathf.Sin(elapsed / dur * Mathf.PI) * 0.35f;
            t.localScale = Vector3.one * s;
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = Vector3.one;
    }
}
