using System;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

public class SQLiteDBScript : MonoBehaviour
{
    private string dbPath;

    private void Start()
    {
        dbPath = "URI=file:" + Path.Combine(Application.persistentDataPath, "GameDB.db");
        CreateDatabase();
        Debug.Log(dbPath);
    }

    private void CreateDatabase()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "CREATE TABLE IF NOT EXISTS MainDB (" +
                        "PlayerID INTEGER PRIMARY KEY AUTOINCREMENT, " + // Changed to AUTOINCREMENT
                        "PlayerName TEXT NOT NULL, " +
                        "PlayerAge INTEGER NOT NULL, " +
                        "PlayerEmail TEXT UNIQUE NOT NULL, " +
                        "PlayerAgeGeneration TEXT, " +
                        "SimonScore INTEGER DEFAULT 0, " +
                        "MazeCompleteTime REAL, " +
                        "ShootingScores INTEGER, " +
                        "FOREIGN KEY (ShootingScores) REFERENCES ShootingScoresDB(ShootingScores));";
                    command.ExecuteNonQuery();

                    command.CommandText =
                        "CREATE TABLE IF NOT EXISTS ShootingScoresDB (" +
                        "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                        "ShootingScores INTEGER NOT NULL, " +
                        "ShootingTime REAL NOT NULL, " +
                        "HitPointX REAL NOT NULL, " +
                        "HitPointY REAL NOT NULL);";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating database: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
    }

    // Updated method to not require playerID parameter
    public void InsertPlayerData(string playerName, int playerAge, string playerEmail, string ageGen, int simonScore, double mazeTime, int shootingScore)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO MainDB (PlayerName, PlayerAge, PlayerEmail, PlayerAgeGeneration, SimonScore, MazeCompleteTime, ShootingScores) " +
                        "VALUES (@PlayerName, @PlayerAge, @PlayerEmail, @PlayerAgeGeneration, @SimonScore, @MazeCompleteTime, @ShootingScores);";

                    command.Parameters.AddWithValue("@PlayerName", playerName);
                    command.Parameters.AddWithValue("@PlayerAge", playerAge);
                    command.Parameters.AddWithValue("@PlayerEmail", playerEmail);
                    command.Parameters.AddWithValue("@PlayerAgeGeneration", ageGen);
                    command.Parameters.AddWithValue("@SimonScore", simonScore);
                    command.Parameters.AddWithValue("@MazeCompleteTime", mazeTime);
                    command.Parameters.AddWithValue("@ShootingScores", shootingScore);

                    command.ExecuteNonQuery();

                    // Get the ID of the newly inserted player and save it to PlayerPrefs
                    command.CommandText = "SELECT last_insert_rowid()";
                    int newPlayerId = Convert.ToInt32(command.ExecuteScalar());
                    PlayerPrefs.SetInt("CurrentPlayerId", newPlayerId);
                    PlayerPrefs.Save();

                    Debug.Log($"Játékos adatok sikeresen feltöltve! PlayerID: {newPlayerId}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error inserting player data: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
    }

    public void InsertShootingScore(int shootingScore, double shootingTime, double hitX, double hitY)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "INSERT INTO ShootingScoresDB (ShootingScores, ShootingTime, HitPointX, HitPointY) " +
                        "VALUES (@ShootingScores, @ShootingTime, @HitPointX, @HitPointY);";

                    command.Parameters.AddWithValue("@ShootingScores", shootingScore);
                    command.Parameters.AddWithValue("@ShootingTime", shootingTime);
                    command.Parameters.AddWithValue("@HitPointX", hitX);
                    command.Parameters.AddWithValue("@HitPointY", hitY);

                    command.ExecuteNonQuery();
                    Debug.Log("Lövési adatok sikeresen feltöltve!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error inserting shooting score: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
    }

    public void UpdateSimonScore(int newScore)
    {
        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE MainDB SET SimonScore = @Score WHERE PlayerID = @PlayerID";
                    command.Parameters.AddWithValue("@Score", newScore);
                    command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating Simon score: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
    }

    public void UpdateMazeTime(double completionTime)
    {
        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE MainDB SET MazeCompleteTime = @Time WHERE PlayerID = @PlayerID";
                    command.Parameters.AddWithValue("@Time", completionTime);
                    command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating maze time: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }
    }

    public void UpdateShootingScore(int score, double time, double hitX, double hitY)
    {
        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        // Check if the ShootingScores already exists
                        command.CommandText = "SELECT COUNT(*) FROM ShootingScoresDB WHERE ShootingScores = @Score";
                        command.Parameters.AddWithValue("@Score", score);
                        int exists = Convert.ToInt32(command.ExecuteScalar());

                        if (exists > 0)
                        {
                            // Update existing record
                            command.CommandText =
                                "UPDATE ShootingScoresDB SET ShootingTime = @Time, HitPointX = @HitX, HitPointY = @HitY " +
                                "WHERE ShootingScores = @Score";
                        }
                        else
                        {
                            // Insert new record
                            command.CommandText =
                                "INSERT INTO ShootingScoresDB (ShootingScores, ShootingTime, HitPointX, HitPointY) " +
                                "VALUES (@Score, @Time, @HitX, @HitY)";
                        }

                        command.Parameters.AddWithValue("@Time", time);
                        command.Parameters.AddWithValue("@HitX", hitX);
                        command.Parameters.AddWithValue("@HitY", hitY);
                        command.ExecuteNonQuery();

                        // Then update the main player record
                        command.CommandText =
                            "UPDATE MainDB SET ShootingScores = @Score WHERE PlayerID = @PlayerID";
                        command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Debug.LogError($"Error updating shooting score: {ex.Message}");
                }
                finally
                {
                    if (connection != null && connection.State != ConnectionState.Closed)
                        connection.Close();
                }
            }
        }
    }

    // Add method to get current player data
    public DataTable GetPlayerData(int playerId)
    {
        DataTable results = new DataTable();

        using (var connection = new SqliteConnection(dbPath))
        {
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM MainDB WHERE PlayerID = @PlayerID";
                    command.Parameters.AddWithValue("@PlayerID", playerId);

                    using (var reader = command.ExecuteReader())
                    {
                        results.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error retrieving player data: {ex.Message}");
            }
            finally
            {
                if (connection != null && connection.State != ConnectionState.Closed)
                    connection.Close();
            }
        }

        return results;
    }
}