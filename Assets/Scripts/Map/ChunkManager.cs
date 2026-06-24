using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class ChunkManager : MonoBehaviour
{
    public static ChunkManager Instance { get; private set; }

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Tilemap masterTilemap; //메인 타일맵
    [SerializeField, HideInInspector] string[] savedLevelData; //멀티 타일맵 크리에이터에서 넣어주는 맵 데이터
    
    private char[][] fullLevel;
    
    //플레이어 위치 추적용
    private Transform playerTransform;
    private Vector2Int currentPlayerRoom = new Vector2Int(-1, -1);
    
    //실시간으로 변경되는 맵 로딩 관련 데이터
    private HashSet<Vector2Int> loadedRooms = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> loadingRooms = new HashSet<Vector2Int>();
    private Dictionary<Vector2Int, BoundsInt> roomBoundsCache = new Dictionary<Vector2Int, BoundsInt>();
    
    //맵 크기 정보
    private const int RoomWidth = 28;
    private const int RoomHeight = 16;
    private const int MapWidth = 4;
    private const int MapHeight = 4;

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

    private IEnumerator Start()
    {
        //시작 방 찾기
        Vector2Int startRoom = GetStartRoom();

        if (startRoom.x != -1)
        {
            currentPlayerRoom = startRoom;
            
            //시작 방 기준 주변 씬들만 로딩
            yield return StartCoroutine(LoadInitialChunksRoutine(startRoom));

            //물리 프레임이 1프레임이라도 동작할 때 까지 대기
            yield return new WaitForFixedUpdate();
            
            //공간이 완전하게 생성된 이후 플레이어 스폰
            SpawnPlayerDynamically();
        }
        else
        {
            Debug.LogError("[ChunkManager] 전체 설계도 데이터에서 시작점 'S'를 찾을 수 없습니다.");
        }
    }
    
    private void Update()
    {
        //플레이어 위치가 확인되지 않는다면 패스
        if (playerTransform == null) return;

        //플레이어의 월드좌표를 방의 크기대로 나눠서 몇번째 방에 있는지 확인
        int currentRoomX = Mathf.FloorToInt(playerTransform.position.x / RoomWidth);
    
        //Y축은 배열에 맞춰서 아래로 내려가는 방향으로 되어있기 때문에
        //-를 곱해서 양수로 만들고 방의 세로 크기로 나눠야 정확한 방 번호가 나옴
        int currentRoomY = Mathf.FloorToInt(-playerTransform.position.y / RoomHeight);

        //두 좌표를 합쳐서 현재 방 위치 확보
        Vector2Int newRoom = new Vector2Int(currentRoomX, currentRoomY);

        //플레이어가 직전까지 있던 방과 다른 방에 진입했다면
        if (newRoom != currentPlayerRoom)
        {
            //currentPlayerRoom 데이터를 현재 방으로 변경
            currentPlayerRoom = newRoom;
        
            //새 방을 기준으로 맵 로딩 다시 진행
            UpdateChunks(newRoom);
        }
    }
    
    //시작 방을 찾아주는 메소드
    private Vector2Int GetStartRoom()
    {
        if (fullLevel == null) return new Vector2Int(-1, -1);

        //y축은 방 하나의 y축(16 만큼만 확인해도 됨)
        for (int y = 0; y < RoomHeight; y++)
        {
            for (int x = 0; x < fullLevel[y].Length; x++)
            {
                //먼저 시작 타일을 찾음
                if (fullLevel[y][x] == 'S')
                {
                    //타일을 찾으면 현재 위치를 방 규격(28x16)으로 나눠서 방 인덱스를 도출해냄
                    return new Vector2Int(x / RoomWidth, 0);
                }
            }
        }
        
        //시작 타일을 못찾은 경우
        return new Vector2Int(-1, -1);
    }

    //게임 시작 직후 주변 방들에 대한 첫 로딩을 진행해주는 메소드
    private IEnumerator LoadInitialChunksRoutine(Vector2Int centerRoom)
    {
        //주변 방 목록을 가져오고, 새 코루틴 리스트 생성
        List<Vector2Int> desiredRooms = GetDesiredRooms(centerRoom);
        List<Coroutine> loadCoroutines = new List<Coroutine>();

        //주변 방 리스트를 Coroutine타입 List에 StartCoroutine으로 넣어줘서
        //모든 방을 사실상 거의 동시에 로딩하기 시작함
        foreach (var room in desiredRooms)
        {
            loadCoroutines.Add(StartCoroutine(LoadAndBakeRoomRoutine(room)));
        }

        //loadCoroutines 리스트를 하나씩 확인
        foreach (var c in loadCoroutines)
        {
            //현재 확인중인 코루틴이 끝날때 까지 대기
            //다음 방이 먼저 로딩이 끝난다 하더라도 현재방이 끝날때 까지 대기
            yield return c;
        }
    }
    
    //현재 방을 기준으로 주변 방들을 필터링 하는 메소드
    private List<Vector2Int> GetDesiredRooms(Vector2Int centerRoom)
    {
        List<Vector2Int> desiredRooms = new List<Vector2Int>();
        
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                //x, y 좌표를 -1 ~ +1까지 돌면서 현 좌표에 더해 주변 방 체크 진행
                int targetX = centerRoom.x + x;
                int targetY = centerRoom.y + y;

                //맵 밖 영역이 아닌, 정상적인 영역의 방일 경우
                if (targetX >= 0 && targetX < MapWidth && targetY >= 0 && targetY < MapHeight)
                {
                    //desiredRooms 리스트에 현재 방 추가
                    desiredRooms.Add(new Vector2Int(targetX, targetY));
                }
            }
        }
        
        //확보된 방 위치들을 반환
        return desiredRooms;
    }

    //불러와야 하는 씬을 불러와서 메인 타일맵에 타일 그려넣기
    private IEnumerator LoadAndBakeRoomRoutine(Vector2Int room)
    {
        //로딩중인 방에 현재 씬 추가
        loadingRooms.Add(room);
        string roomName = $"Room_{room.x}_{room.y}";

        //불러와야 하는 씬의 (메모리)로딩 시작(비동기 방식)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(roomName, LoadSceneMode.Additive);
        
        //비동기 로딩이 끝날 때 까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //메모리에 로딩된 씬 데이터를 loadedScene로 가져옴
        Scene loadedScene = SceneManager.GetSceneByName(roomName);
        Tilemap sourceTilemap = null;
        GameObject spawnContainer = null; //스폰 컨테이너

        //현재 방 안에 있는 최상위 오브젝트들을 확인
        foreach (GameObject rootObj in loadedScene.GetRootGameObjects())
        {
            //하위 오브젝트들을 뒤져서 Tilemap 컴포넌트 및 스폰 컨테이너 확보
            if (sourceTilemap == null) sourceTilemap = rootObj.GetComponentInChildren<Tilemap>();
            if (rootObj.name == "SpawnContainer") spawnContainer = rootObj;
        }

        if (sourceTilemap != null && masterTilemap != null)
        {
            //cellBounds를 가져오기 전 최소 영역으로 재계산
            sourceTilemap.CompressBounds();
            //cellBounds = 타일이 그려져있는 최소, 최대 범위를 의미
            BoundsInt roomBounds = sourceTilemap.cellBounds;
            //타일이 그려진 정확한 범위만큼 모든 타일 설계도를 가져옴(룰타일, 빈공간 등 포함)
            TileBase[] tilesBlock = sourceTilemap.GetTilesBlock(roomBounds);
            
            //메인 타일맵에 가져온 타일 정보들을 기준으로 타일 그려넣기
            masterTilemap.SetTilesBlock(roomBounds, tilesBlock);
            //맵 데이터에 현재 방을 인덱스로, 타일 범위 저장
            roomBoundsCache[room] = roomBounds;
            
            //폭탄에 의해 부셔진 벽이 재로딩 됐을 때 복구되지 않게 방의면적을 fullLevel과 비교
            for (int x = roomBounds.xMin; x < roomBounds.xMax; x++) 
            {
                for (int y = roomBounds.yMin; y < roomBounds.yMax; y++) 
                {
                    if (-y >= 0 && -y < fullLevel.Length && x >= 0 && x < fullLevel[0].Length) 
                    {
                        //전체 맵 설계도에서는 0이면서 새로 로딩했을 때 타일이 있는 상태라면 해당 타일을 null로 변경
                        if (fullLevel[-y][x] == '0' && masterTilemap.HasTile(new Vector3Int(x, y, 0)))
                        {
                            masterTilemap.SetTile(new Vector3Int(x, y, 0), null);
                        }
                    }
                }
            }

            //개별 방의 타일맵 오브젝트는 비활성화
            sourceTilemap.gameObject.SetActive(false);
        }
        
        //다음 물리 프레임이 바닥을 생성할 때 까지 대기
        yield return new WaitForFixedUpdate();
        
        //물리 프레임 실행 이후, 스폰 컨테이너 확인
        if (spawnContainer != null)
        {
            //중간에 삭제과정이 있기 때문에 뒤에서부터 역으로 검사 진행
            for (int i = spawnContainer.transform.childCount - 1; i >= 0; i--) 
            {
                //객체를 하나 찾아서 가져옴
                Transform child = spawnContainer.transform.GetChild(i);
                
                //컨테이너의 자식 객체(몬스터, 아이템, 함정)의 격자 좌표를 가져옴
                Vector3Int targetCell = masterTilemap.WorldToCell(child.position - new Vector3(0.4f, 0.4f, 0));
            
                //좌표 범위에 문제가 없다면
                if (-targetCell.y >= 0 && -targetCell.y < fullLevel.Length && targetCell.x >= 0 && targetCell.x < fullLevel[0].Length) 
                {
                    //오브젝트의 격자 좌표에 D로 표기되어있다면 객체 삭제처리
                    if (fullLevel[-targetCell.y][targetCell.x] == 'D') Destroy(child.gameObject);
                }
            }
        
            //컨테이너를 활성화 해서 삭제가 안된 오브젝트들을 출력
            spawnContainer.SetActive(true);
        }

        //현재 방을 로딩이 완료된 방 리스트에 넣고, 로딩중인 방 리스트에서 제거함
        loadedRooms.Add(room);
        loadingRooms.Remove(room);
    }

    //모든 타일 중, 시작포인트를 찾아서 플레이어를 스폰해주는 메소드
    private void SpawnPlayerDynamically()
    {
        //전체 타일을 돌면서 확인
        for (int y = 0; y < RoomHeight; y++)
        {
            for (int x = 0; x < fullLevel[y].Length; x++)
            {
                if (fullLevel[y][x] == 'S')
                {
                    //월드좌표로 변환 후, 플레이어 스폰
                    Vector3 worldPos = new Vector3(x + 1f, -y + 1f, 0); 
                    GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
                    
                    //플레이어 위치 추적 시작
                    playerTransform = playerObj.transform;
                    return;
                }
            }
        }
    }
    
    //현재 방을 중심으로, 주변 방들을 로딩하는 메소드
    private void UpdateChunks(Vector2Int centerRoom)
    {
        //현재 방을 중심으로, 주변 방 범위에서 벗어난 방을 제거하고, 새로 들어온 방을 로딩해줌
        UnLoadRoom(centerRoom);
        LoadRoom(centerRoom);
    }

    private void UnLoadRoom(Vector2Int centerRoom)
    {
        //주변 방 목록을 가져옴
        List<Vector2Int> desiredRooms = GetDesiredRooms(centerRoom);
        //UnLoad 리스트 생성
        List<Vector2Int> roomsToUnload = new List<Vector2Int>();
    
        //현재 로딩이 끝난 방들중에서 주변 방 목록에 없는 방을 확인함
        foreach (var loadedRoom in loadedRooms)
            if (!desiredRooms.Contains(loadedRoom))
                //주변 방 목록에 없는 방이 있다면 UnLoad 리스트에 추가
                roomsToUnload.Add(loadedRoom);

        //UnLoad 리스트에 들어있는 방들을 타일맵에서 지우고, 메모리 해제
        foreach (var room in roomsToUnload)
            UnloadAndClearRoom(room);
    }
    
    //전달받은 좌표의 방을 메인 타일맵에서 지우고 메모리에서도 제거
    private void UnloadAndClearRoom(Vector2Int room)
    {
        //로딩된 방을 범위별로 저장해둔 목록에서 현재 방을 찾음
        if (roomBoundsCache.TryGetValue(room, out BoundsInt bounds) && masterTilemap != null)
        {
            //현재 방이 확인됐다면 방의 범위만큼 메인 타일맵에서 전부 null 처리
            TileBase[] nullBlock = new TileBase[bounds.size.x * bounds.size.y * bounds.size.z];
            masterTilemap.SetTilesBlock(bounds, nullBlock);
            
            //목록에서 현재 방 삭제
            roomBoundsCache.Remove(room);
        }

        //삭제한 방의 씬을 메모리에서도 제거해서 메모리 공간 확보
        string roomName = $"Room_{room.x}_{room.y}";
        SceneManager.UnloadSceneAsync(roomName);
    
        //로딩된 방 목록에서도 제거
        loadedRooms.Remove(room);
    }

    private void LoadRoom(Vector2Int centerRoom)
    {
        //주변 방 목록을 가져옴
        List<Vector2Int> desiredRooms = GetDesiredRooms(centerRoom);

        //주변 방 목록 확인
        foreach (var room in desiredRooms)
        {
            //로딩이 다 된 방도, 로딩중인 방도 아니라면
            if (!loadedRooms.Contains(room) && !loadingRooms.Contains(room))
            {
                //씬을 불러와서 메인 타일맵에 그대로 그려줌
                StartCoroutine(LoadAndBakeRoomRoutine(room));
            }
        }
    }

    //타일맵 크리에이터쪽에서 호출하는 데이터 저장용 메소드
    public void SaveLevelData(string[] serializedData)
    {
        savedLevelData = serializedData;
    }

    //월드 좌표를 격자 좌표로 변환해주는 메소드
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        //메인 타일맵의 월드 -> 격자 좌표 변환 기능 이용
        if (masterTilemap != null)
        {
            return masterTilemap.WorldToCell(worldPos);
        }
        
        //혹시 타일맵이 정상적으로 로딩되지 않은 경우를 위한 내림 연산을 통한 직접 변환 과정
        return new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), 0);
    }

    //폭탄 폭발 시 호출하는 폭탄 범위 타일맵 제거 메소드
    public void ExplodeTilesInWorld(Vector3 bombWorldPos, int range, GameObject effectPrefab)
    {
        if (fullLevel == null || masterTilemap == null) return;

        //폭탄의 좌표를 격자 좌표로 변환
        Vector3Int centerCell = masterTilemap.WorldToCell(bombWorldPos);

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
                    //폭탄 범위에 이펙트 출력
                    if (effectPrefab != null)
                    {
                        Vector3 cellWorldPos = masterTilemap.GetCellCenterWorld(targetCell);
                        Instantiate(effectPrefab, cellWorldPos, Quaternion.identity);
                    }

                    //아이템, 도착지 무시
                    char tileType = fullLevel[arrayY][arrayX];
                    if (tileType == 'I' || tileType == 'E') continue;

                    //1번 타일 삭제처리
                    if (tileType == '1')
                    {
                        masterTilemap.SetTile(targetCell, null);
                        fullLevel[arrayY][arrayX] = '0'; 
                    }
                }
            }
        }
    }
    
    //아이템, 함정, 몬스터가 획득 혹은 파괴될 때 호출되어서 D를 마킹하는 메소드
    public void MarkObjectAsDestroyed(Vector3 worldPos)
    {
        if (fullLevel == null || masterTilemap == null) return;

        //월드 좌표를 격자 좌표로 변환
        Vector3Int cell = WorldToCell(worldPos - new Vector3(0.4f, 0.4f, 0));
        int arrayX = cell.x;
        int arrayY = -cell.y; //Y축 보정

        //변환된 좌표가 맵 범위에 문제가 없는지 체크
        if (arrayY >= 0 && arrayY < fullLevel.Length && arrayX >= 0 && arrayX < fullLevel[0].Length)
        {
            //파괴를 의미하는 D로 변경
            fullLevel[arrayY][arrayX] = 'D';
        }
    }
}