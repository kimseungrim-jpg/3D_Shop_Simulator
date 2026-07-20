using UnityEngine;

/// <summary>
/// 매장이 영업 중일 때 설정된 시간 간격과 최대 인원에 맞춰 손님 프리팹을 지정된 위치에 생성
/// </summary>
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

    /// <summary>
    /// 현재 존재하는 손님 수가 최대 인원보다 적을 때 새로운 손님을 생성
    /// 생성 타이머가 만료될 때 Update에서 호출
    /// </summary>
    void TrySpawnCustomer()
    {
        int currentCustomers = GameObject.FindGameObjectsWithTag("Customer").Length;

        if (currentCustomers >= maxCustomers)
            return;

        Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
    }

    /// <summary>
    /// 최소 최대 생성 시간 사이에서 다음 손님 생성 대기 시간을 무작위로 설정
    /// 시작 시점과 손님 생성 시도가 끝난 직후 호출
    /// </summary>
    void SetNextSpawnTime()
    {
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }
}
