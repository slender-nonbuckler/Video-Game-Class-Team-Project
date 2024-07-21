using UnityEngine;

public class RaceId : MonoBehaviour {
    public static int nextId => _nextId++;
    private static int _nextId;

    public int id;
}