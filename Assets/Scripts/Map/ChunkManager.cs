using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [SerializeField] GameObject playerPrefab;
    [SerializeField, HideInInspector] string[] savedLevelData; //멀티 타일맵 크리에이터에서 넣어주는 맵 데이터
    
    private char[][] fullLevel;
    private List<Tilemap> activeTilemaps = new List<Tilemap>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //savedLevelData의 데이터를 fullLevel에 저장
        if (savedLevelData != null && savedLevelData.Length > 0)
        {
            fullLevel = new char[savedLevelData.Length][];
            for (int i = 0; i < savedLevelData.Length; i++)
            {
                fullLevel[i] = savedLevelData[i].ToCharArray();
            }
        }
    }

    private void Start()
    {
        StartCoroutine(LoadAllRoomsRoutine());
    }

    //타일맵 크리에이터쪽에서 호출하는 데이터 저장용 메소드
    public void SaveLevelData(string[] serializedData)
    {
        savedLevelData = serializedData;
    }

    //모든 씬을 돌면서 로딩하는 메소드
    private IEnumerator LoadAllRoomsRoutine()
    {
        activeTilemaps.Clear();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                //각 씬을 순서로 확인하면서 가져오기
                string roomName = $"Room_{x}_{y}";
                AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);
                
                //씬이 완전하게 로드될 때 까지 대기
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }

                //불러온 씬 데이터 확보
                Scene loadedScene = SceneManager.GetSceneByName(roomName);
                
                //Grid, SpawnContainer 같은 최상위 컨테이너 확인
                foreach (GameObject rootObj in loadedScene.GetRootGameObjects())
                {
                    //이중 타일맵 컴포넌트 확인
                    Tilemap tm = rootObj.GetComponentInChildren<Tilemap>();
                    if (tm != null)
                    {
                        //타일맵 컴포넌트를 확인했다면, activeTilemaps 목록에 집어넣고 탈출
                        activeTilemaps.Add(tm);
                        break;
                    }
                }
            }
        }
        
        //16개 씬의 타일맵을 하나로 합침
        MergeTilemaps();
        
        //시작점을 찾아서 플레이어 스폰
        SpawnPlayerDynamically();
    }

    private void MergeTilemaps()
    {
        //타일맵 로딩에 문제가 없었다면, 메인 타일맵 지정
        if (activeTilemaps.Count <= 1) return;
        Tilemap mainTm = activeTilemaps[0];
        
        //활성화 된 타일맵들을 하나씩 확인
        for (int i = 1; i < activeTilemaps.Count; i++)
        {
            //이번에 확인하는 타일맵을 타일이 그려진 정확한 범위만큼만 가져옴(cellBounds) 
            Tilemap tm = activeTilemaps[i];
            BoundsInt bounds = tm.cellBounds;
            
            //그려진 타일의 좌표를 전부 돌면서 가져온 타일 데이터를 메인 타일맵에 그려줌
            foreach (Vector3Int pos in bounds.allPositionsWithin)
            {
                TileBase tile = tm.GetTile(pos);
                if (tile != null) mainTm.SetTile(pos, tile);
            }
            
            //타일을 전부 옮겼다면, 0_0룸을 제외한 나머지 씬의 Grid는 삭제
            Destroy(tm.transform.parent.gameObject);
        }
        
        //활성화된 타일맵 리스트 삭제
        activeTilemaps.Clear();
        
        //메인 타일맵만 새롭게 넣어줌
        activeTilemaps.Add(mainTm);
        
        //메인 타일맵의 룰 타일을 재검토하여 다시 그려줌
        mainTm.RefreshAllTiles(); 
    }

    //모든 타일 중, 시작포인트를 찾아서 플레이어를 스폰해주는 메소드
    private void SpawnPlayerDynamically()
    {
        if (playerPrefab == null || fullLevel == null || activeTilemaps.Count == 0) return;

        for (int y = 0; y < fullLevel.Length; y++)
        {
            for (int x = 0; x < fullLevel[0].Length; x++)
            {
                //현 타일이 시작점이라면
                if (fullLevel[y][x] == 'S')
                {
                    //월드좌표로 변환 후, 플레이어 스폰
                    Vector3Int cellPos = new Vector3Int(x, -y, 0);
                    Vector3 worldPos = activeTilemaps[0].GetCellCenterWorld(cellPos);
                    Instantiate(playerPrefab, worldPos + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                    return; 
                }
            }
        }
    }

    //월드 좌표를 격자 좌표로 변환해주는 메소드
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        //메인 타일맵의 월드 -> 격자 좌표 변환 기능 이용
        if (activeTilemaps.Count > 0 && activeTilemaps[0] != null)
        {
            return activeTilemaps[0].WorldToCell(worldPos);
        }
        
        //혹시 타일맵이 정상적으로 로딩되지 않은 경우를 위한 내림 연산을 통한 직접 변환 과정
        return new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), 0);
    }

    //폭탄 폭발 시 호출하는 폭탄 범위 타일맵 제거 메소드
    public void ExplodeTilesInWorld(Vector3 bombWorldPos, int range, GameObject effectPrefab)
    {
        if (fullLevel == null || activeTilemaps.Count == 0) return;

        //메인 타일맵 지정, 폭탄의 좌표를 격자 좌표로 변환
        Tilemap mainTm = activeTilemaps[0];
        Vector3Int centerCell = mainTm.WorldToCell(bombWorldPos);

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                //폭탄의 범위를 돌면서 맨해튼 거리 체크
                if (Mathf.Abs(x) + Mathf.Abs(y) > range) continue;

                //Y축 반전 보정
                Vector3Int targetCell = centerCell + new Vector3Int(x, y, 0);
                int arrayX = targetCell.x;
                int arrayY = -targetCell.y;

                //폭탄의 영향을 맵 범위 내부로 한정
                if (arrayY >= 0 && arrayY < fullLevel.Length && arrayX >= 0 && arrayX < fullLevel[0].Length)
                {
                    //폭탄 범위에 출력
                    if (effectPrefab != null)
                    {
                        Vector3 cellWorldPos = mainTm.GetCellCenterWorld(targetCell);
                        Instantiate(effectPrefab, cellWorldPos, Quaternion.identity);
                    }

                    //아이템, 도착지 무시
                    char tileType = fullLevel[arrayY][arrayX];
                    if (tileType == 'I' || tileType == 'E') continue;

                    //1번 타일 삭제처리
                    if (tileType == '1')
                    {
                        mainTm.SetTile(targetCell, null);
                        fullLevel[arrayY][arrayX] = '0'; 
                    }
                }
            }
        }
    }
}