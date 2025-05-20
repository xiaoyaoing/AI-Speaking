using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class CharacterBehavior : MonoBehaviour, IInteractable
{
    [Header("基本属性")]
    public string characterName = "学生";
    public float moveSpeed = 2.0f;
    public float rotateSpeed = 100.0f;
    
    [Header("行为设置")]
    public float idleTimeMin = 1.0f;
    public float idleTimeMax = 5.0f;
    public float wanderRadius = 5.0f;
    
    [Header("状态")]
    [SerializeField] private CharacterState currentState = CharacterState.Idle;
    [SerializeField] private float stateTimer = 0f;
    
    // 组件引用
    private Animator animator;
    private NavMeshAgent navAgent;
    
    // 行为相关变量
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Coroutine activeCoroutine;
    
    // 动画参数名称
    private readonly string animIsWalking = "IsWalking";
    private readonly string animIsTalking = "IsTalking";
    private readonly string animIsStudying = "IsStudying";
    
    // 角色状态枚举
    public enum CharacterState
    {
        Idle,
        Walking,
        Talking,
        Studying,
        Interacting
    }
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = rotateSpeed;
        }
    }
    
    void Start()
    {
        startPosition = transform.position;
        
        // 开始角色的AI行为
        StartBehaviorCycle();
    }
    
    void Update()
    {
        UpdateAnimations();
        
        // 状态计时器更新
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0 && currentState != CharacterState.Interacting)
            {
                ChangeState(CharacterState.Idle);
                StartBehaviorCycle();
            }
        }
        
        // 如果使用NavMeshAgent，检查是否到达目的地
        if (navAgent != null && currentState == CharacterState.Walking)
        {
            if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                ChangeState(CharacterState.Idle);
                stateTimer = Random.Range(idleTimeMin, idleTimeMax);
            }
        }
    }
    
    // 更新动画状态
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 更新动画参数
        animator.SetBool(animIsWalking, currentState == CharacterState.Walking);
        animator.SetBool(animIsTalking, currentState == CharacterState.Talking);
        animator.SetBool(animIsStudying, currentState == CharacterState.Studying);
    }
    
    // 改变角色状态
    public void ChangeState(CharacterState newState)
    {
        // 如果状态相同，不做任何事
        if (currentState == newState) return;
        
        // 停止当前行为协程
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        
        // 从上一个状态退出
        ExitState(currentState);
        
        // 设置新状态
        currentState = newState;
        
        // 进入新状态
        EnterState(newState);
    }
    
    // 退出状态时的处理
    private void ExitState(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Walking:
                if (navAgent != null)
                {
                    navAgent.ResetPath();
                }
                break;
        }
    }
    
    // 进入状态时的处理
    private void EnterState(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.Idle:
                stateTimer = Random.Range(idleTimeMin, idleTimeMax);
                break;
            
            case CharacterState.Studying:
                stateTimer = Random.Range(5f, 15f);
                break;
                
            case CharacterState.Talking:
                stateTimer = Random.Range(3f, 8f);
                break;
        }
    }
    
    // 开始行为循环
    private void StartBehaviorCycle()
    {
        if (currentState != CharacterState.Idle) return;
        
        // 随机选择下一个行为
        float behaviorRoll = Random.Range(0f, 1f);
        
        if (behaviorRoll < 0.6f) // 60%几率闲逛
        {
            Wander();
        }
        else if (behaviorRoll < 0.8f) // 20%几率学习
        {
            ChangeState(CharacterState.Studying);
        }
        else // 20%几率继续待机
        {
            stateTimer = Random.Range(idleTimeMin, idleTimeMax);
        }
    }
    
    // 闲逛行为
    public void Wander()
    {
        if (navAgent == null) return;
        
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1))
        {
            navAgent.SetDestination(hit.position);
            ChangeState(CharacterState.Walking);
        }
    }
    
    // 前往指定位置
    public void MoveTo(Vector3 position)
    {
        if (navAgent == null) return;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 20f, 1))
        {
            navAgent.SetDestination(hit.position);
            ChangeState(CharacterState.Walking);
        }
    }
    
    // 停止移动
    public void StopMoving()
    {
        if (navAgent != null)
        {
            navAgent.ResetPath();
        }
        
        ChangeState(CharacterState.Idle);
    }
    
    // 实现IInteractable接口
    public void Interact(GameObject other)
    {
        // 保存当前状态以便交互后恢复
        CharacterState previousState = currentState;
        
        // 转向对方
        transform.LookAt(other.transform);
        
        // 进入交互状态
        ChangeState(CharacterState.Talking);
        
        // 交互时间结束后返回上一个状态
        activeCoroutine = StartCoroutine(ReturnToPreviousState(previousState, Random.Range(3f, 8f)));
    }
    
    // 交互结束后返回之前的状态
    private IEnumerator ReturnToPreviousState(CharacterState previousState, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 返回之前的状态或空闲状态
        if (previousState == CharacterState.Interacting || previousState == CharacterState.Talking)
        {
            ChangeState(CharacterState.Idle);
        }
        else
        {
            ChangeState(previousState);
        }
        
        // 重新开始行为循环
        if (currentState == CharacterState.Idle)
        {
            StartBehaviorCycle();
        }
    }
    
    // 显示角色说话内容（可以与UI系统集成）
    public void Say(string message, float duration = 3f)
    {
        // 这里可以显示对话气泡或UI文本
        Debug.Log($"{characterName}: {message}");
        
        // 进入说话状态
        ChangeState(CharacterState.Talking);
        
        // 说话结束后返回空闲状态
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }
        
        activeCoroutine = StartCoroutine(ReturnToIdleAfterTalking(duration));
    }
    
    private IEnumerator ReturnToIdleAfterTalking(float duration)
    {
        yield return new WaitForSeconds(duration);
        ChangeState(CharacterState.Idle);
        StartBehaviorCycle();
    }
} 