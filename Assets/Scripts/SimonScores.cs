using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class SimonScores : MonoBehaviour
{
    [SerializeField] int score_points = 0;
    [SerializeField] int highscore_point = 0;

    private void Start()
    {
        Set();
    }

    public void Addto(int amt = 1)
    {
        score_points += amt;
    }

    public void Set(int amt = 0)
    {
        score_points = amt;
        //Debug.Log("New score: " + amt);
    }

    void LoadHighScore() 
    {
        highscore_point =  PlayerPrefs.GetInt("highscore_point", 0);
    }

    void SaveHighScore() 
    {
        PlayerPrefs.SetInt("highscore_point", highscore_point);
    }

    public void CheckForNewHighscore() 
    {
        if (score_points > highscore_point)
        {
            highscore_point = score_points;
            SaveHighScore();
            Debug.Log("New high score : " + highscore_point);
        }
    }

}
