using UnityEngine;
using UnityEngine.Events;

public class SimonScores : MonoBehaviour
{
    [SerializeField] private int score_points = 0;
    [SerializeField] private int highscore_point = 0;
    private const string HIGHSCORE_KEY = "highscore_point";

    // Esemény a pontszám változásához
    public UnityEvent<int> onScoreChanged = new UnityEvent<int>();
    public UnityEvent<int> onHighScoreChanged = new UnityEvent<int>();

    private void Start()
    {
        //LoadHighScore();
        ResetScore();
    }

    public void ResetScore()
    {
        score_points = 0;
        onScoreChanged.Invoke(score_points);
    }

    public void Set(int newScore)
    {
        score_points = newScore;
        Debug.Log($"Score updated to: {score_points}");
        onScoreChanged.Invoke(score_points);
        CheckForNewHighscore();
    }

    // Növelési függvény a pontokhoz
    public void AddPoint()
    {
        score_points++;
        Debug.Log($"Score increased to: {score_points}");
        onScoreChanged.Invoke(score_points);
        CheckForNewHighscore();
    }

    private void LoadHighScore()
    {
        highscore_point = PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
        Debug.Log($"Loaded highscore: {highscore_point}");
        onHighScoreChanged.Invoke(highscore_point);
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGHSCORE_KEY, highscore_point);
        PlayerPrefs.Save();
        Debug.Log($"Saved highscore: {highscore_point}");
    }

    public void CheckForNewHighscore()
    {
        if (score_points > highscore_point)
        {
            highscore_point = score_points;
            SaveHighScore();
            Debug.Log($"New highscore achieved: {highscore_point}!");
            onHighScoreChanged.Invoke(highscore_point);
        }
    }

    public void ClearHighScore()
    {
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
        highscore_point = 0;
        Debug.Log("Highscore cleared");
        onHighScoreChanged.Invoke(highscore_point);
    }

    public int GetCurrentScore()
    {
        return score_points;
    }

    public int GetHighScore()
    {
        return highscore_point;
    }
}