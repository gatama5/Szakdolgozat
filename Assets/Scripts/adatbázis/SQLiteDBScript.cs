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
    }

    private void CreateDatabase()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS MainDB (" +
                    "PlayerID INTEGER PRIMARY KEY, " +
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
                    "ShootingScores INTEGER PRIMARY KEY, " +
                    "ShootingTime REAL NOT NULL, " +
                    "HitPointX REAL NOT NULL, " +
                    "HitPointY REAL NOT NULL);";
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    public void InsertPlayerData(int playerID, string playerName, int playerAge, string playerEmail, string ageGen, int simonScore, double mazeTime, int shootingScore)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "INSERT INTO MainDB (PlayerID, PlayerName, PlayerAge, PlayerEmail, PlayerAgeGeneration, SimonScore, MazeCompleteTime, ShootingScores) " +
                    "VALUES (@PlayerID, @PlayerName, @PlayerAge, @PlayerEmail, @PlayerAgeGeneration, @SimonScore, @MazeCompleteTime, @ShootingScores);";

                command.Parameters.AddWithValue("@PlayerID", playerID);
                command.Parameters.AddWithValue("@PlayerName", playerName);
                command.Parameters.AddWithValue("@PlayerAge", playerAge);
                command.Parameters.AddWithValue("@PlayerEmail", playerEmail);
                command.Parameters.AddWithValue("@PlayerAgeGeneration", ageGen);
                command.Parameters.AddWithValue("@SimonScore", simonScore);
                command.Parameters.AddWithValue("@MazeCompleteTime", mazeTime);
                command.Parameters.AddWithValue("@ShootingScores", shootingScore);

                command.ExecuteNonQuery();
                Debug.Log("Játékos adatok sikeresen feltöltve!");
            }
            connection.Close();
        }
    }

    public void InsertShootingScore(int shootingScore, double shootingTime, double hitX, double hitY)
    {
        using (var connection = new SqliteConnection(dbPath))
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
            connection.Close();
        }
    }

    public void UpdateSimonScore(int newScore)
    {
        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE MainDB SET SimonScore = @Score WHERE PlayerID = @PlayerID";
                command.Parameters.AddWithValue("@Score", newScore);
                command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    // Labirintus idõ frissítése
    public void UpdateMazeTime(double completionTime)
    {
        int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "UPDATE MainDB SET MazeCompleteTime = @Time WHERE PlayerID = @PlayerID";
                command.Parameters.AddWithValue("@Time", completionTime);
                command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    // Lövés játék eredményeinek frissítése
    //public void UpdateShootingScore(int score, double time, double hitX, double hitY)
    //{
    //    int currentPlayerId = PlayerPrefs.GetInt("CurrentPlayerId", 1);

    //    // Elõször mentsük el a lövés adatait
    //    InsertShootingScore(score, time, hitX, hitY);

    //    // Majd frissítsük a fõ táblában is
    //    using (var connection = new SqliteConnection(dbPath))
    //    {
    //        connection.Open();
    //        using (var command = connection.CreateCommand())
    //        {
    //            command.CommandText = "UPDATE MainDB SET ShootingScores = @Score WHERE PlayerID = @PlayerID";
    //            command.Parameters.AddWithValue("@Score", score);
    //            command.Parameters.AddWithValue("@PlayerID", currentPlayerId);
    //            command.ExecuteNonQuery();
    //        }
    //        connection.Close();
    //    }
    //}

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
                        // First insert the shooting score details
                        command.CommandText =
                            "INSERT INTO ShootingScoresDB (ShootingScores, ShootingTime, HitPointX, HitPointY) " +
                            "VALUES (@Score, @Time, @HitX, @HitY)";
                        command.Parameters.AddWithValue("@Score", score);
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
            }
            connection.Close();
        }
    }

}
