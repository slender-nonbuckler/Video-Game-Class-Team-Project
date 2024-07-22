using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TMPro;


/**
 * In charge of an individual racetrack, its setup progress and completion.
 *
 * Responsibilities:
 * - Providing start locations for the racers
 * - Keeping track of individual racer progress
 * - Knowing when the race is over
 * - Providing info on race events like overtakes, or last laps.
 * - Calculating money and rewarding money to player at the end of the round.
 * - Switching player from 
 */
public class RaceManager : MonoBehaviour, IDataPersistence
{
    public UnityEvent OnRaceCountdown;
    public UnityEvent OnDrivingStart;
    public UnityEvent OnRaceFirstRacerFinish;
    public UnityEvent OnPlayerFinish;
    public UnityEvent OnRaceFinish;

    [Header("References")]
    [SerializeField]
    private List<Transform> startPositions;

    [SerializeField] private List<Checkpoint> checkpoints;
    [SerializeField] private List<RaceId> racers;
    [SerializeField] private List<GameObject> testRacerGos;

    [Header("Parameters")]
    [SerializeField]
    private float countdownLength = 3f;

    [SerializeField] private int lapsNeededToFinish = 2;
    private bool isCountdownStarted = false;
    private bool isCountdownFinished = false;
    private float countdownTimer = 0f;

    //Field to adjust out of bounds height
    [SerializeField] private float OutOfBoundsHeight = -10f;
   

    public bool isRaceFinished { get; private set; } = false;
    private Dictionary<int, RaceProgress> progressByCar = new Dictionary<int, RaceProgress>();

    public RacePlayerCarSpawn playerCarSpawnManager;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Reward System")]
    [SerializeField] private int firstPlaceReward = 50;
    [SerializeField] private int secondPlaceReward = 25;
    [SerializeField] private int thirdPlaceReward = 15;
    [SerializeField] private int participationReward = 10;
    private int playerPlacement;
    private int moneyEarnedThisRace;

    private int moneyEarnedThisSession = 0;
    public List<Transform> GetStartPositions()
    {
        return startPositions;
    }

    public List<RaceResult> GetResults()
    {
        List<RaceResult> results = new List<RaceResult>();

        foreach (var entry in progressByCar)
        {
            int raceId = entry.Key;
            RaceProgress progress = entry.Value;

            RaceResult result = new RaceResult();
            result.raceId = raceId;
            result.position = progress.racePosition;
            result.time = progress.time;

            results.Add(result);
        }

        results.Sort();
        return results;
    }

    public void PositionRacers(List<GameObject> racerGameObjects)
    {
        int placementCount = Math.Min(startPositions.Count, racerGameObjects.Count);
        for (int i = 0; i < placementCount; i++)
        {
            GameObject racer = racerGameObjects[i];
            Transform startPosition = startPositions[i];

            racer.transform.position = startPosition.position;
            racer.transform.rotation = startPosition.rotation;

            RaceId raceId = racer.AddComponent<RaceId>();
            raceId.id = RaceId.nextId;
            racers.Add(raceId);
            progressByCar[raceId.id] = new RaceProgress(lapsNeededToFinish, checkpoints.Count);

            CarController carController = racerGameObjects[i].GetComponent<CarController>();
            if (carController) {
                carController.OnReset += HandleCarControllerReset;
            }
        }

        DisableDrivers();

        for (int i = placementCount; i < racerGameObjects.Count; i++) {
            GameObject extraRacer = racerGameObjects[i];
            Destroy(extraRacer);
        }
    }

    public void StartRaceCountdown()
    {
        if (isCountdownStarted)
        {
            return;
        }

        isCountdownStarted = true;
        countdownTimer = countdownLength;
        Debug.Log("OnRaceCountdown");
        OnRaceCountdown?.Invoke();
    }


    private void Start()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].id = i;
            checkpoints[i].OnPassCheckpoint += HandlePassCheckpoint;
        }

        if (playerCarSpawnManager) {
            testRacerGos.Insert(0, playerCarSpawnManager.selectedCar);
        }
        PositionRacers(testRacerGos);
        StartRaceCountdown();

        OnRaceFinish.AddListener(RewardPlayerMoney);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isRaceFinished)
        {
            return;
        }

        UpdateCountdown();
        UpdateRacerProgress();
        OutOfBoundsReset();
        UpdateIsRaceFinished();
    }

    private void OnDrawGizmosSelected()
    {
        Color startPositionColor = Color.green;
        startPositionColor.a = 0.2f;

        Gizmos.color = startPositionColor;
        foreach (Transform startPosition in startPositions)
        {
            Gizmos.matrix = startPosition.transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.one, new Vector3(3f, 0.5f, 5f));
        }
    }

    private void UpdateCountdown() {
        countdownTimer -= Time.deltaTime;
        UpdateCountdownText();
        
        if (isCountdownStarted == false || isCountdownFinished)
        {
            return;
        }

        if (countdownTimer < 0f)
        {
            isCountdownFinished = true;
            EnableDrivers();

            foreach (RaceProgress progress in progressByCar.Values)
            {
                progress.StartRace();
            }

            Debug.Log("OnDrivingStart");
            OnDrivingStart?.Invoke();
        }
    }
    
    private void UpdateCountdownText() {
        if (!countdownText || !isCountdownStarted) {
            return;
        }

        switch (countdownTimer) {
            case > 0:
                countdownText.gameObject.SetActive(true);
                countdownText.text = $"{Mathf.Floor(countdownTimer) + 1}";
                break;
            case > -1f:
                countdownText.gameObject.SetActive(true);
                countdownText.text = "Go!";
                break;
            default:
                countdownText.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateRacerProgress()
    {
        foreach (RaceId racer in racers)
        {
            RaceProgress progress = progressByCar[racer.id];
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
        foreach (RaceId racer in racers) {
            if (!isPlayer(racer)) {
                continue;
            }

            RaceProgress progress = progressByCar[racer.id];
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
    public Dictionary<int, RaceProgress> GetProgressByCar()
    {
        return progressByCar;
    }

    public int GetLapsNeededToFinish()
    {
        return lapsNeededToFinish;
    }



    private void UpdateIsRaceFinished()
    {
        if (progressByCar.Count <= 0)
        {
            return;
        }

        foreach (RaceProgress progress in progressByCar.Values)
        {
            if (progress.isComplete == false)
            {
                return;
            }
        }

        EndRace();
    }

    private void EndRace()
    {
        isRaceFinished = true;
        Debug.Log("OnRaceFinish");
        OnRaceFinish?.Invoke();

        SwitchPlayerToAI();
    }

    private void HandlePassCheckpoint(object sender, RaceId raceId)
    {
        if (progressByCar.ContainsKey(raceId.id) == false)
        {
            Debug.Log("Non racing car passed checkpoint.");
            return;
        }

        Checkpoint checkpoint = (Checkpoint)sender;
        RaceProgress raceProgress = progressByCar[raceId.id];

        if (checkpoint.id != raceProgress.nextCheckpointId)
        {
            //Debug.Log("Checkpoint visited in wrong order");
            return;
        }

        Debug.Log($"{raceId} passed checkpoint {checkpoint.id}");


        if (checkpoint.id == 0 && raceProgress.previousCheckpointId != int.MinValue)
        {
            raceProgress.lapsCompleted++;
            raceProgress.checkpointsCompleted = 0;
        }
        raceProgress.checkpointsCompleted++;
        
        

        if (raceProgress.lapsCompleted >= lapsNeededToFinish)
        {
            raceProgress.CompleteRace();
            Debug.Log("OnRaceFirstRacerFinish");
            OnRaceFirstRacerFinish?.Invoke();

            if (isPlayer(raceId))
            {
                Debug.Log("OnPlayerFinish");
                OnPlayerFinish?.Invoke();
                EndRace();
            }
        }

        raceProgress.previousCheckpointId = raceProgress.nextCheckpointId;
        raceProgress.nextCheckpointId++;
        raceProgress.nextCheckpointId %= checkpoints.Count;
    }

    private void HandleCarControllerReset(object sender, CarController carController) {
        Debug.Log($"{carController} was reset");
        RaceId raceId = carController.GetComponent<RaceId>();
        if (!raceId) {
            return;
        }

        RaceProgress progress = progressByCar[raceId.id];
        int previousCheckpoint = progress.previousCheckpointId;
        if (previousCheckpoint < 0) {
            return;
        }

        Checkpoint lastCheckpoint = checkpoints[previousCheckpoint];
        Transform resetTransform = lastCheckpoint.resetTransform;
        if (!resetTransform) {
            return;
        }
        
        carController.transform.position = resetTransform.position;
        carController.transform.rotation = resetTransform.rotation;
    }

    private void EnableDrivers()
    {
        foreach (RaceId racer in racers)
        {
            PlayerDriver playerDriver = racer.GetComponent<PlayerDriver>();
            AiDriver aiDriver = racer.GetComponent<AiDriver>();

            if (playerDriver)
            {
                playerDriver.enabled = true;
                continue;
            }

            if (aiDriver)
            {
                aiDriver.enabled = true;
            }
        }
    }

    private void DisableDrivers()
    {
        foreach (RaceId racer in racers)
        {
            PlayerDriver playerDriver = racer.GetComponent<PlayerDriver>();
            AiDriver aiDriver = racer.GetComponent<AiDriver>();

            if (playerDriver)
            {
                playerDriver.enabled = false;
            }

            if (aiDriver)
            {
                aiDriver.enabled = false;
            }
        }
    }

    private bool isPlayer(RaceId raceId)
    {
        return raceId.GetComponent<PlayerDriver>();
    }

    public class RaceProgress : IComparable<RaceProgress>
    {
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

        public RaceProgress(int totalLaps, int totalCheckpoints)
        {
            lapWeight = 1f / totalLaps;
            checkpointWeight = 1f / (totalCheckpoints + 1);
        }

        public void CompleteRace()
        {
            isComplete = true;
            raceCompletionPercent = 1f;
        }

        public void StartRace()
        {
            startTime = Time.time;
            time = Time.time - startTime;
        }

        public void UpdateProgress(
            Vector3 racerPosition,
            Vector3 prevCheckpointPosition,
            Vector3 nextCheckpointPosition
        )
        {
            if (isComplete)
            {
                return;
            }

            UpdateTime();
            UpdateLapCompletionPercent(racerPosition, prevCheckpointPosition, nextCheckpointPosition);
            UpdateRaceCompletionPercent();
        }

        public int CompareTo(RaceProgress other)
        {
            return -raceCompletionPercent.CompareTo(other.raceCompletionPercent);
        }

        private void UpdateRaceCompletionPercent()
        {
            raceCompletionPercent = lapsCompleted * lapWeight + lapWeight * lapCompletionPercent;
        }

        private void UpdateLapCompletionPercent(
            Vector3 racerPosition,
            Vector3 prevCheckpointPosition,
            Vector3 nextCheckpointPosition
        )
        {
            float totalDistance = Vector3.Distance(prevCheckpointPosition, nextCheckpointPosition);
            float remainingDistance = Vector3.Distance(racerPosition, nextCheckpointPosition);
            float chekpointCompletionPercent = (1f - remainingDistance) / totalDistance;

            lapCompletionPercent = (checkpointWeight * checkpointsCompleted)
                                   + (checkpointWeight * chekpointCompletionPercent);
        }

        private void UpdateTime()
        {
            time = Time.time - startTime;
        }
    }

    public class RaceResult : IComparable<RaceResult>
    {
        public int raceId;
        public int position;
        public float time;

        public int CompareTo(RaceResult other)
        {
            return position.CompareTo(other.position);
        }
    }


    // Rewarding Implementation

    private void RewardPlayerMoney()
    {
        List<RaceResult> results = GetResults();
        RaceId playerRaceId = FindPlayerRaceId(results);

        if (playerRaceId != null)
        {
            playerPlacement = results.FindIndex(result => result.raceId == playerRaceId.id) + 1;
            moneyEarnedThisRace = CalculateReward(playerRaceId, results);
            AddMoneyToPlayer(moneyEarnedThisRace);
        }
    }

    public int GetPlayerPlacement()
    {
        return playerPlacement;
    }

    public int GetMoneyEarned()
    {
        return moneyEarnedThisRace;
    }

    private RaceId FindPlayerRaceId(List<RaceResult> results)
    {
        foreach (RaceId raceId in racers) {
            if (raceId.gameObject.CompareTag("Player")) {
                return raceId;
            }
        }

        return null;
    }

    private int CalculateReward(RaceId playerRaceId, List<RaceResult> results)
    {
        int playerPosition = results.FindIndex(result => result.raceId == playerRaceId.id) + 1;

        switch (playerPosition)
        {
            case 1: return firstPlaceReward;
            case 2: return secondPlaceReward;
            case 3: return thirdPlaceReward;
            default: return participationReward;
        }
    }

    private void AddMoneyToPlayer(int amount)
    {
        moneyEarnedThisSession += amount;
        Debug.Log($"Player rewarded {amount} money for race completion! Total this session: {moneyEarnedThisSession}");
    }

    public void LoadData(GameData data)
    {
        moneyEarnedThisSession = 0;
    }

    public void SaveData(ref GameData data)
    {
        data.Money += moneyEarnedThisSession;
        Debug.Log($"Saving race earnings: {moneyEarnedThisSession}. New total money: {data.Money}");
        moneyEarnedThisSession = 0;
    }

    // turn player driver into ai driver

    private void SwitchPlayerToAI()
    {
        RaceId playerRaceId = FindPlayerRaceId(GetResults());
        if (!playerRaceId) {
            return;
        }
        
        PlayerDriver playerDriver = playerRaceId.GetComponent<PlayerDriver>(); 
        AiDriver aiDriver = playerRaceId.GetComponent<AiDriver>();

        if (playerDriver && aiDriver) 
        {
            playerDriver.enabled = false; 
            aiDriver.enabled = true;
        }
    }
}