using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/**
 * In charge of an individual racetrack, its setup progress and completion.
 *
 * Responsibilities:
 * - Providing start locations for the racers
 * - Keeping track of individual racer progress
 * - Knowing when the race is over
 * - Providing info on race events like overtakes, or last laps.
 */
public class RaceManager : MonoBehaviour {
    public UnityEvent OnRaceCountdown;
    public UnityEvent OnRaceEnd;

    [SerializeField] private List<Transform> startPositions;
    [SerializeField] private List<Checkpoint> checkpoints;
    [SerializeField] private List<CarController> racers;
    [SerializeField] private List<GameObject> testRacerGos;

    public List<Transform> getStartPositions() {
        return startPositions;
    }

    public List<CarController> getResults() {
        throw new NotImplementedException();
    }

    public void PositionRacers(List<GameObject> racerGameObjects) {
        int placementCount = Math.Min(testRacerGos.Count, racerGameObjects.Count);
        for (int i = 0; i < placementCount; i++) {
            GameObject racer = racerGameObjects[i];
            Transform startPosition = startPositions[i];

            racer.transform.position = startPosition.position;
            Debug.Log("Placing racer " + i + " at " + startPosition.position);
            racer.transform.rotation = startPosition.rotation;
        }
    }

    private void Start() {
        foreach (Checkpoint checkpoint in checkpoints) {
            checkpoint.OnPassCheckpoint += HandlePassCheckpoint;
        }
        
        PositionRacers(testRacerGos);
    }

    private void HandlePassCheckpoint(object sender, EventArgs e) {
        Checkpoint checkpoint = (Checkpoint)sender;
        Debug.Log("Passed checkpoint: " + checkpoint.id);
    }
}