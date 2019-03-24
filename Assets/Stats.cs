using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public int tagsGuessed = 0, mpTagsGuessed = 0, tagsCorrectGuessed = 0,
        mpTagsCorrectGuessed = 0, roundsPlayed = 0, mpRoundsPlayed = 0,
        roundsWon = 0, mpRoundsWon = 0;
    public ulong pointsCollected = 0, mpPointsCollected = 0;

    // Start is called before the first frame update
    void Start()
    {
        LoadStats();
    }

    public void SaveStats()
    {
        PlayerPrefs.SetString("Player_PointsCollected", pointsCollected.ToString());
        PlayerPrefs.SetInt("Player_TagsGuessed", tagsGuessed);
        PlayerPrefs.SetInt("Player_TagsCorrectGuessed", tagsCorrectGuessed);
        PlayerPrefs.SetInt("Player_RoundsPlayed", roundsPlayed);
        PlayerPrefs.SetInt("Player_RoundsWon", roundsWon);

        PlayerPrefs.SetString("Player_MPPointsCollected", mpPointsCollected.ToString());
        PlayerPrefs.SetInt("Player_MPTagsGuessed", mpTagsGuessed);
        PlayerPrefs.SetInt("Player_MPTagsCorrectGuessed", mpTagsCorrectGuessed);
        PlayerPrefs.SetInt("Player_MPRoundsPlayed", mpRoundsPlayed);
        PlayerPrefs.SetInt("Player_MPRoundsWon", mpRoundsWon);
    }

    public void LoadStats()
    {
        string rawPC = PlayerPrefs.GetString("Player_PointsCollected", "0");
        pointsCollected = ulong.Parse(rawPC);

        string mpRawPC = PlayerPrefs.GetString("Player_MPPointsCollected", "0");
        mpPointsCollected = ulong.Parse(mpRawPC);

        tagsGuessed = PlayerPrefs.GetInt("Player_TagsGuessed", 0);
        mpTagsGuessed = PlayerPrefs.GetInt("Player_MPTagsGuessed", 0);

        tagsCorrectGuessed = PlayerPrefs.GetInt("Player_TagsCorrectGuessed", 0);
        mpTagsCorrectGuessed = PlayerPrefs.GetInt("Player_MPTagsCorrectGuessed", 0);

        roundsPlayed = PlayerPrefs.GetInt("Player_RoundsPlayed", 0);
        mpRoundsPlayed = PlayerPrefs.GetInt("Player_MPRoundsPlayed", 0);

        roundsWon = PlayerPrefs.GetInt("Player_RoundsWon", 0);
        mpRoundsWon = PlayerPrefs.GetInt("Player_MPRoundsWon", 0);
    }

    private void OnApplicationQuit()
    {
        SaveStats();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
