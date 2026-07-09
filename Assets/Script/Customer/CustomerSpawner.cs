using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("스폰 세팅")]
    public GameObject customerPrefab;
    public Transform spawnPoint;

    [Header("스폰 시간")]
    public float minSpawnTime = 10f;
    public float maxSpawnTime = 30f;

    [Header("손님 한도")]
    public int maxCustomers = 10;

    float spawnTimer;

    void Start()
    {
        //TrySpawnCustomer();
        SetNextSpawnTime();
    }

    
    void Update()
    {
        if (GameManager.instance.currentState != GameManager.GameState.Open)
            return;

        spawnTimer -= Time.deltaTime;
        
        if (spawnTimer <= 0f)
        {
            TrySpawnCustomer();
            SetNextSpawnTime();
        }
    }

    void TrySpawnCustomer()
    {
        int currentCustomers = GameObject.FindGameObjectsWithTag("Customer").Length;

        if (currentCustomers >= maxCustomers)
            return;

        Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    void SetNextSpawnTime()
    {
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }
}
