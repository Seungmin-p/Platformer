using UnityEngine;
using UnityEngine.Tilemaps;

namespace MapGeneration
{
    public class TilemapCreator : MonoBehaviour
    {
        [SerializeField] Tilemap tilemap;
        [SerializeField] TileBase solidTile; //이게 룰 타일
        [SerializeField] int roomType = 1; //방 타입별 생성 테스트용 변수
        
        [SerializeField] Player playerPrefab;
        [SerializeField] GameObject exitPrefab;
        

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
            char[][] fullLevel = mapGenerator.GenerateValidLevel();
    
            int totalHeight = fullLevel.Length;
            int totalWidth = fullLevel[0].Length;

            //전체 맵 크기에 대해 확인
            for (int y = 0; y < totalHeight; y++)
            {
                for (int x = 0; x < totalWidth; x++)
                {
                    //현재 타일
                    char currentTile = fullLevel[y][x];

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
                        //2차원 배열의 y좌표 처리 방식 보정 후 월드좌표로 변환
                        Vector3Int cellPos = new Vector3Int(x, -y, 0);
                        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
    
                        if (playerPrefab != null) 
                        {
                            //2칸 너비의 중앙 정렬을 위해 x에 +0.5f, 발바닥 안착을 위해 y에 +0.8f 보정
                            Vector3 spawnOffset = new Vector3(0.5f, 0.8f, 0);
                            
                            //지정된 좌표에 출력 진행
                            Instantiate(playerPrefab, worldPos + spawnOffset, Quaternion.identity);
                        }
                    }
                    //현재 타일이 도착점이라면
                    else if (currentTile == 'E')
                    {
                        //2차원 배열의 y좌표 처리 방식 보정 후 월드좌표로 변환
                        Vector3Int cellPos = new Vector3Int(x, -y, 0);
                        Vector3 worldPos = tilemap.GetCellCenterWorld(cellPos);
    
                        if (exitPrefab != null) 
                        {
                            Vector3 spawnOffset = new Vector3(0.5f, 0.8f, 0); 
                            Instantiate(exitPrefab, worldPos + spawnOffset, Quaternion.identity);
                        }
                    }
                }
            }
        }
        
        //맵 생성을 시작할 때, 플레이어 및 탈출구 오브젝트 삭제
        private void ClearOldObjects()
        {
            //태그로 기존 맵에 존재하던 두 객체를 찾아서 삭제
            //프레임이 끝날 때 실행되는 Destroy 대신 DestroyImmediate을 통해 실행 즉시 삭제되도록 처리
            //맵 재생성은 아직은 에디터 단계에서 실행되기 때문에 프레임이란 개념이 없음
            GameObject oldPlayer = GameObject.FindGameObjectWithTag("Player");
            if (oldPlayer != null) DestroyImmediate(oldPlayer);

            GameObject oldExit = GameObject.FindGameObjectWithTag("Finish");
            if (oldExit != null) DestroyImmediate(oldExit);
        }
    }
}