using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text playerPositionText;
    public TMP_Text lapInfoText;
    public RaceManager raceManager;

    private RaceId playerRaceId;
    
    private void Update()
    {
        if (raceManager)
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
        if (!playerRaceId && !FindPlayerRaceId()) {
            return;
        }
        
        var carProgress = raceManager.GetProgressByCar();
        int lapsNeededToFinish = raceManager.GetLapsNeededToFinish();

        // Find the player's car and update the HUD
        RaceManager.RaceProgress playerProgress = carProgress[playerRaceId.id];
        if (playerProgress == null) {
            return;
        }
        
        playerPositionText.text = $"# {playerProgress.racePosition} / {carProgress.Count}";
        lapInfoText.text = $"Lap {playerProgress.lapsCompleted} / {lapsNeededToFinish}";
    }

    private bool FindPlayerRaceId() {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        if (!playerGameObject) {
            return false;
        }

        RaceId raceId = playerGameObject.GetComponent<RaceId>();
        if (!raceId) {
            return false;
        }

        playerRaceId = raceId;
        return true;
    }
}
