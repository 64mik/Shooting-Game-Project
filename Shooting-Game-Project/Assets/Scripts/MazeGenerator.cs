using UnityEngine;

public class BlockMazeGenerator : MonoBehaviour
{
    public enum Axis { X_Axis, Z_Axis }

    [Header("맵 크기 (홀수만 가능)")]
    public int width = 21;
    public int depth = 21;

    [Header("에셋 사이즈")]
    public float cellSize = 2f;

    [Header("프리팹 할당")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pillarPrefab; // 필수

    [Header("옵션")]
    public Transform mapParent;

    [Header("🔴 문제 해결 옵션 (여기서 조절하세요)")]
    [Tooltip("벽이 뚱뚱해지면 이 설정을 반대로 바꾸세요.")]
    public Axis stretchAxis = Axis.Z_Axis; // 기본값을 Z로 변경 (사용자 에셋에 맞춤)

    [Tooltip("벽이 가로세로가 반대로 나오면 체크하세요.")]
    public bool swapRotation = false;

    [Tooltip("벽 길이 늘리기 (1.0 = 원본, 2.0 = 2배)")]
    [Range(1.0f, 3.0f)]
    public float wallStretch = 1.0f;

    // 1 = 벽/기둥, 0 = 길
    private int[,] map;

    void Start()
    {
        GenerateMaze();
    }

    [ContextMenu("Generate Maze")]
    public void GenerateMaze()
    {
        ClearMap();

        if (width % 2 == 0) width++;
        if (depth % 2 == 0) depth++;

        map = new int[width, depth];

        // 1. 초기화 (모든 곳을 벽/기둥으로 채움)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // 테두리나 짝수 좌표는 무조건 벽(1)
                if (x == 0 || x == width - 1 || z == 0 || z == depth - 1 || x % 2 == 0 || z % 2 == 0)
                    map[x, z] = 1;
                else
                    map[x, z] = 0; // 방(Room)은 0
            }
        }

        // 2. Binary Tree 알고리즘 (벽 뚫기)
        // 방(홀수 좌표)을 순회하며 오른쪽이나 위쪽 벽을 뚫음(0으로 만듦)
        for (int x = 1; x < width - 1; x += 2)
        {
            for (int z = 1; z < depth - 1; z += 2)
            {
                if (x == width - 2 && z == depth - 2) continue; // 끝부분 제외

                if (x == width - 2)
                {
                    map[x, z + 1] = 0; // 위쪽 벽 제거
                }
                else if (z == depth - 2)
                {
                    map[x + 1, z] = 0; // 오른쪽 벽 제거
                }
                else
                {
                    // 랜덤 방향 (오른쪽 or 위쪽)
                    if (Random.Range(0, 2) == 0) map[x + 1, z] = 0;
                    else map[x, z + 1] = 0;
                }
            }
        }

        // 입구/출구
        map[1, 0] = 0;
        map[width - 2, depth - 1] = 0;

        // 3. 배치
        BuildMap();
    }

    void BuildMap()
    {
        Vector3 startPos = transform.position;
        Transform parent = mapParent != null ? mapParent : transform;
        float spacing = cellSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                Vector3 pos = startPos + new Vector3(x * spacing, 0, z * spacing);

                // 바닥 생성 (모든 곳)
                Instantiate(floorPrefab, pos, Quaternion.identity, parent).name = $"Floor_{x}_{z}";

                if (map[x, z] == 1)
                {
                    // [기둥] (짝수, 짝수)
                    if (x % 2 == 0 && z % 2 == 0)
                    {
                        if (pillarPrefab != null)
                            Instantiate(pillarPrefab, pos, Quaternion.identity, parent).name = $"Pillar_{x}_{z}";
                    }
                    // [세로 벽] (짝수, 홀수) -> 위아래 연결
                    else if (x % 2 == 0 && z % 2 != 0)
                    {
                        SpawnWall(pos, true, parent);
                    }
                    // [가로 벽] (홀수, 짝수) -> 좌우 연결
                    else
                    {
                        SpawnWall(pos, false, parent);
                    }
                }
            }
        }
    }

    void SpawnWall(Vector3 pos, bool isVertical, Transform parent)
    {
        // 회전 로직
        float yRot = 0f;

        if (isVertical) yRot = swapRotation ? 0f : 90f; // 세로벽 각도
        else yRot = swapRotation ? 90f : 0f; // 가로벽 각도

        GameObject go = Instantiate(wallPrefab, pos, Quaternion.Euler(0, yRot, 0), parent);
        go.name = isVertical ? "Wall_Vertical" : "Wall_Horizontal";

        // 스케일 조절 로직 (핵심)
        Vector3 s = go.transform.localScale;

        if (stretchAxis == Axis.X_Axis)
            go.transform.localScale = new Vector3(s.x * wallStretch, s.y, s.z);
        else // Z축 늘리기
            go.transform.localScale = new Vector3(s.x, s.y, s.z * wallStretch);
    }

    public void ClearMap()
    {
        Transform parent = mapParent != null ? mapParent : transform;
        while (parent.childCount > 0)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }
}