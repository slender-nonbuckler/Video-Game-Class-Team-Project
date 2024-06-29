using UnityEngine;
using System.Collections;
public class Collectible : MonoBehaviour, IDataPersistence
{
    private static int coinsCollectedThisSession = 0;
    private bool isCollected = false;
    private static float cooldownTime = 0.5f;
    private static float lastCollectionTime = -1f;
    [SerializeField] private float spinSpeed = 180f;
    private Renderer collectibleRenderer;
    private void Start()
    {
        InitializeRenderer();
    }
    private void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        PlayerDriver playerDriver = other.GetComponentInParent<PlayerDriver>();
        if (playerDriver != null && !isCollected && Time.time - lastCollectionTime > cooldownTime)
        {
            CarController carController = playerDriver.GetComponent<CarController>();
            if (carController != null)
            {
                CollectCoin(carController);
            }
        }
    }
    private void CollectCoin(CarController carController)
    {
        coinsCollectedThisSession++;
        Debug.Log($"Coin collected! Total this session: {coinsCollectedThisSession}");
        isCollected = true;
        lastCollectionTime = Time.time;
        StartCoroutine(DeactivateAfterDelay());
    }
    private IEnumerator DeactivateAfterDelay()
    {
        if (collectibleRenderer != null)
        {
            collectibleRenderer.enabled = false;
        }
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        yield return new WaitForSeconds(0.1f);
        gameObject.SetActive(false);
    }
    private void InitializeRenderer()
    {
        collectibleRenderer = GetComponent<Renderer>();
        if (collectibleRenderer == null)
        {
            collectibleRenderer = GetComponentInChildren<Renderer>();
            if (collectibleRenderer == null)
            {
                return;
            }
        }
    }
    public void LoadData(GameData data)
    {
        // Reset coins collected this session when loading
        coinsCollectedThisSession = 0;
    }
    public void SaveData(ref GameData data)
    {
        data.Money += coinsCollectedThisSession;
        Debug.Log($"Saving collected coins: {coinsCollectedThisSession}. New total money: {data.Money}");
        coinsCollectedThisSession = 0;
    }
}