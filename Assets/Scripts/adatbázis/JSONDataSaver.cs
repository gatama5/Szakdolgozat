//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using System.Text;
//using System.Data;

//public class JSONDataSaver : MonoBehaviour
//{
//    [Header("Settings")]
//    [SerializeField] private string backupFolderName = "PlayerDataBackup";
//    [SerializeField] private bool autoBackupOnGameEnd = true;
//    [SerializeField] private bool logDebugMessages = true;
//    [SerializeField] private bool backupAfterMazeGame = true;

//    private string backupFolderPath;
//    private SQLiteDBScript dbManager;
//    private ScoreManager scoreManager;

//    // Class to structure the player data for JSON serialization - Updated structure
//    [Serializable]
//    private class PlayerData
//    {
//        public int playerId;
//        public string playerName;
//        public int playerAge;
//        public string playerEmail;
//        public string playerAgeGeneration;
//        public SimonGameData simonData;
//        public MazeGameData mazeData;
//        public List<ShootingSessionData> shootingSessions;
//    }

//    [Serializable]
//    private class SimonGameData
//    {
//        public int highScore;
//        public string recordedAt;
//    }

//    [Serializable]
//    private class MazeGameData
//    {
//        public double completionTime;
//        public string formattedTime;
//        public string recordedAt;
//    }

//    [Serializable]
//    private class ShootingSessionData
//    {
//        public int sessionId;
//        public string startedAt;
//        public List<ShootingData> shots;
//    }

//    [Serializable]
//    private class ShootingData
//    {
//        public int shotNumber;
//        public double reactionTime;
//        public double positionX;
//        public double positionY;
//        public string recordedAt;
//    }

//    // Wrapper class for JSON serialization of player list
//    [Serializable]
//    private class AllPlayersWrapper
//    {
//        public List<PlayerData> players;
//    }

//    private void Awake()
//    {
//        // Create the backup folder path
//        backupFolderPath = Path.Combine(Application.persistentDataPath, backupFolderName);

//        // Ensure the backup directory exists
//        if (!Directory.Exists(backupFolderPath))
//        {
//            Directory.CreateDirectory(backupFolderPath);
//        }

//        // Get reference to the database manager
//        dbManager = FindObjectOfType<SQLiteDBScript>();
//        scoreManager = FindObjectOfType<ScoreManager>();

//        if (dbManager == null)
//        {
//            LogMessage("WARNING: SQLiteDBScript not found in scene!", true);
//        }
//        else
//        {
//            LogMessage("JSONDataSaver initialized successfully");
//        }
//    }

//    private void OnApplicationQuit()
//    {
//        if (autoBackupOnGameEnd)
//        {
//            BackupCurrentPlayerData();
//        }
//    }

//    // Public method that can be called to save the current player's data
//    public void BackupCurrentPlayerData()
//    {
//        if (dbManager == null)
//        {
//            LogMessage("Cannot backup: No database manager found!", true);
//            return;
//        }

//        int currentPlayerId = dbManager.GetCurrentPlayerID();
//        if (currentPlayerId <= 0)
//        {
//            LogMessage("Cannot backup: No current player ID found!", true);
//            return;
//        }

//        BackupPlayerData(currentPlayerId);
//    }

//    // This method listens for maze game completion if needed
//    public void OnMazeGameCompleted()
//    {
//        if (backupAfterMazeGame)
//        {
//            LogMessage("Maze game completed, backing up player data...");
//            BackupCurrentPlayerData();
//        }
//    }

//    // This function should be called from the ButtonsForMaze.cs script when the maze is completed
//    // You would need to add: FindObjectOfType<JSONDataSaver>().OnMazeGameCompleted();
//    // to the maze completion handler in ButtonsForMaze.cs

//    // Backs up a specific player's data - UPDATED for new DB structure
//    public void BackupPlayerData(int playerId)
//    {
//        try
//        {
//            // Get the player's basic data
//            DataTable playerBasicInfo = GetPlayerBasicInfo(playerId);

//            if (playerBasicInfo == null || playerBasicInfo.Rows.Count == 0)
//            {
//                LogMessage($"No data found for player ID: {playerId}", true);
//                return;
//            }

//            // Create a new PlayerData object
//            var playerData = new PlayerData
//            {
//                playerId = playerId,
//                playerName = playerBasicInfo.Rows[0]["Name"].ToString(),
//                playerAge = 0, // Default values, will be set if details exist
//                playerEmail = "",
//                playerAgeGeneration = "",
//                simonData = new SimonGameData(),
//                mazeData = new MazeGameData(),
//                shootingSessions = new List<ShootingSessionData>()
//            };

//            // Get player details if available
//            DataTable playerDetails = GetPlayerDetails(playerId);
//            if (playerDetails != null && playerDetails.Rows.Count > 0)
//            {
//                var detailRow = playerDetails.Rows[0];

//                if (detailRow["Age"] != DBNull.Value)
//                    playerData.playerAge = Convert.ToInt32(detailRow["Age"]);

//                if (detailRow["Email"] != DBNull.Value)
//                    playerData.playerEmail = detailRow["Email"].ToString();

//                if (detailRow["Generation"] != DBNull.Value)
//                    playerData.playerAgeGeneration = detailRow["Generation"].ToString();
//            }

//            // Get Simon game scores
//            DataTable simonScores = GetSimonScores(playerId);
//            if (simonScores != null && simonScores.Rows.Count > 0)
//            {
//                // Get the highest score
//                int highestScore = 0;
//                string recordedAt = "";

//                foreach (DataRow row in simonScores.Rows)
//                {
//                    int score = Convert.ToInt32(row["Score"]);
//                    if (score > highestScore)
//                    {
//                        highestScore = score;
//                        recordedAt = row["RecordedAt"].ToString();
//                    }
//                }

//                playerData.simonData.highScore = highestScore;
//                playerData.simonData.recordedAt = recordedAt;
//            }

//            // Get Maze completion times
//            DataTable mazeScores = GetMazeScores(playerId);
//            if (mazeScores != null && mazeScores.Rows.Count > 0)
//            {
//                // Get the best (lowest) completion time
//                double bestTime = double.MaxValue;
//                string formattedTime = "";
//                string recordedAt = "";

//                foreach (DataRow row in mazeScores.Rows)
//                {
//                    if (row["CompletionTime"] != DBNull.Value)
//                    {
//                        double time = Convert.ToDouble(row["CompletionTime"]);
//                        if (time < bestTime && time > 0)
//                        {
//                            bestTime = time;
//                            formattedTime = row["FormattedTime"].ToString();
//                            recordedAt = row["RecordedAt"].ToString();
//                        }
//                    }
//                }

//                if (bestTime < double.MaxValue)
//                {
//                    playerData.mazeData.completionTime = bestTime;
//                    playerData.mazeData.formattedTime = formattedTime;
//                    playerData.mazeData.recordedAt = recordedAt;
//                }
//            }

//            // Get Shooting sessions and scores
//            DataTable shootingSessions = GetShootingSessions(playerId);
//            if (shootingSessions != null && shootingSessions.Rows.Count > 0)
//            {
//                foreach (DataRow sessionRow in shootingSessions.Rows)
//                {
//                    int sessionId = Convert.ToInt32(sessionRow["SessionID"]);

//                    var sessionData = new ShootingSessionData
//                    {
//                        sessionId = sessionId,
//                        startedAt = sessionRow["StartedAt"].ToString(),
//                        shots = new List<ShootingData>()
//                    };

//                    // Get all shots for this session
//                    DataTable shootingScores = GetShootingScores(sessionId);
//                    if (shootingScores != null && shootingScores.Rows.Count > 0)
//                    {
//                        foreach (DataRow shotRow in shootingScores.Rows)
//                        {
//                            var shotData = new ShootingData
//                            {
//                                shotNumber = Convert.ToInt32(shotRow["ShotNumber"]),
//                                reactionTime = Convert.ToDouble(shotRow["ReactionTime"]),
//                                positionX = Convert.ToDouble(shotRow["PositionX"]),
//                                positionY = Convert.ToDouble(shotRow["PositionY"]),
//                                recordedAt = shotRow["RecordedAt"].ToString()
//                            };

//                            sessionData.shots.Add(shotData);
//                        }
//                    }

//                    playerData.shootingSessions.Add(sessionData);
//                }
//            }

//            // Save the player data to a JSON file
//            string fileName = $"Player_{playerId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
//            string filePath = Path.Combine(backupFolderPath, fileName);

//            // Convert to JSON
//            string json = JsonUtility.ToJson(playerData, true);
//            File.WriteAllText(filePath, json, Encoding.UTF8);

//            LogMessage($"Player data successfully backed up to: {filePath}");

//            // Create a summary file that combines all data for easier viewing
//            CreateSummaryFile(playerData, playerId);
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error backing up player data: {ex.Message}", true);
//        }
//    }

//    // Helper methods to get data from the database
//    private DataTable GetPlayerBasicInfo(int playerId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM Players WHERE PlayerID = @playerId";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@playerId";
//                    param.Value = playerId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting player basic info: {ex.Message}", true);
//            return null;
//        }
//    }

//    private DataTable GetPlayerDetails(int playerId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM PlayerDetails WHERE PlayerID = @playerId";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@playerId";
//                    param.Value = playerId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting player details: {ex.Message}", true);
//            return null;
//        }
//    }

//    private DataTable GetSimonScores(int playerId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM SimonScores WHERE PlayerID = @playerId ORDER BY RecordedAt DESC";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@playerId";
//                    param.Value = playerId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting Simon scores: {ex.Message}", true);
//            return null;
//        }
//    }

//    private DataTable GetMazeScores(int playerId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM MazeScores WHERE PlayerID = @playerId ORDER BY RecordedAt DESC";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@playerId";
//                    param.Value = playerId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting maze scores: {ex.Message}", true);
//            return null;
//        }
//    }

//    private DataTable GetShootingSessions(int playerId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM ShootingSessions WHERE PlayerID = @playerId ORDER BY StartedAt DESC";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@playerId";
//                    param.Value = playerId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting shooting sessions: {ex.Message}", true);
//            return null;
//        }
//    }

//    private DataTable GetShootingScores(int sessionId)
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT * FROM ShootingScores WHERE SessionID = @sessionId ORDER BY ShotNumber";

//                    var param = cmd.CreateParameter();
//                    param.ParameterName = "@sessionId";
//                    param.Value = sessionId;
//                    cmd.Parameters.Add(param);

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting shooting scores: {ex.Message}", true);
//            return null;
//        }
//    }

//    // Creates a summary file with all players' data for easier viewing
//    private void CreateSummaryFile(PlayerData newData, int playerId)
//    {
//        try
//        {
//            string summaryPath = Path.Combine(backupFolderPath, "AllPlayersData.json");
//            List<PlayerData> allPlayers = new List<PlayerData>();

//            // Read existing data if available
//            if (File.Exists(summaryPath))
//            {
//                string existingJson = File.ReadAllText(summaryPath);

//                // Unity's JsonUtility doesn't directly support deserializing JSON arrays,
//                // so we need a wrapper class to deserialize the data
//                AllPlayersWrapper wrapper = JsonUtility.FromJson<AllPlayersWrapper>(existingJson);
//                if (wrapper != null && wrapper.players != null)
//                {
//                    allPlayers = wrapper.players;

//                    // Remove any existing entry for this player
//                    allPlayers.RemoveAll(p => p.playerId == playerId);
//                }
//            }

//            // Add the new player data
//            allPlayers.Add(newData);

//            // Create a wrapper to serialize the list
//            AllPlayersWrapper newWrapper = new AllPlayersWrapper { players = allPlayers };
//            string json = JsonUtility.ToJson(newWrapper, true);

//            // Write to the summary file
//            File.WriteAllText(summaryPath, json, Encoding.UTF8);

//            LogMessage($"Updated summary file with latest player data: {summaryPath}");
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error creating summary file: {ex.Message}", true);
//        }
//    }

//    // Helper method for logging with optional error flag
//    private void LogMessage(string message, bool isError = false)
//    {
//        if (logDebugMessages)
//        {
//            if (isError)
//                Debug.LogError($"[JSONDataSaver] {message}");

//        }
//    }

//    // Manual backup method that can be called from UI buttons
//    public void BackupAllPlayersData()
//    {
//        try
//        {
//            // Get all player IDs from database
//            DataTable allPlayers = GetAllPlayers();

//            if (allPlayers != null && allPlayers.Rows.Count > 0)
//            {
//                LogMessage($"Starting backup for {allPlayers.Rows.Count} players...");

//                foreach (DataRow row in allPlayers.Rows)
//                {
//                    int playerId = Convert.ToInt32(row["PlayerID"]);
//                    BackupPlayerData(playerId);
//                }

//                LogMessage("All players backup completed successfully");
//            }
//            else
//            {
//                LogMessage("No players found to backup", true);
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error backing up all players: {ex.Message}", true);
//        }
//    }

//    private DataTable GetAllPlayers()
//    {
//        try
//        {
//            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
//            {
//                connection.Open();
//                using (var cmd = connection.CreateCommand())
//                {
//                    cmd.CommandText = "SELECT PlayerID FROM Players";

//                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
//                    var table = new DataTable();
//                    adapter.Fill(table);
//                    return table;
//                }
//            }
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error getting all players: {ex.Message}", true);
//            return null;
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text;
using System.Data;

public class JSONDataSaver : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string backupFolderName = "PlayerDataBackup";
    [SerializeField] private bool autoBackupOnGameEnd = true;
    [SerializeField] private bool logDebugMessages = true;
    [SerializeField] private bool backupAfterMazeGame = true;

    private string backupFolderPath;
    private SQLiteDBScript dbManager;
    private ScoreManager scoreManager;

    // Class to structure the player data for JSON serialization - Updated structure
    [Serializable]
    private class PlayerData
    {
        // Az playerId most már YYYYMMDDHHMM formátumú dátumidõbélyeg
        public int playerId;

        public int playerAge;

        public string playerAgeGeneration;
        public SimonGameData simonData;
        public MazeGameData mazeData;
        public List<ShootingSessionData> shootingSessions;
    }

    [Serializable]
    private class SimonGameData
    {
        public int highScore;
        public string recordedAt;
    }

    [Serializable]
    private class MazeGameData
    {
        public double completionTime;
        public string formattedTime;
        public string recordedAt;
    }

    [Serializable]
    private class ShootingSessionData
    {
        public int sessionId;
        public string startedAt;
        public List<ShootingData> shots;
    }

    [Serializable]
    private class ShootingData
    {
        public int shotNumber;
        public double reactionTime;
        public double positionX;
        public double positionY;
        public string recordedAt;
    }

    // Wrapper class for JSON serialization of player list
    [Serializable]
    private class AllPlayersWrapper
    {
        public List<PlayerData> players;
    }

    private void Awake()
    {
        // Create the backup folder path
        backupFolderPath = Path.Combine(Application.persistentDataPath, backupFolderName);

        // Ensure the backup directory exists
        if (!Directory.Exists(backupFolderPath))
        {
            Directory.CreateDirectory(backupFolderPath);
        }

        // Get reference to the database manager
        dbManager = FindObjectOfType<SQLiteDBScript>();
        scoreManager = FindObjectOfType<ScoreManager>();

        if (dbManager == null)
        {
            LogMessage("WARNING: SQLiteDBScript not found in scene!", true);
        }
        else
        {
            LogMessage("JSONDataSaver initialized successfully");
        }
    }

    private void OnApplicationQuit()
    {
        if (autoBackupOnGameEnd)
        {
            BackupCurrentPlayerData();
        }
    }

    // Public method that can be called to save the current player's data
    public void BackupCurrentPlayerData()
    {
        if (dbManager == null)
        {
            LogMessage("Cannot backup: No database manager found!", true);
            return;
        }

        int currentPlayerId = dbManager.GetCurrentPlayerID();
        if (currentPlayerId <= 0)
        {
            LogMessage("Cannot backup: No current player ID found!", true);
            return;
        }

        BackupPlayerData(currentPlayerId);
    }

    // This method listens for maze game completion if needed
    public void OnMazeGameCompleted()
    {
        if (backupAfterMazeGame)
        {
            LogMessage("Maze game completed, backing up player data...");
            BackupCurrentPlayerData();
        }
    }

    // This function should be called from the ButtonsForMaze.cs script when the maze is completed
    // You would need to add: FindObjectOfType<JSONDataSaver>().OnMazeGameCompleted();
    // to the maze completion handler in ButtonsForMaze.cs

    // Backs up a specific player's data - UPDATED for new DB structure
    // Backs up a specific player's data - UPDATED for date-based player ID format
    public void BackupPlayerData(int playerId)
    {
        try
        {
            // Jelenítsük meg a dátum formátumú ID-t olvasható formában (csak logoláshoz)
            string playerIdStr = playerId.ToString();
            if (playerIdStr.Length == 12) // YYYYMMDDHHMM formátum esetén
            {
                string year = playerIdStr.Substring(0, 4);
                string month = playerIdStr.Substring(4, 2);
                string day = playerIdStr.Substring(6, 2);
                string hour = playerIdStr.Substring(8, 2);
                string minute = playerIdStr.Substring(10, 2);
                LogMessage($"Backing up data for player ID: {playerId} (created on {year}-{month}-{day} {hour}:{minute})");
            }
            else
            {
                LogMessage($"Backing up data for player ID: {playerId}");
            }

            // Get the player's basic data
            DataTable playerBasicInfo = GetPlayerBasicInfo(playerId);

            if (playerBasicInfo == null || playerBasicInfo.Rows.Count == 0)
            {
                LogMessage($"No data found for player ID: {playerId}", true);
                return;
            }

            // Create a new PlayerData object
            var playerData = new PlayerData
            {
                playerId = playerId,
                // playerName eltávolítva
                playerAge = 0,
                playerAgeGeneration = "",
                simonData = new SimonGameData(),
                mazeData = new MazeGameData(),
                shootingSessions = new List<ShootingSessionData>()
            };

            // Get player details if available
            DataTable playerDetails = GetPlayerDetails(playerId);
            if (playerDetails != null && playerDetails.Rows.Count > 0)
            {
                var detailRow = playerDetails.Rows[0];

                if (detailRow["Age"] != DBNull.Value)
                    playerData.playerAge = Convert.ToInt32(detailRow["Age"]);


                if (detailRow["Generation"] != DBNull.Value)
                    playerData.playerAgeGeneration = detailRow["Generation"].ToString();
            }

            // Get Simon game scores
            DataTable simonScores = GetSimonScores(playerId);
            if (simonScores != null && simonScores.Rows.Count > 0)
            {
                // Get the highest score
                int highestScore = 0;
                string recordedAt = "";

                foreach (DataRow row in simonScores.Rows)
                {
                    int score = Convert.ToInt32(row["Score"]);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        recordedAt = row["RecordedAt"].ToString();
                    }
                }

                playerData.simonData.highScore = highestScore;
                playerData.simonData.recordedAt = recordedAt;
            }

            // Get Maze completion times
            DataTable mazeScores = GetMazeScores(playerId);
            if (mazeScores != null && mazeScores.Rows.Count > 0)
            {
                // Get the best (lowest) completion time
                double bestTime = double.MaxValue;
                string formattedTime = "";
                string recordedAt = "";

                foreach (DataRow row in mazeScores.Rows)
                {
                    if (row["CompletionTime"] != DBNull.Value)
                    {
                        double time = Convert.ToDouble(row["CompletionTime"]);
                        if (time < bestTime && time > 0)
                        {
                            bestTime = time;
                            formattedTime = row["FormattedTime"].ToString();
                            recordedAt = row["RecordedAt"].ToString();
                        }
                    }
                }

                if (bestTime < double.MaxValue)
                {
                    playerData.mazeData.completionTime = bestTime;
                    playerData.mazeData.formattedTime = formattedTime;
                    playerData.mazeData.recordedAt = recordedAt;
                }
            }

            // Get Shooting sessions and scores
            DataTable shootingSessions = GetShootingSessions(playerId);
            if (shootingSessions != null && shootingSessions.Rows.Count > 0)
            {
                foreach (DataRow sessionRow in shootingSessions.Rows)
                {
                    int sessionId = Convert.ToInt32(sessionRow["SessionID"]);

                    var sessionData = new ShootingSessionData
                    {
                        sessionId = sessionId,
                        startedAt = sessionRow["StartedAt"].ToString(),
                        shots = new List<ShootingData>()
                    };

                    // Get all shots for this session
                    DataTable shootingScores = GetShootingScores(sessionId);
                    if (shootingScores != null && shootingScores.Rows.Count > 0)
                    {
                        foreach (DataRow shotRow in shootingScores.Rows)
                        {
                            var shotData = new ShootingData
                            {
                                shotNumber = Convert.ToInt32(shotRow["ShotNumber"]),
                                reactionTime = Convert.ToDouble(shotRow["ReactionTime"]),
                                positionX = Convert.ToDouble(shotRow["PositionX"]),
                                positionY = Convert.ToDouble(shotRow["PositionY"]),
                                recordedAt = shotRow["RecordedAt"].ToString()
                            };

                            sessionData.shots.Add(shotData);
                        }
                    }

                    playerData.shootingSessions.Add(sessionData);
                }
            }

            // Fájlnév formátumának módosítása - megtartjuk a dátum alapú játékos ID-t is
            string fileName = $"Player_{playerId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(backupFolderPath, fileName);

            // Convert to JSON
            string json = JsonUtility.ToJson(playerData, true);
            File.WriteAllText(filePath, json, Encoding.UTF8);

            LogMessage($"Player data successfully backed up to: {filePath}");

            // Create a summary file that combines all data for easier viewing
            CreateSummaryFile(playerData, playerId);
        }
        catch (Exception ex)
        {
            LogMessage($"Error backing up player data: {ex.Message}", true);
        }
    }

    // Helper methods to get data from the database
    private DataTable GetPlayerBasicInfo(int playerId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Players WHERE PlayerID = @playerId";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@playerId";
                    param.Value = playerId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting player basic info: {ex.Message}", true);
            return null;
        }
    }

    private DataTable GetPlayerDetails(int playerId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM PlayerDetails WHERE PlayerID = @playerId";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@playerId";
                    param.Value = playerId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting player details: {ex.Message}", true);
            return null;
        }
    }

    private DataTable GetSimonScores(int playerId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM SimonScores WHERE PlayerID = @playerId ORDER BY RecordedAt DESC";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@playerId";
                    param.Value = playerId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting Simon scores: {ex.Message}", true);
            return null;
        }
    }

    private DataTable GetMazeScores(int playerId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM MazeScores WHERE PlayerID = @playerId ORDER BY RecordedAt DESC";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@playerId";
                    param.Value = playerId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting maze scores: {ex.Message}", true);
            return null;
        }
    }

    private DataTable GetShootingSessions(int playerId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM ShootingSessions WHERE PlayerID = @playerId ORDER BY StartedAt DESC";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@playerId";
                    param.Value = playerId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting shooting sessions: {ex.Message}", true);
            return null;
        }
    }

    private DataTable GetShootingScores(int sessionId)
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM ShootingScores WHERE SessionID = @sessionId ORDER BY ShotNumber";

                    var param = cmd.CreateParameter();
                    param.ParameterName = "@sessionId";
                    param.Value = sessionId;
                    cmd.Parameters.Add(param);

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting shooting scores: {ex.Message}", true);
            return null;
        }
    }

    // Creates a summary file with all players' data for easier viewing
    private void CreateSummaryFile(PlayerData newData, int playerId)
    {
        try
        {
            string summaryPath = Path.Combine(backupFolderPath, "AllPlayersData.json");
            List<PlayerData> allPlayers = new List<PlayerData>();

            // Read existing data if available
            if (File.Exists(summaryPath))
            {
                string existingJson = File.ReadAllText(summaryPath);

                // Unity's JsonUtility doesn't directly support deserializing JSON arrays,
                // so we need a wrapper class to deserialize the data
                AllPlayersWrapper wrapper = JsonUtility.FromJson<AllPlayersWrapper>(existingJson);
                if (wrapper != null && wrapper.players != null)
                {
                    allPlayers = wrapper.players;

                    // Remove any existing entry for this player
                    allPlayers.RemoveAll(p => p.playerId == playerId);
                }
            }

            // Add the new player data
            allPlayers.Add(newData);

            // Create a wrapper to serialize the list
            AllPlayersWrapper newWrapper = new AllPlayersWrapper { players = allPlayers };
            string json = JsonUtility.ToJson(newWrapper, true);

            // Write to the summary file
            File.WriteAllText(summaryPath, json, Encoding.UTF8);

            LogMessage($"Updated summary file with latest player data: {summaryPath}");
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating summary file: {ex.Message}", true);
        }
    }

    // Helper method for logging with optional error flag
    private void LogMessage(string message, bool isError = false)
    {
        if (logDebugMessages)
        {
            if (isError)
                Debug.LogError($"[JSONDataSaver] {message}");

        }
    }

    // Manual backup method that can be called from UI buttons
    public void BackupAllPlayersData()
    {
        try
        {
            // Get all player IDs from database
            DataTable allPlayers = GetAllPlayers();

            if (allPlayers != null && allPlayers.Rows.Count > 0)
            {
                LogMessage($"Starting backup for {allPlayers.Rows.Count} players...");

                foreach (DataRow row in allPlayers.Rows)
                {
                    int playerId = Convert.ToInt32(row["PlayerID"]);
                    BackupPlayerData(playerId);
                }

                LogMessage("All players backup completed successfully");
            }
            else
            {
                LogMessage("No players found to backup", true);
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error backing up all players: {ex.Message}", true);
        }
    }

    private DataTable GetAllPlayers()
    {
        try
        {
            using (var connection = new Mono.Data.Sqlite.SqliteConnection("URI=file:" + Path.Combine(Application.persistentDataPath, "game_scores.db")))
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT PlayerID FROM Players";

                    var adapter = new Mono.Data.Sqlite.SqliteDataAdapter((Mono.Data.Sqlite.SqliteCommand)cmd);
                    var table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error getting all players: {ex.Message}", true);
            return null;
        }
    }
}