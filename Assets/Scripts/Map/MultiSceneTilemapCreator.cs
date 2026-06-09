//에디터 전용 동작을 위한 분기처리
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;
using MapGeneration;
using UnityEngine.SceneManagement;

public class MultiSceneTilemapCreator : MonoBehaviour
{
    [SerializeField] TileBase solidTile; //룰 타일
    
    //탈출구, 몬스터, 아이템, 함정 프리팹
    [SerializeField] GameObject exitPrefab;
    [SerializeField] GameObject[] monsterPrefabs;
    [SerializeField] GameObject[] itemPrefabs;
    [SerializeField] GameObject[] trapPrefabs;

    //생성된 타일에 부여할 태그, 레이어
    private string tilemapTag = "Ground"; 
    private string tilemapLayer = "Ground";
    
    private const int RoomWidth = 28;
    private const int RoomHeight = 16;
    private const string RoomSavePath = "Assets/Scenes/Rooms/";

    //유니티 에디터 영역 동작
#if UNITY_EDITOR
    [ContextMenu("16개 씬 랜덤 생성")]
    public void BuildMultiSceneLevel()
    {
        //맵 설계도 생성
        MapGenerator mapGenerator = new MapGenerator();
        char[][] fullLevel = mapGenerator.GenerateValidLevel();

        if (fullLevel == null) return;

        //하나의 설계도를 나눠서 각 방별로 씬으로 만듦
        for (int roomY = 0; roomY < 4; roomY++)
        {
            for (int roomX = 0; roomX < 4; roomX++)
            {
                CreateAndSaveRoomScene(fullLevel, roomX, roomY);
            }
        }
        
        string[] serializedData = new string[fullLevel.Length];
        for (int i = 0; i < fullLevel.Length; i++)
        {
            serializedData[i] = new string(fullLevel[i]);
        }

        ChunkManager chunkManager = Object.FindFirstObjectByType<ChunkManager>();
        if (chunkManager != null)
        {
            chunkManager.SaveLevelData(serializedData);
            EditorUtility.SetDirty(chunkManager); 
            EditorSceneManager.MarkSceneDirty(chunkManager.gameObject.scene); 
            Debug.Log("<color=green>[데이터 주입 성공]</color> 이제 Ctrl+S를 눌러주세요.");
        }
    }

    //전체 설계도, 방 좌표를 받아 씬으로 변경
    private void CreateAndSaveRoomScene(char[][] fullLevel, int roomX, int roomY)
    {
        //비어있는 씬 생성 및 이름 지정
        string sceneName = $"Room_{roomX}_{roomY}";
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        newScene.name = sceneName;

        //Grid라는 빈 오브젝트 생성 후, Grid 컴포넌트 등록
        //이 Grid는 월드 공간을 격자 형태로 쪼개서 타일이 배치되기 좋게 해줌
        GameObject gridObj = new GameObject("Grid");
        Grid grid = gridObj.AddComponent<Grid>();
        
        //Tilemap이라는 빈 오브젝트 생성 후, 그리드의 자식으로 둠
        GameObject tilemapObj = new GameObject("Tilemap");
        tilemapObj.transform.SetParent(gridObj.transform);
        
        //타일맵, 타일맵 렌더러, 타일맵 콜라이더 컴포넌트 등록
        Tilemap tilemap = tilemapObj.AddComponent<Tilemap>();
        tilemapObj.AddComponent<TilemapRenderer>();
        tilemapObj.AddComponent<TilemapCollider2D>();

        //타일맵 오브젝트에 태그, 레이어 등록
        try { tilemapObj.tag = tilemapTag; } catch { }
        int layerIndex = LayerMask.NameToLayer(tilemapLayer);
        if (layerIndex != -1) tilemapObj.layer = layerIndex;

        //빈 오브젝트, 현 방의 월드 좌표 시작점, 전체 맵의 테두리 밖 1칸을 미리 확보
        GameObject spawnContainer = new GameObject("SpawnContainer");
        int startX = roomX * RoomWidth;
        int startY = roomY * RoomHeight;
        int xMin = (roomX == 0) ? -1 : 0;
        int xMax = (roomX == 3) ? RoomWidth : RoomWidth - 1;
        int yMin = (roomY == 0) ? -1 : 0;
        int yMax = (roomY == 3) ? RoomHeight : RoomHeight - 1;

        //Min ~ Max까지 동작하면서 현 방의 모든 칸들 확인
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                //현재 확인하는 타일의 격자 좌표
                int globalX = startX + x;
                int globalY = startY + y;

                //우선 현재 타일을 1로 지정
                char currentTile = '1';
                
                //맵 범위 안에 들어온 현재 타일 좌표는 설계도에서 확인
                //이로 인해 맵 범위 밖 테두리 한칸은 전부 1로 고정
                if (globalX >= 0 && globalX < 112 && globalY >= 0 && globalY < 64)
                {
                    currentTile = fullLevel[globalY][globalX];
                }

                //격자 좌표의 월드좌표 확보
                Vector3Int cellPos = new Vector3Int(globalX, -globalY, 0);
                Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

                //현재 타일이 1인 경우 타일로 채움
                if (currentTile == '1')
                {
                    tilemap.SetTile(cellPos, solidTile);
                }
                
                //각종 오브젝트 스폰은 방 범위 내에서만 진행
                if (x >= 0 && x < RoomWidth && y >= 0 && y < RoomHeight)
                {
                    if (currentTile == 'E')
                    {
                        if (exitPrefab != null) Instantiate(exitPrefab, worldPos + new Vector3(0.5f, 0.5f, 0), Quaternion.identity, spawnContainer.transform);
                    }
                    else if (currentTile == 'I')
                    {
                        GameObject prefab = GetRandomPrefab(itemPrefabs);
                        if (prefab != null) Instantiate(prefab, worldPos, Quaternion.identity, spawnContainer.transform);
                    }
                    else if (currentTile == 'M')
                    {
                        GameObject prefab = GetRandomPrefab(monsterPrefabs);
                        if (prefab != null) Instantiate(prefab, worldPos + new Vector3(0.5f, 0.5f, 0), Quaternion.identity, spawnContainer.transform);
                    }
                    else if (currentTile is '^' or 'v' or '<' or '>')
                    {
                        GameObject prefab = GetRandomPrefab(trapPrefabs);
                        if (prefab != null)
                        {
                            Quaternion rot = Quaternion.identity;
                            if (currentTile == 'v') rot = Quaternion.Euler(0, 0, 180);
                            else if (currentTile == '>') rot = Quaternion.Euler(0, 0, -90);
                            else if (currentTile == '<') rot = Quaternion.Euler(0, 0, 90);
                            Instantiate(prefab, worldPos, rot, spawnContainer.transform);
                        }
                    }
                }
            }
        }
        
        //지정된 경로에 씬 저장하고 닫기
        string fullPath = $"{RoomSavePath}{sceneName}.unity";
        EditorSceneManager.SaveScene(newScene, fullPath);
        EditorSceneManager.CloseScene(newScene, true);
    }

    //프리팹 목록에서 랜덤하게 하나를 반환해주는 메소드
    private GameObject GetRandomPrefab(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0) return null;
        return prefabs[Random.Range(0, prefabs.Length)];
    }
#endif
}