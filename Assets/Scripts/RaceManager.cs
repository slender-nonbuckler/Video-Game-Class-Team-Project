using System;
using System.Collections.Generic;
using System.Linq;
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
    public UnityEvent OnRaceFirstRacerFinish;
    public UnityEvent OnPlayerFinish;
    public UnityEvent OnRaceFinish; 

    [Header("References")] [SerializeField]
    private List<Transform> startPositions;

    [SerializeField] private List<Checkpoint> checkpoints;
    [SerializeField] private List<CarController> racers;
    [SerializeField] private List<GameObject> testRacerGos;

    [Header("Parameters")] [SerializeField]
    private float countdownLength = 3f;

    [SerializeField] private int lapsNeededToFinish = 1;
    private bool isCountdownStarted = false;
    private bool isCountdownFinished = false;
    private float countdownTimer = 0f;

    //Field to adjust out of bounds height
    [SerializeField] private float OutOfBoundsHeight = -10f;
   

    public bool isRaceFinished { get; private set; } = false;
    private Dictionary<CarController, RaceProgress> progressByCar = new Dictionary<CarController, RaceProgress>();

    public RacePlayerCarSpawn playerCarSpawnManager;

    public List<Transform> GetStartPositions() {
        return startPositions;
    }

    public List<RaceResult> GetResults() {
        List<RaceResult> results = new List<RaceResult>();
        
        foreach (var entry in progressByCar) {
            CarController car = entry.Key;
            RaceProgress progress = entry.Value;

            RaceResult result = new RaceResult();
            result.car = car;
            result.position = progress.racePosition;
            result.time = progress.time;
            
            results.Add(result);
        }
        
        results.Sort();
        return results;
    }

    public RaceProgress GetRaceProgressFor(CarController carController) {
        return progressByCar[carController];
    }

    public void PositionRacers(List<GameObject> racerGameObjects) {
        int placementCount = Math.Min(startPositions.Count, racerGameObjects.Count);
        for (int i = 0; i < placementCount; i++) {
            GameObject racer = racerGameObjects[i];
            Transform startPosition = startPositions[i];

            racer.transform.position = startPosition.position;
            racer.transform.rotation = startPosition.rotation;

            CarController carController = racer.GetComponent<CarController>();
            if (carController) {
                racers.Add(carController);
                progressByCar[carController] = new RaceProgress(lapsNeededToFinish, checkpoints.Count);
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
        Debug.Log("OnRaceCountdown");
        OnRaceCountdown?.Invoke();
    }


    private void Start() {
        for (int i = 0; i < checkpoints.Count; i++) {
            checkpoints[i].id = i;
            checkpoints[i].OnPassCheckpoint += HandlePassCheckpoint;
        }

        testRacerGos.Insert(0, playerCarSpawnManager.selectedCar);
        PositionRacers(testRacerGos);
        StartRaceCountdown();
    }

    private void Update() {
        if (isRaceFinished) {
            return;
        }
        
        UpdateCountdown();
        UpdateRacerProgress();
        OutOfBoundsReset();
        UpdateIsRaceFinished();
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

            foreach (RaceProgress progress in progressByCar.Values) {
                progress.StartRace();
            }

            Debug.Log("OnDrivingStart");
            OnDrivingStart?.Invoke();
        }
    }

    private void UpdateRacerProgress()
    {
        foreach (CarController racer in racers)
        {
            RaceProgress progress = progressByCar[racer];
            if (progress == null)
            {
                continue;
            }

            Vector3 racerPosition = racer.gameObject.transform.position;
            Vector3 nextCheckpointPosition = checkpoints[progress.nextCheckpointId].transform.position;
            Vector3 prevCheckpointPosition = racerPosition;
            if (progress.previousCheckpointId >= 0)
            {
                prevCheckpointPosition = checkpoints[progress.previousCheckpointId].transform.position;
            }

            progress.UpdateProgress(racerPosition, prevCheckpointPosition, nextCheckpointPosition);
        }

        List<RaceProgress> progresses = progressByCar.Values.ToList();
        progresses.Sort();

        for (int i = 0; i < progresses.Count; i++)
        {
            progresses[i].racePosition = i + 1;
        }
    }

    private void OutOfBoundsReset() {
        foreach (CarController racer in racers) {
            if (!isPlayer(racer)) {
                continue;
            }

            RaceProgress progress = progressByCar[racer];
            int previousCheckpoint = progress.previousCheckpointId;

            if (racer.transform.position.y > OutOfBoundsHeight) {
                continue;
            }

            Rigidbody playerRb = racer.GetComponent<Rigidbody>();
            if (playerRb) {
                playerRb.velocity = Vector3.zero;
            }

            if (previousCheckpoint < 0) {
                racer.transform.position = startPositions[0].transform.position;
                racer.transform.rotation = startPositions[0].transform.rotation;
            }
            else {
                racer.transform.position =
                    checkpoints[previousCheckpoint].transform.position + new Vector3(0, 8f, 0);
                racer.transform.rotation = checkpoints[previousCheckpoint].transform.rotation;
                
                if (
                    previousCheckpoint == 2
                    || previousCheckpoint == 3
                    || previousCheckpoint == 7
                    || previousCheckpoint == 8
                ) {
                    racer.transform.Rotate(0f, 180f, 0f);
                }

                if (previousCheckpoint == 7) {
                    racer.transform.position += new Vector3(0, 0, -1.5f);
                }
            }
        }
    }

    /**
     * The following method used to obtain progressByCar and lapsneededtofinish 
     * for HUDManager, because progressByCar and lapsneededtofinish are set to
     * be private in this class.
     */
    public Dictionary<CarController, RaceProgress> GetProgressByCar()
    {
        return progressByCar;
    }

    public int GetLapsNeededToFinish()
    {
        return lapsNeededToFinish;
    }



    private void UpdateIsRaceFinished() {
        if (progressByCar.Count <= 0) {
            return;
        }
        
        foreach (RaceProgress progress in progressByCar.Values) {
            if (progress.isComplete == false) {
                return;
            }
        }
        
        EndRace();
    }

    private void EndRace() {
        isRaceFinished = true;
        Debug.Log("OnRaceFinish");
        OnRaceFinish?.Invoke();
    }

    private void HandlePassCheckpoint(object sender, CarController carController) {
        if (progressByCar.ContainsKey(carController) == false) {
            Debug.Log("Non racing car passed checkpoint.");
            return;
        }

        Checkpoint checkpoint = (Checkpoint)sender;
        RaceProgress raceProgress = progressByCar[carController];

        if (checkpoint.id != raceProgress.nextCheckpointId) {
            //Debug.Log("Checkpoint visited in wrong order");
            return;
        }

        raceProgress.checkpointsCompleted++;
        Debug.Log($"{carController} passed checkpoint {checkpoint.id}");

        if (checkpoint.id == 0 && raceProgress.previousCheckpointId != int.MinValue) {
            raceProgress.lapsCompleted++;
            raceProgress.checkpointsCompleted = 0;
        }

        if (raceProgress.lapsCompleted >= lapsNeededToFinish) {
            raceProgress.CompleteRace();
            Debug.Log("OnRaceFirstRacerFinish");
            OnRaceFirstRacerFinish?.Invoke();

            if (isPlayer(carController)) {
                Debug.Log("OnPlayerFinish");
                OnPlayerFinish?.Invoke();
                EndRace();
            }
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
                continue;
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

    private bool isPlayer(CarController carController) {
        return carController.GetComponent<PlayerDriver>() != null;
    }

    public class RaceProgress: IComparable<RaceProgress> {
        public bool isComplete { get; private set; }
        public int racePosition = 0;
        public float time;

        public float raceCompletionPercent { get; private set; }
        public int lapsCompleted = 0;
        public float lapCompletionPercent = 0f;
        public int checkpointsCompleted = 0;

        public int nextCheckpointId = 0;
        public int previousCheckpointId = int.MinValue;

        private float lapWeight;
        private float checkpointWeight;
        private float startTime;

        public RaceProgress(int totalLaps, int totalCheckpoints) {
            lapWeight = 1f / totalLaps;
            checkpointWeight = 1f / totalCheckpoints;
        }

        public void CompleteRace() {
            isComplete = true;
            raceCompletionPercent = 1f;
        }

        public void StartRace() {
            startTime = Time.time;
            time = Time.time - startTime;
        }

        public void UpdateProgress(
            Vector3 racerPosition,
            Vector3 prevCheckpointPosition,
            Vector3 nextCheckpointPosition
        ) {
            if (isComplete) {
                return;
            }

            UpdateTime();
            UpdateLapCompletionPercent(racerPosition, prevCheckpointPosition, nextCheckpointPosition);
            UpdateRaceCompletionPercent();
        }
        
        public int CompareTo(RaceProgress other) {
            return -raceCompletionPercent.CompareTo(other.raceCompletionPercent);
        }

        private void UpdateRaceCompletionPercent() {
            raceCompletionPercent = lapsCompleted * lapWeight + lapWeight * lapCompletionPercent;
        }

        private void UpdateLapCompletionPercent(
            Vector3 racerPosition,
            Vector3 prevCheckpointPosition,
            Vector3 nextCheckpointPosition
        ) {
            float totalDistance = Vector3.Distance(prevCheckpointPosition, nextCheckpointPosition);
            float remainingDistance = Vector3.Distance(racerPosition, nextCheckpointPosition);
            float chekpointCompletionPercent = (1f - remainingDistance) / totalDistance;

            lapCompletionPercent = (checkpointWeight * checkpointsCompleted)
                                   + (checkpointWeight * chekpointCompletionPercent);
        }

        private void UpdateTime() {
            time = Time.time - startTime;
        }
    }
    
    public class RaceResult : IComparable<RaceResult> {
        public CarController car;
        public int position;
        public float time;
        
        public int CompareTo(RaceResult other) {
            return position.CompareTo(other.position);
        }
    }
}