//using UnityEngine;

//public class SimonScores : MonoBehaviour
//{
//    [SerializeField] private int score_points = 0;
//    [SerializeField] public int highscore_point = 0;

//    private const string HIGHSCORE_KEY = "highscore_point";

//    private void Start()
//    {
//        LoadHighScore();
//        Set();
//    }

//    public void Addto(int amt = 1)
//    {
//        score_points += amt;
//        CheckForNewHighscore();
//    }

//    public void Set(int amt = 0)
//    {
//        score_points = amt;
//        CheckForNewHighscore();
//    }

//    public int GetCurrentScore()
//    {
//        return score_points;
//    }

//    public int GetHighScore()
//    {
//        return highscore_point;
//    }

//    private void LoadHighScore()
//    {
//        highscore_point = PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
//    }

//    private void SaveHighScore()
//    {
//        PlayerPrefs.SetInt(HIGHSCORE_KEY, highscore_point);
//        PlayerPrefs.Save(); // Azonnal menti a változásokat
//    }

//    public void CheckForNewHighscore()
//    {
//        if (score_points > highscore_point)
//        {
//            highscore_point = score_points;
//            SaveHighScore();
//            Debug.Log($"New high score: {highscore_point}");
//        }
//    }

//    // Játék végén explicit mentés, ha szükséges
//    private void OnDisable()
//    {
//        SaveHighScore();
//    }
//}

using UnityEngine;

public class SimonScores : MonoBehaviour
{
    [SerializeField] private int score_points = 0;
    [SerializeField] private int highscore_point = 0;

    private const string HIGHSCORE_KEY = "highscore_point";

    private void Start()
    {
        LoadHighScore();
        ResetScore();
    }

    public void ResetScore()
    {
        score_points = 0;
    }

    public void Set(int newScore)
    {
        score_points = newScore;
        Debug.Log($"Score updated to: {score_points}");
        CheckForNewHighscore();
    }

    private void LoadHighScore()
    {
        highscore_point = PlayerPrefs.GetInt(HIGHSCORE_KEY, 0);
        Debug.Log($"Loaded highscore: {highscore_point}");
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
        }
    }

    public void ClearHighScore()
    {
        PlayerPrefs.DeleteKey(HIGHSCORE_KEY);
        highscore_point = 0;
        Debug.Log("Highscore cleared");
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
