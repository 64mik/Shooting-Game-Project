using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 씬 관리를 위해 추가

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("UI 연결")]
    [SerializeField] TMP_Text clearResultText;

    [Header("게임 설정")]
    public int baseScore = 10000;         // 기본 점수
    public int perSecondPenalty = 1;      // 초당 감점
    public int hitPenalty = 200;          // 피격 시 감점

    // 내부 변수
    float playTime = 0f;
    int score = 0;
    public int keysCollected = 0;
    bool isPlaying = false; // 기본값을 false로 변경 (메뉴에서 시작하므로)

    // 게임 씬 이름 (유니티에 저장한 실제 씬 이름과 똑같아야 함)
    const string GAME_SCENE_NAME = "Main";

    // [중요] 게임 시작 전 자동으로 생성되게 하는 부분
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Ensure()
    {
        if (FindFirstObjectByType<GameManager>() == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
            Object.DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        // 싱글톤 패턴
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 파괴되지 않음
    }

    // 씬이 로드될 때마다 호출되는 이벤트 연결
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 감지 함수: 여기서 메뉴인지 게임인지 판단
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 로드된 씬이 "Main" (게임 화면) 이라면 게임 시작
        if (scene.name == GAME_SCENE_NAME)
        {
            GameStart();
        }
        else
        {
            // 그 외(메인 메뉴 등)라면 게임 정지 상태
            isPlaying = false;
        }
    }

    // 게임 시작 시 초기화
    void GameStart()
    {
        isPlaying = true;
        playTime = 0f;
        score = baseScore;
        keysCollected = 0;

        // 결과창 끄기 (혹시 켜져있다면)
        if (clearResultText != null)
            clearResultText.gameObject.SetActive(false);

        // HUD 초기화
        UIHUD.I?.SetTime(0);
        UIHUD.I?.SetScore(score);

        Debug.Log("게임 시작! 데이터 초기화 완료");
    }

    void Update()
    {
        // isPlaying이 false면(메뉴 화면 등) 아무것도 안 함
        if (!isPlaying) return;

        // 시간 계산
        playTime += Time.deltaTime;

        // HUD 시간 표시
        int totalSec = Mathf.FloorToInt(playTime);
        UIHUD.I?.SetTime(totalSec);

        // 점수 차감
        score -= perSecondPenalty * Mathf.FloorToInt(Time.deltaTime);
        if (score < 0) score = 0;

        // HUD 점수 표시
        UIHUD.I?.SetScore(score);
    }

    public void AddKey()
    {
        keysCollected++;
        Debug.Log($"[GameManager] 열쇠 획득! 현재 열쇠 개수: {keysCollected}");
    }

    public bool UseKeys(int count)
    {
        if (keysCollected >= count)
        {
            keysCollected -= count;
            Debug.Log($"[GameManager] 문 열림! 남은 열쇠: {keysCollected}");
            return true;
        }
        else
        {
            Debug.Log($"[GameManager] 열쇠 부족! 필요: {count}, 보유: {keysCollected}");
            return false;
        }
    }

    public void RegisterMonsterHit()
    {
        if (!isPlaying) return;

        score -= hitPenalty;
        if (score < 0) score = 0;

        UIHUD.I?.SetScore(score);
        Debug.Log($"[GameManager] 몬스터 피격! 남은 점수: {score}");
    }

    public float PlayTime => playTime;
    public int Score => score;
}