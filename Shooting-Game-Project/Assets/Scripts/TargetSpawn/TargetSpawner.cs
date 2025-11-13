using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 지정한 '벽' 평면 위에서 랜덤 위치로 Target 프리팹을 스폰.
// maxAlive 개수로 유지

public class TargetSpawner : MonoBehaviour
{
    [Header("필수")]
    public GameObject targetPrefab;   // Target 프리팹 (Tag=Target, Target.cs 포함)
    public Transform wall;            // 기준이 되는 벽 Transform

    [Header("스폰 영역(벽 기준)")]
    public Vector2 areaSize = new Vector2(6f, 3f); // 가로=right, 세로=up

    [Header("벽에서 떨어지는 거리 설정")]
    public float wallOffset = 0.2f;   // 벽 표면에서 추가로 더 띄우는 거리

    [Header("유지 설정")]
    public int maxAlive = 3;          // 동시에 살아있는 최대 타겟 수
    public float checkInterval = 0.2f; // 개수 채우기 점검 주기

    private readonly List<GameObject> alive = new List<GameObject>();
    private bool isActive = false;
    private Coroutine loop;

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        loop = StartCoroutine(Loop());
    }

    public void Deactivate()
    {
        isActive = false;
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i] != null)
                Destroy(alive[i]);
        }
        alive.Clear();
    }

    private IEnumerator Loop()
    {
        var wait = new WaitForSeconds(checkInterval);

        // 시작 시 바로 채워놓기
        RefillIfNeeded();

        while (isActive)
        {
            // 파괴된 타겟 정리
            alive.RemoveAll(x => x == null);

            // 부족하면 채움
            RefillIfNeeded();

            yield return wait;
        }
    }

    private void RefillIfNeeded()
    {
        while (alive.Count < maxAlive)
            SpawnOne();
    }

    private void SpawnOne()
    {
        if (!targetPrefab || !wall) return;

        // 벽의 좌표계
        Vector3 right = wall.right;
        Vector3 up = wall.up;
        Vector3 wallForward = wall.forward.normalized;

        // 타겟이 바라볼 방향
        Vector3 faceDir = -wallForward;

        float wallDepth = 0f;
        BoxCollider box = wall.GetComponent<BoxCollider>();
        if (box != null)
        {
            wallDepth = box.size.z * wall.lossyScale.z * 0.5f;
        }
        else
        {
            wallDepth = wall.localScale.z * 0.5f;
        }

        // 벽의 중심에서, 타겟이 서야 하는 "벽 표면 위치"를 구함
        float totalOffset = wallDepth + wallOffset; // 벽 반두께 + 추가 거리
        Vector3 wallSurfaceCenter = wall.position + faceDir * totalOffset;

        // 벽 표면 위에서 좌우/상하로 랜덤 이동
        float rx = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
        float ry = Random.Range(-areaSize.y * 0.5f, areaSize.y * 0.5f);

        Vector3 pos = wallSurfaceCenter + right * rx + up * ry;

        // 생성 및 방향 설정
        GameObject go = Instantiate(targetPrefab, pos, Quaternion.identity);
        go.transform.forward = faceDir;

        alive.Add(go);
    }

    public bool IsActive => isActive;
}
