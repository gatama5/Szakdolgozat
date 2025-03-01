//OLD DB SCOREMANAGER

//using TMPro;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;
//using System;
//using UnityEngine.SceneManagement;
//using System.Text;

//public class ScoreManager : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] private TextMeshProUGUI simonScore;
//    [SerializeField] private TextMeshProUGUI shootingScore;
//    [SerializeField] private TextMeshProUGUI mazeScore;

//    // �j UI elem az aktu�lis Simon pontsz�mhoz
//    [SerializeField] private TextMeshProUGUI simonCurrentScore;

//    [Header("Game References")]
//    [SerializeField] private SimonScores sm_scr;
//    [SerializeField] private ObjectSpawner_1place objectSpawner;
//    [SerializeField] private ButtonsForMaze maze_scr;
//    [SerializeField] private Gun gunscript;

//    private SQLiteDBScript dbManager;
//    private List<double> shootingTimes = new List<double>();
//    private List<string> hitPositions = new List<string>();
//    private double bestTime = double.MaxValue;
//    private int bestTimeIndex = -1;

//    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();

//    void Awake()
//    {
//        dbManager = FindObjectOfType<SQLiteDBScript>();
//        ResetScoresBasedOnLevel();
//    }

//    void Start()
//    {
//        InitializeUITexts();
//        UpdateAllScores();

//        // Feliratkoz�s a Simon pontsz�m v�ltoz�sokra
//        if (sm_scr != null)
//        {
//            sm_scr.onScoreChanged.AddListener(OnSimonScoreChanged);
//            sm_scr.onHighScoreChanged.AddListener(OnSimonHighScoreChanged);
//        }

//        // Debug log - ellen�rizz�k, hogy a ScoreManager megtal�lta-e az �sszes sz�ks�ges komponenst
//        Debug.Log($"ScoreManager initialized with: objectSpawner={objectSpawner != null}, gunscript={gunscript != null}, dbManager={dbManager != null}");
//    }

//    // Simon pontsz�m v�ltoz�s kezel�se
//    private void OnSimonScoreChanged(int newScore)
//    {
//        if (simonCurrentScore != null)
//        {
//            simonCurrentScore.SetText($"Current Score: {newScore}");
//        }
//    }

//    // Simon highscore v�ltoz�s kezel�se
//    private void OnSimonHighScoreChanged(int newHighScore)
//    {
//        UpdateSimonScore();
//    }

//    private void ResetScoresBasedOnLevel()
//    {
//        int currentLevel = NextGameColliderScript.GetCurrentLevel();

//        // A jelenlegi szint el�tti j�t�kok eredm�nyeit megtartjuk,
//        // a mostani �s k�vetkez� j�t�kok eredm�nyeit null�zzuk

//        if (currentLevel <= 0) // Simon j�t�k
//        {
//            if (sm_scr != null)
//                sm_scr.ResetScore();
//        }

//        if (currentLevel <= 1) // Labirintus
//        {
//            if (maze_scr != null)
//                maze_scr.ResetScore();
//        }

//        if (currentLevel <= 2) // L�v�ld�z�s j�t�k
//        {
//            if (objectSpawner != null)
//            {
//                objectSpawner.hit_times?.Clear();
//                objectSpawner.hitPlace_fromMiddle?.Clear();
//            }
//            shootingTimes.Clear();
//            hitPositions.Clear();
//            bestTime = double.MaxValue;
//            bestTimeIndex = -1;
//            hitpoints.Clear();
//        }
//    }

//    private void InitializeUITexts()
//    {
//        int currentLevel = NextGameColliderScript.GetCurrentLevel();

//        if (shootingScore != null)
//            shootingScore.text = currentLevel > 2 ? shootingScore.text : "No hits recorded yet";

//        if (simonScore != null)
//            simonScore.text = currentLevel > 0 ? simonScore.text : "Simon High Score: 0";

//        if (simonCurrentScore != null)
//            simonCurrentScore.text = currentLevel > 0 ? simonCurrentScore.text : "Current Score: 0";

//        if (mazeScore != null)
//            mazeScore.text = currentLevel > 1 ? mazeScore.text : "Maze Score: 0";
//    }

//    void Update()
//    {
//        if (objectSpawner != null && objectSpawner.hit_times != null && shootingScore != null)
//        {
//            // Ellen�rizz�k, hogy v�ltozott-e a tal�latok sz�ma
//            if (objectSpawner.hit_times.Count != shootingTimes.Count)
//            {
//                Debug.Log($"Hit times count changed: {objectSpawner.hit_times.Count} vs {shootingTimes.Count}");
//                UpdateShootingScores();
//            }

//            // Ellen�rizz�k a hitPlace_fromMiddle list�t is
//            if (objectSpawner.hitPlace_fromMiddle != null &&
//                objectSpawner.hitPlace_fromMiddle.Count != hitPositions.Count)
//            {
//                Debug.Log($"Hit positions count changed: {objectSpawner.hitPlace_fromMiddle.Count} vs {hitPositions.Count}");
//                UpdateShootingScores();
//            }
//        }
//    }

//    private void UpdateAllScores()
//    {
//        UpdateSimonScore();
//        UpdateMazeScore();
//        UpdateShootingScores();
//    }

//    private void UpdateSimonScore()
//    {
//        if (simonScore != null && sm_scr != null)
//        {
//            int currentHighScore = sm_scr.GetHighScore();
//            simonScore.SetText($"Simon High Score: {currentHighScore}");

//            // Aktu�lis pontsz�m friss�t�se
//            if (simonCurrentScore != null)
//            {
//                int currentScore = sm_scr.GetCurrentScore();
//                simonCurrentScore.SetText($"Current Score: {currentScore}");
//            }

//            // Adatb�zis friss�t�se csak ha teljes�tette a j�t�kot
//            if (dbManager != null && FindObjectOfType<SimonGameManager>() != null &&
//                FindObjectOfType<SimonGameManager>().isEnded)
//            {
//                dbManager.UpdateSimonScore(currentHighScore);
//            }
//        }
//    }
//    private void UpdateMazeScore()
//    {
//        if (mazeScore != null && maze_scr != null)
//        {
//            // Format the time for display
//            string formattedTime = string.Format("{0:mm\\:ss\\.ff}", maze_scr.score_time);
//            mazeScore.SetText($"Maze Score: {formattedTime}");

//            // Calculate time in minutes for database (original format)
//            double currentTime = maze_scr.score_time.TotalMinutes;

//            // Check if we have a valid time
//            if (dbManager != null && maze_scr.score_time.TotalSeconds > 0)
//            {
//                Debug.Log($"Updating maze score in database: {formattedTime}");
//                // Pass both the numeric value and the formatted string
//                dbManager.UpdateMazeTime(currentTime, formattedTime);
//            }
//        }
//    }

//    private void UpdateShootingScores()
//    {
//        if (objectSpawner == null || shootingScore == null)
//        {
//            Debug.LogWarning("UpdateShootingScores: objectSpawner or shootingScore is null");
//            return;
//        }

//        try
//        {
//            // Adatok m�sol�sa �s ellen�rz�se
//            if (objectSpawner.hit_times != null)
//            {
//                shootingTimes = new List<double>(objectSpawner.hit_times);
//                Debug.Log($"Copied {shootingTimes.Count} hit times from objectSpawner");
//            }
//            else
//            {
//                Debug.LogError("objectSpawner.hit_times is null!");
//            }

//            if (objectSpawner.hitPlace_fromMiddle != null)
//            {
//                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
//                Debug.Log($"Copied {hitPositions.Count} hit positions from objectSpawner");

//                // Debug: Minden poz�ci� ki�r�sa
//                for (int i = 0; i < hitPositions.Count; i++)
//                {
//                    Debug.Log($"Hit position {i}: {hitPositions[i]}");
//                }
//            }
//            else
//            {
//                Debug.LogError("objectSpawner.hitPlace_fromMiddle is null!");
//            }

//            if (shootingTimes.Count == 0)
//            {
//                shootingScore.SetText("No hits recorded yet");
//                return;
//            }

//            double currentBestTime = shootingTimes.Min();
//            if (currentBestTime < bestTime)
//            {
//                bestTime = currentBestTime;
//                bestTimeIndex = shootingTimes.IndexOf(bestTime);
//                Debug.Log($"New best time: {bestTime} at index {bestTimeIndex}");
//            }

//            // Format the display text with proper position values
//            StringBuilder displayText = new StringBuilder();
//            displayText.AppendLine($"Best Shot:");
//            displayText.AppendLine($"Time: {bestTime:F2} sec");

//            if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
//            {
//                displayText.AppendLine($"Position: {hitPositions[bestTimeIndex]}");
//                Debug.Log($"Best shot position: {hitPositions[bestTimeIndex]}");
//            }
//            else
//            {
//                displayText.AppendLine("Position: N/A");
//                Debug.LogWarning($"Best shot position unavailable. bestTimeIndex={bestTimeIndex}, hitPositions.Count={hitPositions.Count}");
//            }

//            displayText.AppendLine("\nLast Shot:");
//            displayText.AppendLine($"Time: {shootingTimes[shootingTimes.Count - 1]:F2} sec");

//            if (hitPositions.Count > 0)
//            {
//                displayText.AppendLine($"Position: {hitPositions[hitPositions.Count - 1]}");
//                Debug.Log($"Last shot position: {hitPositions[hitPositions.Count - 1]}");
//            }
//            else
//            {
//                displayText.AppendLine("Position: N/A");
//                Debug.LogWarning("Last shot position unavailable. hitPositions is empty.");
//            }

//            displayText.AppendLine($"\nTotal Hits: {shootingTimes.Count}");

//            shootingScore.SetText(displayText.ToString());
//            Debug.Log($"Updated shooting score text: {shootingScore.text}");

//            // Update database with each shot
//            if (dbManager != null)
//            {
//                for (int i = 0; i < shootingTimes.Count; i++)
//                {
//                    if (i < hitPositions.Count)
//                    {
//                        string[] coordinates = hitPositions[i].Split('|');
//                        if (coordinates.Length == 2)
//                        {
//                            if (double.TryParse(coordinates[0], out double hitX) &&
//                                double.TryParse(coordinates[1], out double hitY))
//                            {
//                                Debug.Log($"Sending to database: shot {i + 1}, time={shootingTimes[i]}, X={hitX}, Y={hitY}");
//                                dbManager.UpdateShootingScore(i + 1, shootingTimes[i], hitX, hitY);
//                            }
//                            else
//                            {
//                                Debug.LogError($"Failed to parse coordinates: {hitPositions[i]}");
//                            }
//                        }
//                        else
//                        {
//                            Debug.LogError($"Invalid coordinate format: {hitPositions[i]}, expected format: X,Y");
//                        }
//                    }
//                    else
//                    {
//                        Debug.LogWarning($"Missing position data for shot {i + 1}");
//                    }
//                }
//            }
//            else
//            {
//                Debug.LogError("dbManager is null, cannot update database!");
//            }
//        }
//        catch (System.Exception e)
//        {
//            Debug.LogError($"Error in UpdateShootingScores: {e.Message}\n{e.StackTrace}");
//            shootingScore.SetText("Score updating error...");
//        }
//    }

//    public void RefreshScores()
//    {
//        Debug.Log("RefreshScores called");
//        UpdateAllScores();
//    }

//    public void RestartCurrentGame()
//    {
//        Debug.Log("RestartCurrentGame called");
//        ResetScoresBasedOnLevel();
//        InitializeUITexts();
//        UpdateAllScores();
//    }
//}


//NEW DB SCORE MANAGER

using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;
using System.Text;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI simonScore;
    [SerializeField] private TextMeshProUGUI shootingScore;
    [SerializeField] private TextMeshProUGUI mazeScore;

    // �j UI elem az aktu�lis Simon pontsz�mhoz
    [SerializeField] private TextMeshProUGUI simonCurrentScore;

    [Header("Game References")]
    [SerializeField] private SimonScores sm_scr;
    [SerializeField] private ObjectSpawner_1place objectSpawner;
    [SerializeField] private ButtonsForMaze maze_scr;
    [SerializeField] private Gun gunscript;

    private SQLiteDBScript dbManager;
    private List<double> shootingTimes = new List<double>();
    private List<string> hitPositions = new List<string>();
    private double bestTime = double.MaxValue;
    private int bestTimeIndex = -1;

    private static int lastSavedShotIndex = -1;

    List<Tuple<double, double>> hitpoints = new List<Tuple<double, double>>();


    void Awake()
    {
        dbManager = FindObjectOfType<SQLiteDBScript>();

        // Ellen�rizz�k, hogy van-e �rv�nyes j�t�kos azonos�t�
        if (dbManager != null && dbManager.GetCurrentPlayerID() <= 0)
        {
            if (PlayerPrefs.HasKey("CurrentPlayerID"))
            {
                int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
                dbManager.SetCurrentPlayerID(savedPlayerID);
                Debug.Log($"ScoreManager restored player ID from PlayerPrefs: {savedPlayerID}");
            }
        }

        ResetScoresBasedOnLevel();
    }

    void Start()
    {
        InitializeUITexts();
        UpdateAllScores();

        // Feliratkoz�s a Simon pontsz�m v�ltoz�sokra
        if (sm_scr != null)
        {
            sm_scr.onScoreChanged.AddListener(OnSimonScoreChanged);
            sm_scr.onHighScoreChanged.AddListener(OnSimonHighScoreChanged);
        }

        // Debug log - ellen�rizz�k, hogy a ScoreManager megtal�lta-e az �sszes sz�ks�ges komponenst
        Debug.Log($"ScoreManager initialized with: objectSpawner={objectSpawner != null}, gunscript={gunscript != null}, dbManager={dbManager != null}");
    }

    // Simon pontsz�m v�ltoz�s kezel�se
    private void OnSimonScoreChanged(int newScore)
    {
        if (simonCurrentScore != null)
        {
            simonCurrentScore.SetText($"Current Score: {newScore}");
        }
    }

    // Simon highscore v�ltoz�s kezel�se
    private void OnSimonHighScoreChanged(int newHighScore)
    {
        UpdateSimonScore();
    }

    private void ResetScoresBasedOnLevel()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        // A jelenlegi szint el�tti j�t�kok eredm�nyeit megtartjuk,
        // a mostani �s k�vetkez� j�t�kok eredm�nyeit null�zzuk

        if (currentLevel <= 0) // Simon j�t�k
        {
            if (sm_scr != null)
                sm_scr.ResetScore();
        }

        if (currentLevel <= 1) // Labirintus
        {
            if (maze_scr != null)
                maze_scr.ResetScore();
        }

        if (currentLevel <= 2) // L�v�ld�z�s j�t�k
        {
            if (objectSpawner != null)
            {
                objectSpawner.hit_times?.Clear();
                objectSpawner.hitPlace_fromMiddle?.Clear();
            }
            shootingTimes.Clear();
            hitPositions.Clear();
            bestTime = double.MaxValue;
            bestTimeIndex = -1;
            hitpoints.Clear();
        }
    }

    private void InitializeUITexts()
    {
        int currentLevel = NextGameColliderScript.GetCurrentLevel();

        if (shootingScore != null)
            shootingScore.text = currentLevel > 2 ? shootingScore.text : "No hits recorded yet";

        if (simonScore != null)
            simonScore.text = currentLevel > 0 ? simonScore.text : "Simon High Score: 0";

        if (simonCurrentScore != null)
            simonCurrentScore.text = currentLevel > 0 ? simonCurrentScore.text : "Current Score: 0";

        if (mazeScore != null)
            mazeScore.text = currentLevel > 1 ? mazeScore.text : "Maze Score: 0";
    }

    void Update()
    {
        if (objectSpawner != null && objectSpawner.hit_times != null && shootingScore != null)
        {
            // Ellen�rizz�k, hogy v�ltozott-e a tal�latok sz�ma
            if (objectSpawner.hit_times.Count != shootingTimes.Count)
            {
                Debug.Log($"Hit times count changed: {objectSpawner.hit_times.Count} vs {shootingTimes.Count}");
                UpdateShootingScores();
            }

            // Ellen�rizz�k a hitPlace_fromMiddle list�t is
            if (objectSpawner.hitPlace_fromMiddle != null &&
                objectSpawner.hitPlace_fromMiddle.Count != hitPositions.Count)
            {
                Debug.Log($"Hit positions count changed: {objectSpawner.hitPlace_fromMiddle.Count} vs {hitPositions.Count}");
                UpdateShootingScores();
            }
        }
    }

    private void UpdateAllScores()
    {
        UpdateSimonScore();
        UpdateMazeScore();
        UpdateShootingScores();
    }

    private void UpdateSimonScore()
    {
        if (simonScore != null && sm_scr != null)
        {
            int currentHighScore = sm_scr.GetHighScore();
            simonScore.SetText($"Simon High Score: {currentHighScore}");

            // Aktu�lis pontsz�m friss�t�se
            if (simonCurrentScore != null)
            {
                int currentScore = sm_scr.GetCurrentScore();
                simonCurrentScore.SetText($"Current Score: {currentScore}");
            }

            // Adatb�zis friss�t�se csak ha teljes�tette a j�t�kot
            if (dbManager != null && FindObjectOfType<SimonGameManager>() != null &&
                FindObjectOfType<SimonGameManager>().isEnded)
            {
                // Ellen�rizz�k hogy van-e �rv�nyes j�t�kos azonos�t�
                if (dbManager.GetCurrentPlayerID() <= 0 && PlayerPrefs.HasKey("CurrentPlayerID"))
                {
                    int savedPlayerID = PlayerPrefs.GetInt("CurrentPlayerID");
                    dbManager.SetCurrentPlayerID(savedPlayerID);
                    Debug.Log($"Restored player ID before updating Simon score: {savedPlayerID}");
                }

                if (dbManager.GetCurrentPlayerID() > 0)
                {
                    dbManager.UpdateSimonScore(currentHighScore);
                    Debug.Log($"Simon pontsz�m sikeresen friss�tve: {currentHighScore}");
                }
                else
                {
                    Debug.LogWarning("Nem siker�lt a Simon pontsz�m friss�t�se: Nincs akt�v j�t�kos");
                }
            }
        }
    }


    private void UpdateMazeScore()
    {
        if (mazeScore != null && maze_scr != null)
        {
            // Format the time for display
            string formattedTime = string.Format("{0:mm\\:ss\\.ff}", maze_scr.score_time);
            mazeScore.SetText($"Maze Score: {formattedTime}");

            // Calculate time in minutes for database (original format)
            double currentTime = maze_scr.score_time.TotalMinutes;

            // Check if we have a valid time
            if (dbManager != null && maze_scr.score_time.TotalSeconds > 0)
            {
                Debug.Log($"Updating maze score in database: {formattedTime}");
                // Pass both the numeric value and the formatted string
                dbManager.UpdateMazeTime(currentTime, formattedTime);
            }
        }
    }

    private void UpdateShootingScores()
    {
        if (objectSpawner == null || shootingScore == null)
        {
            Debug.LogWarning("UpdateShootingScores: objectSpawner or shootingScore is null");
            return;
        }

        try
        {
            // Adatok m�sol�sa �s ellen�rz�se
            if (objectSpawner.hit_times != null)
            {
                shootingTimes = new List<double>(objectSpawner.hit_times);
                Debug.Log($"Copied {shootingTimes.Count} hit times from objectSpawner");
            }
            else
            {
                Debug.LogError("objectSpawner.hit_times is null!");
            }

            if (objectSpawner.hitPlace_fromMiddle != null)
            {
                hitPositions = new List<string>(objectSpawner.hitPlace_fromMiddle);
                Debug.Log($"Copied {hitPositions.Count} hit positions from objectSpawner");

                // Debug: Minden poz�ci� ki�r�sa
                for (int i = 0; i < hitPositions.Count; i++)
                {
                    Debug.Log($"Hit position {i}: {hitPositions[i]}");
                }
            }
            else
            {
                Debug.LogError("objectSpawner.hitPlace_fromMiddle is null!");
            }

            if (shootingTimes.Count == 0)
            {
                shootingScore.SetText("No hits recorded yet");
                return;
            }

            double currentBestTime = shootingTimes.Min();
            if (currentBestTime < bestTime)
            {
                bestTime = currentBestTime;
                bestTimeIndex = shootingTimes.IndexOf(bestTime);
                Debug.Log($"New best time: {bestTime} at index {bestTimeIndex}");
            }

            // Format the display text with proper position values
            StringBuilder displayText = new StringBuilder();
            displayText.AppendLine($"Best Shot:");
            displayText.AppendLine($"Time: {bestTime:F2} sec");

            if (bestTimeIndex >= 0 && bestTimeIndex < hitPositions.Count)
            {
                displayText.AppendLine($"Position: {hitPositions[bestTimeIndex]}");
                Debug.Log($"Best shot position: {hitPositions[bestTimeIndex]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
                Debug.LogWarning($"Best shot position unavailable. bestTimeIndex={bestTimeIndex}, hitPositions.Count={hitPositions.Count}");
            }

            displayText.AppendLine("\nLast Shot:");
            displayText.AppendLine($"Time: {shootingTimes[shootingTimes.Count - 1]:F2} sec");

            if (hitPositions.Count > 0)
            {
                displayText.AppendLine($"Position: {hitPositions[hitPositions.Count - 1]}");
                Debug.Log($"Last shot position: {hitPositions[hitPositions.Count - 1]}");
            }
            else
            {
                displayText.AppendLine("Position: N/A");
                Debug.LogWarning("Last shot position unavailable. hitPositions is empty.");
            }

            displayText.AppendLine($"\nTotal Hits: {shootingTimes.Count}");
            displayText.AppendLine($"Session ID: {dbManager.GetCurrentShootingSessionID()}");
            displayText.AppendLine($"Player ID: {dbManager.GetCurrentPlayerID()}");

            shootingScore.SetText(displayText.ToString());
            Debug.Log($"Updated shooting score text: {shootingScore.text}");

            // Update database with most recent shot
            if (dbManager != null)
            {
                // CSAK AKKOR FRISS�TS�NK, HA �J L�V�S T�RT�NT
                int lastIndex = shootingTimes.Count - 1;

                if (lastIndex > lastSavedShotIndex && lastIndex >= 0 && lastIndex < hitPositions.Count)
                {
                    string[] coordinates = hitPositions[lastIndex].Split('|');
                    if (coordinates.Length == 2)
                    {
                        if (double.TryParse(coordinates[0], out double hitX) &&
                            double.TryParse(coordinates[1], out double hitY))
                        {
                            Debug.Log($"Sending to database: shot {lastIndex + 1}, time={shootingTimes[lastIndex]}, X={hitX}, Y={hitY}");
                            bool success = dbManager.UpdateShootingScore(lastIndex + 1, shootingTimes[lastIndex], hitX, hitY);
                            if (success)
                            {
                                lastSavedShotIndex = lastIndex;
                            }
                        }
                        else
                        {
                            Debug.LogError($"Failed to parse coordinates: {hitPositions[lastIndex]}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Invalid coordinate format: {hitPositions[lastIndex]}, expected format: X,Y");
                    }
                }
                else
                {
                    Debug.Log($"Shot already saved or missing position data for shot {lastIndex + 1}");
                }
            }
            else
            {
                Debug.LogError("dbManager is null, cannot update database!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in UpdateShootingScores: {e.Message}\n{e.StackTrace}");
            shootingScore.SetText("Score updating error...");
        }
    }

    // Ez a met�dus megh�vhat�, amikor �j j�t�kos j�tszik
    public void StartNewShootingSession()
    {
        if (dbManager != null)
        {
            int currentPlayerID = dbManager.GetCurrentPlayerID();
            if (currentPlayerID > 0)
            {
                dbManager.StartNewShootingSession(currentPlayerID);
                Debug.Log($"Started new shooting session for player {currentPlayerID}");
            }
            else
            {
                Debug.LogWarning("Cannot start new shooting session: No current player");
            }
        }
    }

    // Ha egy �j j�t�kos v�lasztotta ki a j�t�kot, ezt a met�dust kell megh�vni
    public void OnNewPlayerSelected(int playerID)
    {
        if (dbManager != null)
        {
            dbManager.SetCurrentPlayerID(playerID);
            Debug.Log($"Set current player to {playerID}");
        }
    }

    public void RefreshScores()
    {
        Debug.Log("RefreshScores called");
        UpdateAllScores();
    }

    public void RestartCurrentGame()
    {
        Debug.Log("RestartCurrentGame called");
        ResetScoresBasedOnLevel();
        InitializeUITexts();
        UpdateAllScores();
    }
}
