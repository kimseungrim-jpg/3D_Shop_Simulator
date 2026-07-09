using NUnit.Framework;
using System.Collections.Generic;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.AI;

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

    // ⭐ 기다림 관련
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

    void ChangeState(CustomerState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case CustomerState.MoveToShelf:
                if (targetShelf == null)
                {
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


    void UpdateMoveToShelf()
    {
        if (!shelf.HasStock())
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

    public void SetQueuePosition(Transform pos)
    {
        queueTarget = pos;

        waitTimer = maxWaitTime;

        isAtCounter = false;

        ChangeState(CustomerState.MoveToQueue);
    }

    void UpdateMoveToQueue()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            isAtCounter = true;
            ChangeState(CustomerState.WaitInQueue);
        }
    }

    void UpdateWaitInQueue()
    {
        // ⭐ 첫 번째 손님은 안 떠남
        if (checkOutCounter.IsFirst(this))
            return;

        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            LeaveAngry();
        }
    }

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
                // ⭐ 진열대 근처에서 서성거리기
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
                // ⭐ 진열대 근처에서 서성거리기
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

    void LeaveAngry()
    {
        if (!hasPaid)
        {
            ReturnItems();
        }

        checkOutCounter.RemoveFromQueue(this);

        ChangeState(CustomerState.Leaving);
    }

    void LeaveWithoutBuying()
    {
        ReturnItems();
        ChangeState(CustomerState.Leaving);
    }

    void ReturnItems()
    {
        if (cart.Count > 0)
        {
            returnZone.AddItems(cart);
            cart.Clear();
        }
    }

    public void FinishCheckout()
    {
        hasPaid = true;
        ChangeState(CustomerState.Leaving);
    }

    void UpdateLeaving()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f)
        {
            Destroy(gameObject);
        }
    }

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