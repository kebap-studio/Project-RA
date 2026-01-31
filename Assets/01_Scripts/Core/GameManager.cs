using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; set; } // 편의를 위해 set -> public

    [Header("Game State")]
    [SerializeField] private bool isPaused = false;
    public bool IsPaused() => isPaused;

    [Header("References")]
    [SerializeField] private PlayerCharacter playerObject;
    [SerializeField] private CameraController cameraController;

    [Header("Rule")]
    [SerializeField] private int clearKillCount = 20;
    private int killCount = 0;
    private bool ended = false;

    [Header("Scene")]
    [SerializeField] private string MainMenuName = "MainMenuScene";

    [Header("Enemy Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 2.5f;
    [SerializeField] private Vector2 mapHalfSize = new Vector2(20f, 20f);
    [SerializeField] private float spawnY = 0f;

    // 몬스터 스폰용 임시 코루틴
    private Coroutine spawnRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGame()
    {
        // 플레이어가 인스펙터에 안 들어갔으면 씬에서 찾아보기
        if (playerObject == null)
            playerObject = FindFirstObjectByType<PlayerCharacter>();

        // 카메라가 인스펙터에 안 들어갔으면 씬에서 찾아보기
        if (cameraController == null)
            cameraController = FindFirstObjectByType<CameraController>();

        // 플레이어 첫 스폰
        SpawnPlayer();

        // 몬스터 스폰 코루틴 시작(임시)
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(CoSpawnLoop());

        Debug.Log("Game Initialized!");
    }

    private void Update()
    {
        // 게임이 끝났으면 더 이상 룰 체크 안 함
        if (ended) return;

        // 일시정지면 룰 체크를 굳이 안함
        if (isPaused) return;

        // 죽었으면 게임 종료(게임 오버)
        // if (playerObject != null && playerObject.GetCurrentHp() <= 0)
        // {
        //     EndGameDead();
        // }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 유니티 전체 시간이 멈추기
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 시간 다시 흐르게
    }

    private void OnDestroy()
    {
        // 플레이어는 죽어도 인스턴스를 유지한다면? -> 이벤트 구독 해제
        // 파괴된다면 밑 코드는 삭제해도 무방
        playerObject.OnDeath -= HandlePlayerDead;
    }

    // =========================
    // 게임 룰
    // =========================

    // 적이 죽으면 킬수 증가
    public void RegisterMonsterKilled()
    {
        if (ended) return;

        killCount++;

        // 킬수 >= 클리어 카운트 -> 게임 종료(클리어)
        if (killCount >= clearKillCount)
        {
            EndGameClear();
        }
    }

    // 플레이어가 죽었을때 처리
    private void EndGameDead()
    {
        if (ended) return;
        ended = true;

        Debug.Log("[GAME END] DEAD");

        CleanupEnemiesAndStopSpawn();

        // 메인 메뉴로 이동
        SceneManager.LoadScene(MainMenuName);
    }

    // 플레이어가 클리어했을때 처리
    private void EndGameClear()
    {
        if (ended) return;
        ended = true;

        Debug.Log("[GAME END] CLEAR");

        CleanupEnemiesAndStopSpawn();

        SceneManager.LoadScene(MainMenuName);
    }

    // 모든 적 오브젝트 정리 후 추가스폰 방지
    private void CleanupEnemiesAndStopSpawn()
    {
        // 스폰 중단
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        // 게임 끝나면 남은 적 전부 제거
        // 수정자: 김성민
        // enemyPrefabScript가 존재하지 않아 오류가 발생해서 임시로 주석처리 했습니다. 
        // enemyPrefabScript가 해결될 경우 주석 해제
        // foreach (var e in FindObjectsByType<enemyPrefabScript>(FindObjectsSortMode.None))
        //     Destroy(e.gameObject);
    }

    // =========================
    // 스폰
    // =========================

    // 코루틴으로 일정 시간마다 적 스폰 함수 호출(임시)
    // 타이머 구현이 완료되면 통합 필요
    private System.Collections.IEnumerator CoSpawnLoop()
    {
        while (!ended)
        {
            // Time.timeScale = 0 이면 일시정지
            yield return new WaitForSeconds(spawnInterval);

            if (ended) yield break;
            if (isPaused) continue;

            SpawnEnemy();
        }
    }

    // 스폰을 게임 매니저에서 바로 할건지?
    // 적은 항상 초기화해서 새로 만들어도 좋다.
    // 플레이어는..? 리스폰 이전 상태를 저장 해야하나? -> 로그라이크라 안해도 될수도?

    private void SpawnPlayer() // 플레이어 첫 생성
    {
        if (playerObject == null)
        {
            Debug.Log("Need playerObject to spawn Player.");
            return;
        }

        Vector3 pos = GetRandomPositionInMap();
        GameObject player = Instantiate(playerObject, pos, Quaternion.identity).gameObject;
        if (player != null)
        {
            player.GetComponent<PlayerCharacter>().OnDeath += EndGameDead;

            // 카메라 타겟 설정
            if (cameraController != null)
                cameraController.SetTarget(player.transform);
        }
        else
        {
            Debug.Log("Player Spawn Failed!");
        }
    }

    private void RespawnPlayer()
    {
        // 플레이어 리스폰
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.Log("Need enemyPrefab to spawn Enemy.");
            return;
        }

        Vector3 pos = GetRandomPositionInMap();
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        if (enemy != null)
        {
            enemy.GetComponent<MonsterCharacter>().OnDeath += RegisterMonsterKilled;
        }
        else
        {
            Debug.Log("Enemy Spawn Failed!");
        }
    }

    // 랜덤 좌표 생성
    private Vector3 GetRandomPositionInMap()
    {
        float x = Random.Range(-mapHalfSize.x, mapHalfSize.x);
        float z = Random.Range(-mapHalfSize.y, mapHalfSize.y);
        return new Vector3(x, spawnY, z);
    }

    // 임시 함수 
    private void HandlePlayerDead()
    {

    }
}
