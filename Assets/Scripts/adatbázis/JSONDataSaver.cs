//using System;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using System.Text;

//public class JSONDataSaver : MonoBehaviour
//{
//    [Header("Settings")]
//    [SerializeField] private string backupFolderName = "PlayerDataBackup";
//    [SerializeField] private bool autoBackupOnGameEnd = true;
//    [SerializeField] private bool logDebugMessages = true;

//    private string backupFolderPath;
//    private SQLiteDBScript dbManager;

//    // Class to structure the player data for JSON serialization
//    [Serializable]
//    private class PlayerData
//    {
//        public int playerId;
//        public string playerName;
//        public int playerAge;
//        public string playerEmail;
//        public string playerAgeGeneration;
//        public int simonScore;
//        public string mazeCompleteTime;
//        public List<ShootingData> shootingScores;
//    }

//    [Serializable]
//    private class ShootingData
//    {
//        public int shootingScore;
//        public double shootingTime;
//        public double hitPointX;
//        public double hitPointY;
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

//        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", -1);
//        if (currentPlayerId == -1)
//        {
//            LogMessage("Cannot backup: No current player ID found!", true);
//            return;
//        }

//        BackupPlayerData(currentPlayerId);
//    }

//    // Backs up a specific player's data
//    public void BackupPlayerData(int playerId)
//    {
//        try
//        {
//            // Get the player data from SQLite
//            var dataTable = dbManager.GetPlayerData(playerId);

//            if (dataTable == null || dataTable.Rows.Count == 0)
//            {
//                LogMessage($"No data found for player ID: {playerId}", true);
//                return;
//            }

//            // Create a new PlayerData object
//            var playerData = new PlayerData
//            {
//                playerId = playerId,
//                playerName = dataTable.Rows[0]["PlayerName"].ToString(),
//                playerAge = Convert.ToInt32(dataTable.Rows[0]["PlayerAge"]),
//                playerEmail = dataTable.Rows[0]["PlayerEmail"].ToString(),
//                playerAgeGeneration = dataTable.Rows[0]["PlayerAgeGeneration"].ToString(),
//                simonScore = Convert.ToInt32(dataTable.Rows[0]["SimonScore"]),
//                shootingScores = new List<ShootingData>()
//            };

//            // Handle MazeCompleteTime which could be either a double or a string
//            var mazeTimeValue = dataTable.Rows[0]["MazeCompleteTime"];
//            if (mazeTimeValue != DBNull.Value)
//            {
//                // Try to preserve the formatted time string if possible
//                playerData.mazeCompleteTime = mazeTimeValue.ToString();
//            }

//            // Save the player data to a JSON file
//            string fileName = $"Player_{playerId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
//            string filePath = Path.Combine(backupFolderPath, fileName);

//            // Convert to JSON
//            string json = JsonUtility.ToJson(playerData, true);
//            File.WriteAllText(filePath, json, Encoding.UTF8);

//            LogMessage($"Player data successfully backed up to: {filePath}");

//            // Create a simplified version that combines all data for easier viewing
//            CreateSummaryFile(playerData, playerId);
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error backing up player data: {ex.Message}", true);
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

//            LogMessage("Updated summary file with latest player data");
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error creating summary file: {ex.Message}", true);
//        }
//    }

//    // Wrapper class for JSON serialization of player list
//    [Serializable]
//    private class AllPlayersWrapper
//    {
//        public List<PlayerData> players;
//    }

//    // Helper method for logging with optional error flag
//    private void LogMessage(string message, bool isError = false)
//    {
//        if (logDebugMessages)
//        {
//            if (isError)
//                Debug.LogError($"[JSONDataSaver] {message}");
//            else
//                Debug.Log($"[JSONDataSaver] {message}");
//        }
//    }

//    // Manual backup method that can be called from UI buttons
//    public void BackupAllPlayersData()
//    {
//        try
//        {
//            // This would require a new method in SQLiteDBScript to get all player IDs
//            // For now, we'll just back up the current player
//            BackupCurrentPlayerData();
//            LogMessage("Backup operation completed");
//        }
//        catch (Exception ex)
//        {
//            LogMessage($"Error backing up all players: {ex.Message}", true);
//        }
//    }
//}
