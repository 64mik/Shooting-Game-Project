using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour, IHittable
{
    private enum State { Patrol, Chase, Stunned }

    [Header("필수 설정")]
    public Transform player;          // 플레이어 Transform
    public LayerMask obstacleMask;    // 벽/장애물 레이어

    [Header("애니메이션 설정 (자식 오브젝트의 Animator 연결)")]
    public Animator animator;         // [추가됨] 인스펙터에서 자식의 Animator를 드래그해서 넣으세요

    // [추가됨] 애니메이터 파라미터 이름 (Animator Controller의 파라미터와 같아야 함)
    private readonly string hashWalk = "IsWalk";
    private readonly string hashRun = "IsRun";
    private readonly string hashAttack = "Attack";
    private readonly string hashHit = "Hit";

    [Header("순찰 설정 (4방향 이동)")]
    public float patrolSpeed = 2f;
    public float directionChangeInterval = 2f;
    public float wallCheckDistance = 0.5f;

    [Header("추격 설정 (NavMesh)")]
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

    // 내부 변수
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

        // [추가됨] 만약 인스펙터에서 할당 안 했으면 자식에서 찾기 시도
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

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
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Stunned:
                UpdateStunned();
                break;
        }
    }

    // --------- 순찰 상태 ---------
    void UpdatePatrol()
    {
        // [추가됨] 순찰 애니메이션 (걷기 ON, 뛰기 OFF)
        SetMoveAnim(true, false);

        if (agent != null && agent.enabled)
            agent.enabled = false;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            PickRandomDirection();
        }

        transform.position += currentDir * patrolSpeed * Time.deltaTime;
        transform.forward = currentDir;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, currentDir,
            wallCheckDistance, obstacleMask))
        {
            PickRandomDirection();
        }

        if (CanSeePlayer())
        {
            StartChase();
        }
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

    // --------- 추격 상태 ---------
    void StartChase()
    {
        state = State.Chase;
        loseSightTimer = 0f;

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = chaseSpeed;
        }

        if (sfxSource != null && chaseStartClip != null)
            sfxSource.PlayOneShot(chaseStartClip);

        if (bgmSource != null && !bgmSource.isPlaying)
            bgmSource.Play();

        // [추가됨] 추격 애니메이션 (걷기 OFF, 뛰기 ON) -> UpdateChase에서도 호출되지만 확실히 하기 위해
        SetMoveAnim(false, true);
    }

    void StopChase()
    {
        state = State.Patrol;
        PickRandomDirection();

        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();

        if (agent != null && agent.enabled)
            agent.enabled = false;

        // [추가됨] 추격 종료 시 순찰로 변경되므로 걷기 애니메이션
        SetMoveAnim(true, false);
    }

    void UpdateChase()
    {
        // [추가됨] 계속 뛰는 상태 유지
        SetMoveAnim(false, true);

        if (player == null)
        {
            StopChase();
            return;
        }

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

        if (CanSeePlayer())
        {
            loseSightTimer = 0f;
        }
        else
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseChaseDelay)
            {
                StopChase();
            }
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

    // --------- 피격/스턴 상태 ---------
    void StartStun(float duration)
    {
        state = State.Stunned;
        stunTimer = duration;

        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();

        if (agent != null && agent.enabled)
            agent.enabled = false;

        // [추가됨] 스턴 상태에서는 움직임 애니메이션 멈춤
        SetMoveAnim(false, false);

        // [추가됨] 피격 애니메이션 재생 (Trigger)
        if (animator != null) animator.SetTrigger(hashHit);
    }

    void UpdateStunned()
    {
        // 스턴 중에는 움직임 없음 (Idle 상태가 됨)
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            if (CanSeePlayer())
                StartChase();
            else
            {
                state = State.Patrol;
                PickRandomDirection();
            }
        }
    }

    // --------- 공격 함수 ---------
    void DoAttack()
    {
        Debug.Log("[Enemy] DoAttack called");
        Vector3 to = (player.position - transform.position);
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
            transform.forward = Vector3.Lerp(transform.forward, to.normalized, Time.deltaTime * 20f);

        var hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(attackDamage);

        // [추가됨] 공격 애니메이션 재생 (Trigger)
        if (animator != null) animator.SetTrigger(hashAttack);
    }

    // --------- 보조 함수 ---------

    // [추가됨] 걷기/달리기 애니메이션 상태 설정 함수
    void SetMoveAnim(bool isWalk, bool isRun)
    {
        if (animator == null) return;
        animator.SetBool(hashWalk, isWalk);
        animator.SetBool(hashRun, isRun);
    }

    // (기존 CanSeePlayer, OnDrawGizmosSelected 함수들은 그대로 유지)
    bool CanSeePlayer()
    {
        if (player == null) return false;
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f + forward * (viewDistance * 0.5f);
        Vector3 halfExtents = new Vector3(viewBoxSize.x * 0.5f, 1f, viewDistance * 0.5f);
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        bool insideRect = false;
        foreach (var col in hits)
        {
            if (col.transform == player) { insideRect = true; break; }
        }
        if (!insideRect) return false;
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 toPlayer = (player.position + Vector3.up * 1f) - origin;
        float dist = toPlayer.magnitude;
        if (Physics.Raycast(origin, toPlayer.normalized, dist, obstacleMask)) return false;
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f + forward * (viewDistance * 0.5f);
        Vector3 size = new Vector3(viewBoxSize.x, 2f, viewDistance);
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        float stunTime = damage > 0f ? damage : defaultStunDuration;
        StartStun(stunTime);
    }
}