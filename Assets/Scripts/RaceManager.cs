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
    public UnityEvent OnRaceStart;
    public UnityEvent OnRaceEnd;

    [Header("References")]
    [SerializeField] private List<Transform> startPositions;
    [SerializeField] private List<Checkpoint> checkpoints;
    [SerializeField] private List<CarController> racers;
    [SerializeField] private List<GameObject> testRacerGos;

    [Header("Parameters")] 
    [SerializeField] private float countdownLength = 3f;
    [SerializeField] private int lapsNeededToFinish = 3;
    private bool isCountdownStarted = false;
    private bool isCountdownFinished = false;
    private float countdownTimer = 0f;
    
    
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
            racer.transform.rotation = startPosition.rotation;

            CarController carController = racer.GetComponent<CarController>();
            if (carController) {
                racers.Add(carController);
            }
        }
        
        DisableDrivers();
    }

    public void StartRaceCountdown() {
        if (isCountdownStarted) {
            return;
        }

        isCountdownStarted = true;
        countdownTimer = countdownLength;
        OnRaceCountdown?.Invoke();
    }


    private void Start() {
        foreach (Checkpoint checkpoint in checkpoints) {
            checkpoint.OnPassCheckpoint += HandlePassCheckpoint;
        }
        
        PositionRacers(testRacerGos);
        StartRaceCountdown();
    }

    private void Update() {
        UpdateCountdown();
    }

    private void UpdateCountdown() {
        if (isCountdownStarted == false || isCountdownFinished) {
            return;
        }

        countdownTimer -= Time.deltaTime;

        if (countdownTimer < 0f) {
            isCountdownFinished = true;
            EnableDrivers();
            OnRaceStart?.Invoke();
        }
    }

    private void HandlePassCheckpoint(object sender, EventArgs e) {
        Checkpoint checkpoint = (Checkpoint)sender;
        Debug.Log("Passed checkpoint: " + checkpoint.id);
    }

    private void EnableDrivers() {
        foreach (CarController racer in racers) {
            
            PlayerDriver playerDriver = racer.GetComponent<PlayerDriver>();
            AiDriver aiDriver = racer.GetComponent<AiDriver>();

            if (playerDriver) {
                playerDriver.enabled = true;
                return;
            }

            if (aiDriver) {
                aiDriver.enabled = true;
            }
        }
    }

    private void DisableDrivers() {
        foreach (CarController racer in racers) {
            
            PlayerDriver playerDriver = racer.GetComponent<PlayerDriver>();
            AiDriver aiDriver = racer.GetComponent<AiDriver>();

            if (playerDriver) {
                playerDriver.enabled = false;
            }

            if (aiDriver) {
                aiDriver.enabled = false;
            }
        }
    }
}