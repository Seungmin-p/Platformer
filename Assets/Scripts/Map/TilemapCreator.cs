using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapGeneration
{
    public class TilemapCreator : MonoBehaviour
    {
        public static TilemapCreator Instance { get; private set; }
        public Tilemap MainTilemap => tilemap;
        
        [SerializeField] Tilemap tilemap;
        [SerializeField] TileBase solidTile; //이게 룰 타일
        [SerializeField] int roomType = 1; //방 타입별 생성 테스트용 변수

        [SerializeField] Player playerPrefab;
        [SerializeField] GameObject exitPrefab;
        [SerializeField] GameObject[] monsterPrefabs;
        [SerializeField] GameObject[] itemPrefabs;
        [SerializeField] GameObject[] trapPrefabs;
        
        //스폰된 객체들을 묶어줄 폴더
        [SerializeField] Transform spawnContainer;
        
        private char[][] fullLevel;
        
        //씬에 저장해둘 맵 데이터
        [SerializeField, HideInInspector] string[] savedLevelData;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            //savedLevelData의 데이터를 fullLevel에 등록
            if (savedLevelData != null && savedLevelData.Length > 0)
            {
                fullLevel = new char[savedLevelData.Length][];
                for (int y = 0; y < savedLevelData.Length; y++)
                {
                    fullLevel[y] = savedLevelData[y].ToCharArray();
                }
            }
        }

        //맵 재생성 호출 시, 실행되는 메소드
        [ContextMenu("Generate Tilemap")]
        public void CreateTilemap()
        {
            if (tilemap == null)
            {
                Debug.LogError("Tilemap is not assigned!");
                return;
            }

            //맵 생성 전 클리어 진행
            ClearOldObjects();
            tilemap.ClearAllTiles();

            //맵 생성기 가져오기
            MapGenerator mapGenerator = new MapGenerator();

            //맵 생성 메소드 실행
            //결과적으로 문제없는 맵만 만들어서 반환받게 됨
            fullLevel = mapGenerator.GenerateValidLevel();

            int totalHeight = fullLevel.Length;
            int totalWidth = fullLevel[0].Length;

            //생성된 맵을 유니티가 기억할 수 있도록 1차원 문자열 배열로 저장해둠
            savedLevelData = new string[fullLevel.Length];
            for (int y = 0; y < fullLevel.Length; y++)
            {
                savedLevelData[y] = new string(fullLevel[y]);
            }
            
            //전체 맵 크기에 대해 확인
            for (int y = 0; y < totalHeight; y++)
            {
                for (int x = 0; x < totalWidth; x++)
                {
                    //현재 타일
                    char currentTile = fullLevel[y][x];

                    GeneratorCurrentTile(currentTile, x, y);
                }
            }
        }

        private void GeneratorCurrentTile(char currentTile, int x, int y)
        {
            //2차원 배열의 y좌표 처리 방식 보정 후 월드좌표로 변환
            Vector3Int cellPos = new Vector3Int(x, -y, 0);
            Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);

            //현재 타일을 채워줘야 한다면
            if (currentTile == '1')
            {
                //x방향 처리, y방향 반전 처리를 적용해서 타일 세팅
                int tilemapX = x;
                int tilemapY = -y;
                tilemap.SetTile(new Vector3Int(tilemapX, tilemapY, 0), solidTile);
            }
            //현재 타일이 시작점이라면
            else if (currentTile == 'S')
            {
                if (playerPrefab != null)
                {
                    //2칸 너비의 중앙 정렬을 위해 x에 +0.5f, 발바닥 안착을 위해 y에 +0.5f 보정
                    Vector3 spawnOffset = new Vector3(0.5f, 0.5f, 0);

                    //지정된 좌표에 출력 진행
                    Instantiate(playerPrefab, worldPos + spawnOffset, Quaternion.identity, spawnContainer);
                }
            }
            //현재 타일이 도착점이라면
            else if (currentTile == 'E')
            {
                if (exitPrefab != null)
                {
                    Vector3 spawnOffset = new Vector3(0.5f, 0.5f, 0);
                    Instantiate(exitPrefab, worldPos + spawnOffset, Quaternion.identity, spawnContainer);
                }
            }
            //몬스터, 트랩, 아이템 
            //아이템 랜덤 스폰
            else if (currentTile == 'I')
            {
                GameObject itemPrefab = GetRandomPrefab(itemPrefabs);
                if (itemPrefab != null)
                    Instantiate(itemPrefab, worldPos, Quaternion.identity, spawnContainer);
            }
            //몬스터 랜덤 스폰
            else if (currentTile == 'M')
            {
                GameObject monsterPrefab = GetRandomPrefab(monsterPrefabs);
                if (monsterPrefab != null)
                {
                    Vector3 spawnOffset = new Vector3(0.5f, 0.5f, 0);
                    Instantiate(monsterPrefab, worldPos + spawnOffset, Quaternion.identity, spawnContainer);
                }
            }
            //트랩(스파이크 스폰)
            else if (currentTile is '^' or 'v' or '<' or '>')
            {
                GameObject trapPrefab = GetRandomPrefab(trapPrefabs);
                if (trapPrefab != null)
                {
                    Quaternion spawnRotation = Quaternion.identity;

                    if (currentTile == 'v')
                        spawnRotation = Quaternion.Euler(0f, 0f, 180f);
                    else if (currentTile == '>')
                        spawnRotation = Quaternion.Euler(0f, 0f, -90f);
                    else if (currentTile == '<')
                        spawnRotation = Quaternion.Euler(0f, 0f, 90f);

                    Instantiate(trapPrefab, worldPos, spawnRotation, spawnContainer);
                }
            }
        }

        //프리팹 목록에서 랜덤한 하나를 가져옴
        private GameObject GetRandomPrefab(GameObject[] prefabs)
        {
            if (prefabs.Length <= 0) return null;

            int dice = Random.Range(0, prefabs.Length);

            return prefabs[dice];
        }

        //맵 생성을 시작할 때, 기존 모든 오브젝트 삭제
        private void ClearOldObjects()
        {
            while (spawnContainer.childCount > 0)
            {
                DestroyImmediate(spawnContainer.GetChild(0).gameObject);
            }
        }
        
        //폭탄 스크립트에서 폭탄이 터지면서 호출하는 타일 제거 메소드
        public void ExplodeTiles(Vector3 bombWorldPos, int range, GameObject effectPrefab)
        {
            if (tilemap == null || fullLevel == null) return;

            //폭탄의 격자 좌표 확인
            Vector3Int centerCell = tilemap.WorldToCell(bombWorldPos);

            //상하좌우 반경만큼 확인 진행
            for (int x = -range; x <= range; x++)
            {
                for (int y = -range; y <= range; y++)
                {
                    //맨해튼 거리 규칙 적용
                    //총 칸수가 3칸을 넘어가는지 확인하고, 넘으면 패스
                    if (Mathf.Abs(x) + Mathf.Abs(y) > range)
                        continue;

                    //중심점과 현 x,y 좌표를 더해서 현재 칸의 좌표를 확인함
                    Vector3Int targetCell = centerCell + new Vector3Int(x, y, 0);
                    
                    //이펙트 호출을 위해 현재 격자 좌표의 월드 좌표 확인
                    Vector3 cellWorldPos = tilemap.GetCellCenterWorld(targetCell);

                    //폭발 효과가 있다면 현 위치에 생성
                    if (effectPrefab != null)
                    {
                        Instantiate(effectPrefab, cellWorldPos, Quaternion.identity);
                    }

                    int arrayX = targetCell.x;
                    int arrayY = -targetCell.y;

                    //현재 칸이 맵 범위 밖에 나가지 않는다면
                    if (IsValidCoordinate(arrayX, arrayY))
                    {
                        //현재 칸의 타입 확인
                        char tileType = fullLevel[arrayY][arrayX];

                        //타일인 경우
                        if (tileType == '1')
                        {
                            //현재 타일을 제거하고, 설계도에서도 0으로 변경
                            tilemap.SetTile(targetCell, null); 
                            fullLevel[arrayY][arrayX] = '0'; 
                        }
                    }
                }
            }
        }
        
        //맵 밖은 폭발 범위에서 제외
        private bool IsValidCoordinate(int x, int y)
        {
            if (fullLevel == null) return false;
            return y >= 0 && y < fullLevel.Length && x >= 0 && x < fullLevel[y].Length;
        }
    }
}