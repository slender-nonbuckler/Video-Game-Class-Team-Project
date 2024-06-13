
using UnityEngine;

public class FakeCarInfoScript : MonoBehaviour
{
    public float minSpeed = 50f;
    public float maxSpeed = 150f;

    [HideInInspector]
    public float speed;

    private void Start()
    {
        // Randomize the car's speed on startup
        speed = Random.Range(minSpeed, maxSpeed);
    }
}
