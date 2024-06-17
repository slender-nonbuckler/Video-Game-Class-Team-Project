using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour, IDataPersistence
{
    public Text scoreText; // Reference to the UI Text component
    private int score = 0; // Initial score
    private float timer = 0f; // Timer to keep track of time

    void Start()
    {
        // Initialize the score text
        scoreText.text = "Score: " + score;
        // Start the coroutine to update the score
        StartCoroutine(UpdateScore());
    }

    void Update()
    {
        // Update the timer
        timer += Time.deltaTime;
    }
    /**
    This UpdateScore is just for test, the point increases every 5 seconds.
    TODO: update this function based on our reward system.
    */
    IEnumerator UpdateScore()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f); // Wait for 5 seconds
            score += 1; // Increment the score
            scoreText.text = "Score: " + score; // Update the score text
        }
    }
    public void LoadData(GameData data) 
    {
        
        Debug.Log("loaded in score" + data.Score);
        this.score = data.Score;
    }

    public void SaveData(ref GameData data)
    {
        
        data.Score = this.score;
         if (data == null)
        {
            Debug.LogError("SaveData: GameData is null!");
            return;
        }
    }
}
