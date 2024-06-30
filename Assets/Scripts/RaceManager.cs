using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
    public UnityEvent OnDrivingStart;
    public UnityEvent OnRaceEnd;

    [Header("References")] [SerializeField]
    private List<Transform> startPositions;

    [SerializeField] private List<Checkpoint> checkpoints;
    [SerializeField] private List<CarController> racers;
    [SerializeField] private List<GameObject> testRacerGos;

    [Header("Parameters")] [SerializeField]
    private float countdownLength = 3f;

    [SerializeField] private int lapsNeededToFinish = 3;
    private bool isCountdownStarted = false;
    private bool isCountdownFinished = false;
    private float countdownTimer = 0f;

    private Dictionary<CarController, RaceProgress> progressByCar = new Dictionary<CarController, RaceProgress>();

    public List<Transform> GetStartPositions() {
        return startPositions;
    }

    public List<CarController> GetResults() {
        throw new NotImplementedException();
    }

    public RaceProgress GetRaceProgress(CarController carController) {
        return progressByCar[carController];
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
                progressByCar[carController] = new RaceProgress();
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

    private void OnDrawGizmosSelected() {
        Color startPositionColor = Color.green;
        startPositionColor.a = 0.2f;

        Gizmos.color = startPositionColor;
        foreach (Transform startPosition in startPositions) {
            Gizmos.matrix = startPosition.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.one, new Vector3(3f, 0.5f, 5f));
        }
    }

    private void UpdateCountdown() {
        if (isCountdownStarted == false || isCountdownFinished) {
            return;
        }

        countdownTimer -= Time.deltaTime;

        if (countdownTimer < 0f) {
            isCountdownFinished = true;
            EnableDrivers();
            OnDrivingStart?.Invoke();
        }
    }

    private void HandlePassCheckpoint(object sender, CarController carController) {
        if (progressByCar.ContainsKey(carController) == false) {
            Debug.Log("Non racing car passed checkpoint.");
            return;
        }

        Checkpoint checkpoint = (Checkpoint)sender;
        RaceProgress raceProgress = progressByCar[carController];

        if (checkpoint.id != raceProgress.nextCheckpointId) {
            Debug.Log("Checkpoint visited in wrong order");
            return;
        }

        if (checkpoint.id == 0 && raceProgress.previousCheckpointId != int.MinValue) {
            raceProgress.lapsCompleted++;
        }
        
        if (raceProgress.lapsCompleted >= lapsNeededToFinish) {
            Debug.Log("Race has been completed.");
            OnRaceEnd?.Invoke();
        }

        raceProgress.previousCheckpointId = raceProgress.nextCheckpointId;
        raceProgress.nextCheckpointId++; 
        raceProgress.nextCheckpointId %= checkpoints.Count; 
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

    public class RaceProgress {
        public int nextCheckpointId = 0;
        public int previousCheckpointId = int.MinValue;
        public int lapsCompleted = 0;
        public float lapProgress = 0f;
        public int racePosition = 0;
    }
}