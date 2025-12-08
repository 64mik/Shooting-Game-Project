using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour, IHittable
{
    private enum State { Patrol, Chase, Stunned }

    [Header("필수 설정")]
    public Transform player;
    public LayerMask obstacleMask;

    [Header("애니메이션 설정")]
    public Animator animator;

    // [추가됨] 히트 애니메이션의 원래 길이 (초 단위). 에셋에서 확인해서 적어주세요!
    public float hitAnimLength = 1.0f;

    private readonly string hashWalk = "IsWalk";
    private readonly string hashRun = "IsRun";
    private readonly string hashAttack = "Attack";
    private readonly string hashHit = "Hit";

    [Header("순찰 설정")]
    public float patrolSpeed = 2f;
    public float directionChangeInterval = 2f;
    public float wallCheckDistance = 0.5f;

    [Header("추격 설정")]
    public float chaseSpeed = 4f;
    public float viewDistance = 8f;
    public Vector2 viewBoxSize = new Vector2(4f, 8f);
    public float loseChaseDelay = 3f;

    [Header("상태 설정")]
    public float defaultStunDuration = 2f;

    [Header("사운드")]
    public AudioSource sfxSource;
    public AudioClip chaseStartClip;
    public AudioSource bgmSource;

    [Header("공격")]
    [SerializeField] int attackDamage = 10;
    [SerializeField] float attacksPerSec = 1.5f;
    [SerializeField] float attackRange = 1.2f;

    State state = State.Patrol;
    float nextAttackTime;
    float dirTimer;
    float loseSightTimer;
    float stunTimer;
    Vector3 currentDir;
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = Mathf.Max(0.05f, attackRange * 0.8f);
            attackRange = Mathf.Max(attackRange, agent.stoppingDistance);
            agent.updateRotation = false;
        }

        PickRandomDirection();
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase: UpdateChase(); break;
            case State.Stunned: UpdateStunned(); break;
        }
    }

    // ... (Patrol, Chase 관련 코드는 기존과 동일하므로 생략) ...
    // 아래 UpdatePatrol, UpdateChase 등은 기존 코드 그대로 두시면 됩니다.
    // 만약 전체 코드가 필요하면 말씀해주세요. 공간 절약을 위해 핵심만 바꿉니다.

    void UpdatePatrol()
    {
        SetMoveAnim(true, false);
        if (agent != null && agent.enabled) agent.enabled = false;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f) PickRandomDirection();

        transform.position += currentDir * patrolSpeed * Time.deltaTime;
        transform.forward = currentDir;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, currentDir, wallCheckDistance, obstacleMask))
            PickRandomDirection();

        if (CanSeePlayer()) StartChase();
    }

    void PickRandomDirection()
    {
        dirTimer = directionChangeInterval;
        int r = Random.Range(0, 4);
        switch (r)
        {
            case 0: currentDir = Vector3.forward; break;
            case 1: currentDir = Vector3.back; break;
            case 2: currentDir = Vector3.right; break;
            case 3: currentDir = Vector3.left; break;
        }
    }

    void StartChase()
    {
        state = State.Chase;
        loseSightTimer = 0f;
        if (agent != null) { agent.enabled = true; agent.speed = chaseSpeed; }
        if (sfxSource != null && chaseStartClip != null) sfxSource.PlayOneShot(chaseStartClip);
        if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play();
        SetMoveAnim(false, true);
    }

    void StopChase()
    {
        state = State.Patrol;
        PickRandomDirection();
        if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        if (agent != null && agent.enabled) agent.enabled = false;
        SetMoveAnim(true, false);
    }

    void UpdateChase()
    {
        SetMoveAnim(false, true);
        if (player == null) { StopChase(); return; }

        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);
            if (agent.desiredVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 dir = agent.desiredVelocity.normalized;
                transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 8f);
            }
        }
        else
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            transform.forward = dir;
        }

        if (CanSeePlayer()) loseSightTimer = 0f;
        else
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseChaseDelay) StopChase();
        }

        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = player.position; b.y = 0f;
        float distToPlayer = Vector3.Distance(a, b);

        if (distToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSec);
            DoAttack();
        }
    }

    // --------- 스턴 로직 수정 (핵심) ---------
    void StartStun(float duration)
    {
        state = State.Stunned;
        stunTimer = duration;

        if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        if (agent != null && agent.enabled) agent.enabled = false;

        // 이동 애니메이션 끄기
        SetMoveAnim(false, false);

        // 피격 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger(hashHit);

            // [수정됨] 애니메이션 속도 조절
            // 공식: (애니메이션 길이 / 스턴 시간) = 배속
            // 예: 길이 1초 / 스턴 3초 = 0.33배속 (느리게)
            float speedMultiplier = hitAnimLength / Mathf.Max(duration, 0.1f);
            animator.speed = speedMultiplier;
        }
    }

    void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            // 스턴 끝! 속도 원상복구 (매우 중요)
            if (animator != null) animator.speed = 1f;

            if (CanSeePlayer())
                StartChase();
            else
            {
                state = State.Patrol;
                PickRandomDirection();
            }
        }
    }

    void DoAttack()
    {
        Vector3 to = (player.position - transform.position); to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
            transform.forward = Vector3.Lerp(transform.forward, to.normalized, Time.deltaTime * 20f);

        var hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null) hp.TakeDamage(attackDamage);

        if (animator != null) animator.SetTrigger(hashAttack);
    }

    void SetMoveAnim(bool isWalk, bool isRun)
    {
        if (animator == null) return;
        animator.SetBool(hashWalk, isWalk);
        animator.SetBool(hashRun, isRun);
    }

    // IHittable 인터페이스
    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 인스펙터에서 설정한 스턴 시간 사용
        StartStun(defaultStunDuration);
    }

    // (CanSeePlayer, OnDrawGizmosSelected 함수들은 기존과 동일)
    bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f + forward * (viewDistance * 0.5f);
        Vector3 halfExtents = new Vector3(viewBoxSize.x * 0.5f, 1f, viewDistance * 0.5f);
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        bool insideRect = false;
        foreach (var col in hits) { if (col.transform == player) { insideRect = true; break; } }
        if (!insideRect) return false;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 toPlayer = (player.position + Vector3.up * 1f) - origin;
        float dist = toPlayer.magnitude;
        if (Physics.Raycast(origin, toPlayer.normalized, dist, obstacleMask)) return false;
        return true;
    }
}