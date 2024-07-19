using UnityEngine;

public class RaceId : MonoBehaviour {
    private static int nextId = 0;

    public int id;
    
    public RaceId() {
        id = nextId++;
    }
}