using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Objective { CollectCoins, ReachGoal, CollectKeys, Survive }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Objective")]
    [SerializeField] Objective objective = Objective.ReachGoal;
    [SerializeField] int targetScore = 10;
    [SerializeField] int keysRequired = 3;
    [SerializeField] string objectiveLabel = "Reach the goal!";

    [Header("Clock (0 = no clock)")]
    [SerializeField] float timeLimit = 0f;
    [SerializeField] float deathTimePenalty = 0f;

    [Header("Lives and respawn")]
    [SerializeField] int lives = 3;
    [SerializeField] float respawnDelay = 0.6f;
    [SerializeField] float spawnProtection = 1.5f;
    [SerializeField] float respawnLift = 1.2f;
    [SerializeField] Health player;
    [SerializeField] Transform respawnPoint;

    [Header("HUD")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text resultText;
    [SerializeField] TMP_Text loseReasonText;
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] GameObject nextButton;

    int score;
    int keysCollected;
    float timeLeft;
    bool finished;

    public bool IsFinished => finished;
    public int Score => score;
    public int Lives => lives;
    public bool ExitUnlocked => objective != Objective.CollectKeys || keysCollected >= keysRequired;
    public bool GoalOpen => ExitUnlocked && score >= targetScore;
    public float Elapsed => timeLimit > 0f ? timeLimit - timeLeft : Time.timeSinceLevelLoad;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        timeLeft = timeLimit;
        if (timerText != null)
        {
            timerText.gameObject.SetActive(timeLimit > 0f);
            UpdateTimerText();
        }
        if (nextButton != null)
            nextButton.SetActive(SceneManager.GetActiveScene().buildIndex + 1 < SceneManager.sceneCountInBuildSettings);
        UpdateUI();
    }

    void Update()
    {
        if (timeLimit <= 0f || finished) return;
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            UpdateTimerText();
            if (objective == Objective.Survive)
            {
                if (score >= targetScore) Win();
                else Lose($"Only {score} / {targetScore} coins!");
            }
            else Lose("Time's up!");
            return;
        }
        UpdateTimerText();
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;
        string label = objective == Objective.Survive ? "Survive" : "Time";
        timerText.text = $"{label}: {timeLeft:0.0}";
        timerText.color = timeLeft < 10f && objective != Objective.Survive ? JuiceFx.DamageColor : Color.white;
    }

    public void AddTime(float seconds)
    {
        if (finished || timeLimit <= 0f) return;
        timeLeft += seconds;
        UpdateTimerText();
        PunchText(timerText);
    }

    public void AddScore(int amount)
    {
        if (finished) return;
        score += amount;
        UpdateUI();
        PunchText(scoreText);
        if (objective == Objective.CollectCoins && score >= targetScore) Win();
    }

    public void AddKey()
    {
        if (finished) return;
        keysCollected++;
        UpdateUI();
        PunchText(scoreText);
        if (player != null)
        {
            string message = keysCollected >= keysRequired ? "EXIT OPEN!" : $"KEY {keysCollected} / {keysRequired}";
            JuiceFx.FloatingText(player.transform.position + Vector3.up * 1.6f, message, JuiceFx.KeyColor, 20f);
        }
    }

    public bool ReachGoal()
    {
        if (finished || objective == Objective.CollectCoins || objective == Objective.Survive) return false;
        if (!GoalOpen)
        {
            if (player != null)
            {
                string message = !ExitUnlocked ? "FIND ALL THE KEYS" : $"NEED {targetScore - score} MORE COINS";
                JuiceFx.FloatingText(player.transform.position + Vector3.up * 1.6f, message, JuiceFx.DamageColor, 16f);
            }
            return false;
        }
        Win();
        return true;
    }

    public void SetCheckpoint(Transform point) => respawnPoint = point;

    public void UpdateUI()
    {
        if (scoreText != null)
        {
            switch (objective)
            {
                case Objective.CollectKeys:
                    scoreText.text = $"Keys: {keysCollected} / {keysRequired}\nCoins: {score} / {targetScore}";
                    break;
                case Objective.ReachGoal:
                    scoreText.text = $"Coins: {score} / {targetScore}\n{objectiveLabel}";
                    break;
                default:
                    scoreText.text = $"Coins: {score} / {targetScore}";
                    break;
            }
        }
        if (livesText != null && player != null)
            livesText.text = $"Lives: {lives}\nHearts: {Mathf.Max(player.Current, 0)} / {player.Max}";
    }

    static void PunchText(TMP_Text text)
    {
        if (text != null && text.TryGetComponent(out UiPunch punch)) punch.Punch();
    }

    public void PlayerDied()
    {
        if (finished) return;
        lives--;
        if (deathTimePenalty > 0f && timeLimit > 0f)
        {
            timeLeft = Mathf.Max(1f, timeLeft - deathTimePenalty);
            UpdateTimerText();
            if (player != null)
                JuiceFx.FloatingText(player.transform.position + Vector3.up * 1.6f, $"-{deathTimePenalty:0}s", JuiceFx.DamageColor, 18f);
        }
        UpdateUI();
        PunchText(livesText);
        if (lives <= 0) Lose("Out of lives!");
        else StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (finished) yield break;
        Rigidbody body = player.GetComponent<Rigidbody>();
        body.position = respawnPoint.position + Vector3.up * respawnLift;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        player.Restore(spawnProtection);
    }

    void Win()
    {
        if (finished) return;
        finished = true;
        if (resultText != null)
        {
            string summary = $"Coins: {score} / {targetScore}\nLives left: {lives}\nHearts left: {(player != null ? Mathf.Max(player.Current, 0) : 0)}";
            if (objective == Objective.CollectKeys) summary = $"Keys: {keysCollected} / {keysRequired}\n" + summary;
            if (timeLimit > 0f && objective != Objective.Survive) summary += $"\nTime left: {timeLeft:0.0} s";
            resultText.text = summary;
        }
        winPanel.SetActive(true);
        Time.timeScale = 0f;

        Vector3 center = (player != null ? player.transform.position : Vector3.zero) + Vector3.up;
        JuiceFx.Burst(center, JuiceFx.GoldColor, 70, 9f, 0.25f, 1.6f, true, 1.2f);
        JuiceFx.Burst(center, JuiceFx.SpeedColor, 50, 8f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Burst(center, JuiceFx.HeartColor, 50, 7f, 0.22f, 1.6f, true, 1.2f);
        JuiceFx.Ring(center, JuiceFx.GoldColor, 7f, 0.9f, true);
    }

    void Lose(string reason)
    {
        if (finished) return;
        finished = true;
        if (loseReasonText != null) loseReasonText.text = reason;
        losePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next >= SceneManager.sceneCountInBuildSettings) next = 0;
        SceneManager.LoadScene(next);
    }
}
