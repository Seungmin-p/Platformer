using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    //게임 프레임 설정
    [SerializeField] int fps = 60;
    
    //플레이어
    [SerializeField] Player playerPrefab;
    
    //아이템 및 몬스터 수 카운팅용 변수
    private int currentItemCount;
    private int currentMonsterCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 이미 존재하므로, 새로 생성된 중복 오브젝트는 즉시 파괴
            Destroy(gameObject);
            return;
        }

        // 최초 생성된 인스턴스를 static 변수에 저장
        Instance = this;

        // 씬이 전환되어도 이 오브젝트(GameManager)를 파괴하지 않고 유지
        DontDestroyOnLoad(gameObject);

        // 여기에 게임 시작 시 필요한 초기화 로직을 작성합니다.
        InitializeGame();
        
        Application.targetFrameRate = fps;
    }
    
    private void InitializeGame()
    {
        Debug.Log("[GameManager] 게임 시스템 초기화 완료.");
        // 추후 이곳에서 초기 맵 생성 명령이나 플레이어 배치 등을 제어하게 됩니다.
    }

    private void Start()
    {
        //씬이 시작될 때, 게임에 존재하는 오브젝트의 태그들로 아이템, 몬스터 수 파악
        currentItemCount = GameObject.FindGameObjectsWithTag("Item").Length;
        currentMonsterCount = GameObject.FindGameObjectsWithTag("Monster").Length;
    }

    //스크립트가 활성화 되면 이벤트 구독
    private void OnEnable()
    {
        Player.OnPlayerDeath += HandlePlayerDeath; //플레이어 사망
        Player.OnItemCollected += HandleItemCollected; //아이템 획득
        Monster.OnMonsterDefeated += HandleMonsterDefeated; //몬스터 사망
    }

    //스크립트가 비활성화 되면 이벤트 해제
    private void OnDisable()
    {
        Player.OnPlayerDeath -= HandlePlayerDeath;
        Player.OnItemCollected -= HandleItemCollected;
        Monster.OnMonsterDefeated -= HandleMonsterDefeated;
    }
    
    //플레이어 사망 시 스테이지 재시작 메소드 호출
    private void HandlePlayerDeath(Vector2 force)
    {
        StartCoroutine(StageRestartRoutine());
    }
    
    //아이템 획득 이벤트가 호출되면 카운트 -1 이후 클리어 체크
    private void HandleItemCollected()
    {
        currentItemCount--;
        CheckClearCondition();
    }

    //몬스터 사망 이벤트가 호출되면 카운트 -1 이후 클리어 체크
    private void HandleMonsterDefeated()
    {
        currentMonsterCount--;
        CheckClearCondition();
    }

    //스테이지 클리어 판정 체크용 메소드
    private void CheckClearCondition()
    {
        //남은 아이템, 몬스터의 수가 0개 이하라면
        if (currentItemCount <= 0 && currentMonsterCount <= 0)
        {
            //스테이지 클리어
            StartCoroutine(StageClearRoutine());
        }
    }
    
    private IEnumerator StageRestartRoutine()
    {
        Debug.Log("게임 재시작 대기중...");
        
        //플레이어 사망 후 2초 대기
        yield return new WaitForSeconds(2f);
    
        //현재 씬 재시작
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private IEnumerator StageClearRoutine()
    {
        Debug.Log("다음 스테이지 이동 대기중...");
        
        //클리어 이후 2초정도 대기
        yield return new WaitForSeconds(2f);
        
        //다음 씬의 인덱스를 가져옴
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        //다음 씬의 인덱스가 총 인덱스의 개수보다 작은지 확인(인덱스 초과 방지)
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            //다음 스테이지가 존재하면 호출
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            //다음 스테이지가 없으면 첫 스테이지로 돌아가기
            SceneManager.LoadScene(0); 
        }
    }
}
