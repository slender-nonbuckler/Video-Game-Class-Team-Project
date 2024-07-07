using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    public TMP_Text playerPositionText;
    public TMP_Text lapInfoText;
    public RaceManager raceManager;

    private void Update()
    {
        if (raceManager != null)
        {
            UpdateHUD();
        }
        else
        {
            Debug.LogError("RaceManager is not assigned in HUDManager.");
        }
    }

    private void UpdateHUD()
    {
        var carProgress = raceManager.GetProgressByCar();
        int lapsNeededToFinish = raceManager.GetLapsNeededToFinish();

        // Find the player's car and update the HUD
        CarController playerCar = raceManager.playerCarSpawnManager.selectedCar.GetComponent<CarController>();
        if (carProgress.TryGetValue(playerCar, out var playerProgress))
        {
            playerPositionText.text = $"Position: {playerProgress.racePosition}";
        }

        // Update lap completed HUD
        UpdateLapInfoText(carProgress, lapsNeededToFinish);

    }

    private void UpdateLapInfoText(Dictionary<CarController, RaceManager.RaceProgress> carProgress, int lapsNeededToFinish)
    {
        if (carProgress.Count > 0)
        {
            RaceManager.RaceProgress firstProgress = carProgress.Values.First();

            int currentLap = firstProgress.lapsCompleted;
            int totalLaps = lapsNeededToFinish;

            lapInfoText.text = $"Lap {currentLap} / {totalLaps}";
        }
    }
}
