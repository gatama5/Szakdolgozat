using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] TMP_Text simonScore;
    [SerializeField] TMP_Text shootingScore;
    [SerializeField] TMP_Text mazeScore;

    public SimonScores sm_scr;
    public ObjectSpawner_1place hit_place_scores;
    public ObjectSpawner_1place hit_time_scores;
    public ButtonsForMaze maze_scr;

    public List<string> shooting_scores;
    

    void Start()
    {
        //for (int i = 0; i < hit_time_scores.hit_times.Count; i++)
        //{
        //    shooting_scores.Add(hit_place_scores.hitPlace_fromMiddle[i] + " within: " + hit_time_scores.hit_times[i] + "\n");
        //}
        simonScore.text = "Simon High Score: " + sm_scr.highscore_point.ToString();
        mazeScore.text = "Maze Score: " + maze_scr.score_time.ToString();
    }

    void Update()
    {
        
    }
}
