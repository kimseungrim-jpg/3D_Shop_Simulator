using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 손님의 전체적인 행동 흐름을 관리
/// </summary>
public class CustomerAI : MonoBehaviour
{
    NavMeshAgent agent;
    Animator animator;

    public enum CustomerState
    {
        MoveToShelf,
        Picking,
        MoveToQueue,
        WaitInQueue,
        WaitBeforeLeave,
        Leaving,
        WaitForRestock
    }

    public CustomerState currentState;

    public CheckoutCounter checkOutCounter;
    public Transform targetShelf;
    public ReturnZone returnZone;

    Shelf[] shelves;
    public Shelf shelf;

    Transform queueTarget;

    public int itemCount = 0;

    float pickTimer = 0f;

    // 기다림 관련
    float waitTimer;
    public float maxWaitTime = 5f;

    float waitBeforeLeaveTimer;
    public float waitBeforeLeaveTime = 2f;
    public List<ItemData> cart = new List<ItemData>();

    bool hasPaid = false;
    public bool isAtCounter = false;

    //재시도
    int retryCount = 0;
    int maxRetry = 3;
    //재고 기다림
    float restockWaitTimer = 0f;
    float restockWaitTime = 10f;

    //행동 쿨타임
    float idleTimer = 0f;

    //행동 범위
    float disRanMove = 4f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (checkOutCounter == null)
        {
            checkOutCounter = FindAnyObjectByType<CheckoutCounter>();
        }

        if (returnZone == null)
        {
            returnZone = FindAnyObjectByType<ReturnZone>();
        }

        shelves = FindObjectsByType<Shelf>(FindObjectsSortMode.None);

        if (PickRandomShelf())
            ChangeState(CustomerState.MoveToShelf);
        else
            ChangeState(CustomerState.WaitForRestock);
    }

    /// <summary>
    /// 애니메이션을 갱신하고 현재 상태에 해당하는 행동을 매 프레임 실행
    /// 손님 오브젝트가 활성화된 동안 매 프레임 호출
    /// </summary>
    void Update()
    {
        UpdateAnimation();

        switch (currentState)
        {
            case CustomerState.MoveToShelf:
                UpdateMoveToShelf();
                break;
            case CustomerState.Picking:
                UpdatePicking();
                break;
            case CustomerState.MoveToQueue:
                UpdateMoveToQueue();
                break;
            case CustomerState.WaitInQueue:
                UpdateWaitInQueue();
                break;
            case CustomerState.WaitBeforeLeave:
                UpdateWaitBeforeLeave();
                break;
            case CustomerState.Leaving:
                UpdateLeaving();
                break;
            case CustomerState.WaitForRestock:
                UpdateWaitForRestock();
                break;
        }
    }

    /// <summary>
    /// 손님의 현재 상태를 변경하고 해당 상태를 시작하는 데 필요한 이동 및 타이머를 설정
    /// 각 행동 단계가 전환될 때 호출
    /// </summary>
    void ChangeState(CustomerState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case CustomerState.MoveToShelf:
                if (targetShelf == null)
                {
                    //이동할 진열대가 사라진 경우 재입고 상태로 전환
                    ChangeState(CustomerState.WaitForRestock);
                    return;
                }
                agent.isStopped = false;
                agent.SetDestination(targetShelf.position);
                break;

            case CustomerState.Picking:
                agent.isStopped = true;
                pickTimer = 2f;
                break;

            case CustomerState.MoveToQueue:
                agent.isStopped = false;
                agent.SetDestination(queueTarget.position);
                break;

            case CustomerState.WaitInQueue:
                agent.isStopped = true;
                break;

            case CustomerState.WaitBeforeLeave:
                agent.isStopped = false;
                break;

            case CustomerState.Leaving:
                agent.isStopped = false;
                agent.SetDestination(checkOutCounter.exitPoint.position);
                break;
            case CustomerState.WaitForRestock:
                agent.isStopped = false;
                restockWaitTimer = restockWaitTime;
                break;
        }
    }

    /// <summary>
    /// 현재 재고가 남아 있는 진열대 중 하나를 무작위로 선택
    /// 첫 방문 진열대를 정하거나 품절 후 다른 진열대를 찾을 때 호출
    /// </summary>
    bool PickRandomShelf()
    {
        List<Shelf> available = new List<Shelf>();

        foreach (var s in shelves)
        {
            if (s.HasStock())
                available.Add(s);
        }

        if (available.Count == 0)
            return false;

        shelf = available[Random.Range(0,available.Count)];
        targetShelf = shelf.transform;

        return true;
    }

    /// <summary>
    /// 선택한 진열대의 재고와 도착 여부를 확인하여 다음 행동을 결정
    /// </summary>
    void UpdateMoveToShelf()
    {
        if (!shelf.HasStock())
        {
            retryCount++;

            //다른 진열대를 무한하게 탐색하는 것을 방지하기 위해 최대 재시도 횟수 이후에는 재입고를 기다림
            if (retryCount >= maxRetry)
            {
                ChangeState(CustomerState.WaitForRestock);
            }
            else
            {
                if (PickRandomShelf())
                {
                    agent.SetDestination(targetShelf.position);
                }
                else
                {
                    ChangeState(CustomerState.WaitForRestock);
                }
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            ChangeState(CustomerState.Picking);
        }
    }

    /// <summary>
    /// 상품을 고르는 시간을 처리, 선택 완료 후 계산대 진입 또는 이탈 대기를 결정
    /// </summary>
    void UpdatePicking()
    {
        pickTimer -= Time.deltaTime;

        if (pickTimer <= 0f)
        {
            int want = Random.Range(1, 4);

            cart = shelf.TakeItems(want);

            if (cart.Count == 0)
            {
                retryCount++;

                if (retryCount >= maxRetry)
                {
                    ChangeState(CustomerState.WaitForRestock);
                }
                else
                {
                    if (PickRandomShelf())
                    {
                        ChangeState(CustomerState.MoveToShelf);
                    }
                    else
                    {
                        ChangeState(CustomerState.WaitForRestock);
                    }
                }
                return;
            }

            retryCount = 0;

            if (checkOutCounter.CanEnterQueue())
            {
                checkOutCounter.Enqueue(this);               
            }
            else
            {
                waitBeforeLeaveTimer = waitBeforeLeaveTime;
                ChangeState(CustomerState.WaitBeforeLeave);
            }
        }
    }

    /// <summary>
    /// 계산대가 배정한 대기 위치를 목적지로 설정하고 해당 위치로 이동
    /// 계산대의 대기열 순서가 추가 또는 갱신될 때 CheckoutCounter에서 호출
    /// </summary>
    public void SetQueuePosition(Transform pos)
    {
        queueTarget = pos;

        waitTimer = maxWaitTime;

        isAtCounter = false;

        ChangeState(CustomerState.MoveToQueue);
    }

    /// <summary>
    /// 배정된 대기 위치에 도착했는지 확인 후 계산 대기 상태로 전환
    /// </summary>
    void UpdateMoveToQueue()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isAtCounter = true;
            ChangeState(CustomerState.WaitInQueue);
        }
    }

    /// <summary>
    /// 대기열에서 대기 시간을 기다리다 최대 시간에 도달하면 구매를 포기하고 매장에서 퇴장
    /// </summary>
    void UpdateWaitInQueue()
    {
        //첫 번째 손님은 안 떠남
        if (checkOutCounter.IsFirst(this))
            return;

        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            LeaveAngry();
        }
    }

    /// <summary>
    /// 물건을 고른 손님은 대기열이 가득 차있다면 빈자리를 기다림
    /// 대기 시간이 오래되도록 빈자리가 나오지 않는다면 구매를 포기하고 매장에서 퇴장
    /// </summary>
    void UpdateWaitBeforeLeave()
    {
        if (checkOutCounter.CanEnterQueue())
        {
            checkOutCounter.Enqueue(this);
            return;
        }

        idleTimer -= Time.deltaTime;

        if (checkOutCounter != null)
        {
            Vector3 dir = (checkOutCounter.transform.position - transform.position).normalized;
            dir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 3f);
        }

        if (checkOutCounter.CanEnterQueue())
        {
            checkOutCounter.Enqueue(this);
            return;
        }

        if (idleTimer <= 0f)
        {
            idleTimer = Random.Range(0.5f, 2f);

            // 50% 확률로 다른 진열대 가보기
            if (Random.value < 0.5f)
            {
                if (PickRandomShelf())
                {
                    ChangeState(CustomerState.MoveToShelf);
                    return;
                }
            }
            else
            {
                //진열대 근처에서 서성거리기
                Vector3 randomPos = transform.position + Random.insideUnitSphere * disRanMove;
                randomPos.y = transform.position.y;

                agent.SetDestination(randomPos);
            }
        }

        waitBeforeLeaveTimer -= Time.deltaTime;

        if (waitBeforeLeaveTimer <= 0f)
        {
            LeaveWithoutBuying();
        }
    }

    /// <summary>
    /// 물건을 고르지 못한 손님은 재입고가 되기를 기다리면서 매장을 돌아다님
    /// 대기 시간이 끝나면 구매를 포기하고 매장에서 퇴장
    /// </summary>
    void UpdateWaitForRestock()
    {
        if (PickRandomShelf())
        {
            ChangeState(CustomerState.MoveToShelf);
            return;
        }

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            idleTimer = Random.Range(0.5f, 2f);

            // 50% 확률로 다른 진열대 가보기
            if (Random.value < 0.5f)
            {
                if (PickRandomShelf())
                {
                    ChangeState(CustomerState.MoveToShelf);
                    return;
                }
            }
            else
            {
                //진열대 근처에서 서성거리기
                Vector3 randomPos = transform.position + Random.insideUnitSphere * disRanMove;
                randomPos.y = transform.position.y;

                agent.SetDestination(randomPos);
            }
        }

        restockWaitTimer -= Time.deltaTime;

        if (restockWaitTimer <= 0f)
        {
            ChangeState(CustomerState.Leaving);
        }
    }

    /// <summary>
    /// 대기열에서 오래 기다린 손님은 상품을 반납하고 대기열에서 제거한 후 퇴장
    /// WaitInQueue 상태의 대기 시간이 만료되면 호출
    /// </summary>
    void LeaveAngry()
    {
        if (!hasPaid)
        {
            ReturnItems();
        }

        checkOutCounter.RemoveFromQueue(this);

        ChangeState(CustomerState.Leaving);
    }

    /// <summary>
    /// 계산대 대기열에 들어가지 못한 손님의 상품을 반납하고 퇴장
    /// aitBeforeLeave 상태의 제한 시간이 만료되었을 때 호출
    /// </summary>
    void LeaveWithoutBuying()
    {
        ReturnItems();
        ChangeState(CustomerState.Leaving);
    }

    /// <summary>
    /// 결제하지 않은 카트의 상품을 반품 구역으로 전달하고 퇴장
    /// 손님이 구매하지 않고 퇴장시에 퇴장 직전에 호출
    /// </summary>
    void ReturnItems()
    {
        if (cart.Count > 0)
        {
            returnZone.AddItems(cart);
            cart.Clear();
        }
    }

    /// <summary>
    /// 결재 완료 상태를 기록하고 손님을 출구로 이동시킴
    /// CheckoutCounter가 해당 손님의 결제 금액을 정산한 직후 호출
    /// </summary>
    public void FinishCheckout()
    {
        hasPaid = true;
        ChangeState(CustomerState.Leaving);
    }

    /// <summary>
    /// 손님이 출구에 도착했는지 확인하고 오브젝트를 제거
    /// </summary>
    void UpdateLeaving()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 영업 종료 등 모든 손님을 강제로 내보내야 할 때 호출
    /// 상품을 반납하고 대기열에서 제거한 뒤 즉시 퇴장 상태로 전환
    /// </summary>
    public void ForceLeave()
    {
        if (cart.Count > 0)
        {
            returnZone.AddItems(cart);
            cart.Clear();
        }

        checkOutCounter.RemoveFromQueue(this);

        ChangeState(CustomerState.Leaving);
    }

    void UpdateAnimation()
    {
        float speed = agent.velocity.magnitude / agent.speed;
        animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);
    }
}