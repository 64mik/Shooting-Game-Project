using UnityEngine;
using UnityEngine.AI;

// ���� AI
// �̷� �ȿ��� 4���� ����
// ���簢�� �þ߿� �÷��̾� ���̸� NavMesh�� �߰�
// ���ϰ� ������ ���� �ð� ���� (���� ����)
public class MonsterAI : MonoBehaviour, IHittable
{
    private enum State { Patrol, Chase, Stunned }

    [Header("�ʼ� ����")]
    public Transform player;          // �÷��̾� Transform
    public LayerMask obstacleMask;    // ��/��ֹ� ���̾�

    [Header("���� ���� (4���� �̵�)")]
    public float patrolSpeed = 2f;
    public float directionChangeInterval = 2f;   // ���� ���� ���� ����(��)
    public float wallCheckDistance = 0.5f;       // �տ� �� ���� �Ÿ�

    [Header("�߰� ���� (NavMesh)")]
    public float chaseSpeed = 4f;
    public float viewDistance = 8f;              // �þ� ���� (������)
    public Vector2 viewBoxSize = new Vector2(4f, 8f); // �þ� ���簢�� (����, ����)
    public float loseChaseDelay = 3f;            // �÷��̾� �� ������ �� �ð� ������ ��� �߰�

    [Header("���� ����")]
    public float defaultStunDuration = 2f;       // Bullet damage�� 0�� �� �⺻ ���� �ð�

    [Header("����")]
    public AudioSource sfxSource;        // ȿ���� ����� (���� ����)
    public AudioClip chaseStartClip;     // �߰� ���� ȿ����
    public AudioSource bgmSource;        // ���� BGM(AudioSource)

    [Header("공격")]
    [SerializeField] int attackDamage = 10;      // 공격력
    [SerializeField] float attacksPerSec = 1.5f; // 공속(초당 공격 수)
    [SerializeField] float attackRange = 1.2f;   // 공격 판정 거리(근접)

    // ���� ����
    State state = State.Patrol;
    float nextAttackTime;                        // 내부 쿨다운
    float dirTimer;
    float loseSightTimer;
    float stunTimer;
    Vector3 currentDir;      // ���� ���� ���� (��/��/��/��)
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = chaseSpeed;
            agent.stoppingDistance = Mathf.Max(0.05f, attackRange * 0.8f);
            attackRange = Mathf.Max(attackRange, agent.stoppingDistance);
            agent.updateRotation = false; // ȸ���� �츮�� ����
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

    // --------- ���� ���� ---------
    void UpdatePatrol()
    {
        // NavMeshAgent ���� �ܼ� �̵�
        if (agent != null && agent.enabled)
            agent.enabled = false;

        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            PickRandomDirection();
        }

        // ������ �̵�
        transform.position += currentDir * patrolSpeed * Time.deltaTime;
        transform.forward = currentDir;

        // �տ� �� ������ ���� ����
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, currentDir,
            wallCheckDistance, obstacleMask))
        {
            PickRandomDirection();
        }

        // �÷��̾ �þ� �ȿ� ������ �߰� ����
        if (CanSeePlayer())
        {
            StartChase();
        }
    }

    void PickRandomDirection()
    {
        dirTimer = directionChangeInterval;

        // ��/��/��/�� �� �ϳ� ������
        int r = Random.Range(0, 4);
        switch (r)
        {
            case 0: currentDir = Vector3.forward; break;   // +Z
            case 1: currentDir = Vector3.back; break;   // -Z
            case 2: currentDir = Vector3.right; break;   // +X
            case 3: currentDir = Vector3.left; break;   // -X
        }
    }

    // --------- �߰� ���� ---------
    void StartChase()
    {
        state = State.Chase;
        loseSightTimer = 0f;

        if (agent != null)
        {
            agent.enabled = true;
            agent.speed = chaseSpeed;
        }

        // �߰� ���� ȿ����
        if (sfxSource != null && chaseStartClip != null)
        {
            sfxSource.PlayOneShot(chaseStartClip);
        }

        // ���� BGM ����
        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    void StopChase()
    {
        state = State.Patrol;
        PickRandomDirection();

        // BGM ����
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }

        // NavMeshAgent�� �������� �� ��
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

            // ���� �������� �� ������
            if (agent.desiredVelocity.sqrMagnitude > 0.1f)
            {
                Vector3 dir = agent.desiredVelocity.normalized;
                transform.forward = Vector3.Lerp(transform.forward, dir, Time.deltaTime * 8f);
            }
        }
        else
        {
            // NavMesh ���� ���: �׳� ���� �߰� (���� ���� �� ����)
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * chaseSpeed * Time.deltaTime;
            transform.forward = dir;
        }

        // �÷��̾ �� �� ������ Ÿ�̸� �ʱ�ȭ
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
        // ... 기존 추격 로직/시야 체크 아래에 붙이기 ...

        // 플레이어와의 거리 계산
        Vector3 a = transform.position; a.y = 0f;
        Vector3 b = player.position;    b.y = 0f;
        float distToPlayer = Vector3.Distance(a, b);

        // 사거리 안 + 쿨다운 OK 이면 공격
        if (distToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / Mathf.Max(0.01f, attacksPerSec);
            DoAttack();
        }

    }

    // --------- ���� ���� ---------
    void StartStun(float duration)
    {
        state = State.Stunned;
        stunTimer = duration;

        // �߰� ���̾��ٸ� BGM�� ��
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
            // ���� Ǯ���� �ٽ� �þ� Ȯ���ؼ� �߰�/���� ����
            if (CanSeePlayer())
                StartChase();
            else
            {
                state = State.Patrol;
                PickRandomDirection();
            }
        }
    }

    // --------- �þ� üũ (���簢�� + �� ����) ---------
    bool CanSeePlayer()
    {
        if (player == null) return false;

        // ���簢�� �þ� ���� �߽� (������ viewDistance/2)
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * 1f +
                         forward * (viewDistance * 0.5f);

        Vector3 halfExtents = new Vector3(viewBoxSize.x * 0.5f, 1f, viewDistance * 0.5f);

        // Player ���̾ �⵵�� ���̾��ũ ���� ���°� ����
        // ���⼱ ������: �÷��̾� �ݶ��̴��� �ִٰ� ����
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

        // ���簢�� �ȿ� �ִ���, ���� ������ ������ �� ���� �ɷ� ó��
        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 toPlayer = (player.position + Vector3.up * 1f) - origin;
        float dist = toPlayer.magnitude;

        if (Physics.Raycast(origin, toPlayer.normalized, dist, obstacleMask))
        {
            // �߰��� ���� ���θ���
            return false;
        }

        return true;
    }

    // �����Ϳ��� �þ� �ڽ� ���̰�
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

    // --------- IHittable ���� (���� �� �ǰ�) ---------
    public void TakeHit(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // ���⼭�� damage�� "���� �ð�"���� ����
        float stunTime = damage > 0f ? damage : defaultStunDuration;
        StartStun(stunTime);

        // �ǰ� ����Ʈ�� �Ҹ� �ְ� ������ ���⼭ ó��
        // ��: sfxSource.PlayOneShot(stunClip);
    }

        void DoAttack()
    {
         Debug.Log("[Enemy] DoAttack called");
        // 정면을 플레이어로 향하게(선택)
        Vector3 to = (player.position - transform.position);
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
            transform.forward = Vector3.Lerp(transform.forward, to.normalized, Time.deltaTime * 20f);

        // 실제 데미지 적용
        var hp = player.GetComponentInParent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(attackDamage);

        // TODO: 공격 애니메이션/사운드/이펙트가 있다면 여기서 트리거
        // animator.SetTrigger("Attack");
        // sfxSource.PlayOneShot(attackClip);
    }

}
