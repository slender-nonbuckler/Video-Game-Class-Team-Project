using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spacer : MonoBehaviour {
    [SerializeField] private RaceManager raceManager;

    void Update() {
        if (Input.GetButtonDown("Jump")) {
            PrintRaceStanding();
        }
    }

    private void PrintRaceStanding() {
        if (!raceManager) {
            Debug.Log("RaceManager is null");
            return;
        }

        List<RaceManager.RaceResult> raceResults = raceManager.GetResults();
        Debug.Log(raceResults.Count);
        foreach (RaceManager.RaceResult result in raceResults) {
            Debug.Log($"{result.raceId}\tPosition: {result.position}\tTime: {result.time}");
        }
    }
}
