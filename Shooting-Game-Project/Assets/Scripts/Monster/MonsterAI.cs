using UnityEngine;
using UnityEngine.AI;

// 몬스터 AI
// 미로 안에서 4방향 순찰
// 직사각형 시야에 플레이어 보이면 NavMesh로 추격
// 스턴건 맞으면 일정 시간 멈춤 (죽지 않음)
public class MonsterAI : MonoBehaviour, IHittable
{
    private enum State { Patrol, Chase, Stunned }

    [Header("필수 참조")]
    public Transform player;          // 플레이어 Transform
    public LayerMask obstacleMask;    // 벽/장애물 레이어

    [Header("순찰 설정 (4방향 이동)")]
    public float patrolSpeed = 2f;
    public float directionChangeInterval = 2f;   // 방향 강제 변경 간격(초)
    public float wallCheckDistance = 0.5f;       // 앞에 벽 감지 거리

    [Header("추격 설정 (NavMesh)")]
    public float chaseSpeed = 4f;
    public float viewDistance = 8f;              // 시야 길이 (앞으로)
    public Vector2 viewBoxSize = new Vector2(4f, 8f); // 시야 직사각형 (가로, 세로)
    public float loseChaseDelay = 3f;            // 플레이어 안 보여도 이 시간 동안은 계속 추격

    [Header("스턴 설정")]
    public float defaultStunDuration = 2f;       // Bullet damage가 0일 때 기본 스턴 시간

    [Header("사운드")]
    public AudioSource sfxSource;        // 효과음 재생용 (몬스터 본인)
    public AudioClip chaseStartClip;     // 추격 시작 효과음
    public AudioSource bgmSource;        // 긴장 BGM(AudioSource)

    // 내부 상태
    State state = State.Patrol;
    float dirTimer;
    float loseSightTimer;
    float stunTimer;
    Vector3 currentDir;      // 현재 순찰 방향 (상/하/좌/우)
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = 0.1f;
            agent.updateRotation = false; // 회전은 우리가 직접
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
        // NavMeshAgent 끄고 단순 이동
        if (agent != null && agent.enabled)
            agent.enabled = false;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            PickRandomDirection();
        }

        // 앞으로 이동
        transform.position += currentDir * patrolSpeed * Time.deltaTime;
        transform.forward = currentDir;

        // 앞에 벽 있으면 방향 변경
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, currentDir,
            wallCheckDistance, obstacleMask))
        {
            PickRandomDirection();
        }

        // 플레이어가 시야 안에 들어오면 추격 시작
        if (CanSeePlayer())
        {
            StartChase();
        }
    }

    void PickRandomDirection()
    {
        dirTimer = directionChangeInterval;

        // 상/하/좌/우 중 하나 고르기
        int r = Random.Range(0, 4);
        switch (r)
        {
            case 0: currentDir = Vector3.forward; break;   // +Z
            case 1: currentDir = Vector3.back; break;   // -Z
            case 2: currentDir = Vector3.right; break;   // +X
            case 3: currentDir = Vector3.left; break;   // -X
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

        // 추격 시작 효과음
        if (sfxSource != null && chaseStartClip != null)
        {
            sfxSource.PlayOneShot(chaseStartClip);
        }

        // 긴장 BGM 시작
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    void StopChase()
    {
        state = State.Patrol;
        PickRandomDirection();

        // BGM 정지
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        // NavMeshAgent는 순찰에서 안 씀
        if (agent != null && agent.enabled)
            agent.enabled = false;
    }

    void UpdateChase()
    {
        if (player == null)
        {
            StopChase();
            return;
        }

        if (agent != null && agent.enabled)
        {
            agent.SetDestination(player.position);

            // 진행 방향으로 몸 돌리기
            if (agent.desiredVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 dir = agent.desiredVelocity.normalized;
                transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 8f);
            }
        }
        else
        {
            // NavMesh 없는 경우: 그냥 직선 추격 (벽에 막힐 수 있음)
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            transform.forward = dir;
        }

        // 플레이어를 볼 수 있으면 타이머 초기화
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
    }

    // --------- 스턴 상태 ---------
    void StartStun(float duration)
    {
        state = State.Stunned;
        stunTimer = duration;

        // 추격 중이었다면 BGM도 끔
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();

        if (agent != null && agent.enabled)
            agent.enabled = false;
    }

    void UpdateStunned()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0f)
        {
            // 스턴 풀리면 다시 시야 확인해서 추격/순찰 결정
            if (CanSeePlayer())
                StartChase();
            else
            {
                state = State.Patrol;
                PickRandomDirection();
            }
        }
    }

    // --------- 시야 체크 (직사각형 + 벽 가림) ---------
    bool CanSeePlayer()
    {
        if (player == null) return false;

        // 직사각형 시야 영역 중심 (앞으로 viewDistance/2)
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f +
                         forward * (viewDistance * 0.5f);

        Vector3 halfExtents = new Vector3(viewBoxSize.x * 0.5f, 1f, viewDistance * 0.5f);

        // Player 레이어만 잡도록 레이어마스크 따로 쓰는게 좋음
        // 여기선 간단히: 플레이어 콜라이더만 있다고 가정
        Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation);
        bool insideRect = false;
        foreach (var col in hits)
        {
            if (col.transform == player)
            {
                insideRect = true;
                break;
            }
        }
        if (!insideRect) return false;

        // 직사각형 안에 있더라도, 벽에 가려져 있으면 못 보는 걸로 처리
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 toPlayer = (player.position + Vector3.up * 1f) - origin;
        float dist = toPlayer.magnitude;

        if (Physics.Raycast(origin, toPlayer.normalized, dist, obstacleMask))
        {
            // 중간에 벽이 가로막음
            return false;
        }

        return true;
    }

    // 에디터에서 시야 박스 보이게
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f +
                         forward * (viewDistance * 0.5f);
        Vector3 size = new Vector3(viewBoxSize.x, 2f, viewDistance);
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, size);
    }

    // --------- IHittable 구현 (스턴 건 피격) ---------
    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 여기서는 damage를 "스턴 시간"으로 간주
        float stunTime = damage > 0f ? damage : defaultStunDuration;
        StartStun(stunTime);

        // 피격 이펙트나 소리 넣고 싶으면 여기서 처리
        // 예: sfxSource.PlayOneShot(stunClip);
    }
}
