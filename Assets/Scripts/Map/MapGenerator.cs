using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration
{
    public enum RoomType
    {
        Side = 0, //별도의 출구가 보장되지 않는 방
        LeftRight = 1, //양쪽 출구가 보장되는 방
        LeftRightBottom = 2, //양쪽 및 아래 출구가 보장되는 방
        LeftRightTop = 3, //양쪽 및 위 출구가 보장되는 방
        LeftRightTopBottom = 4 //위에서 떨어지자마자 다시 떨어져야 할 때 설정하는 상하좌우 출구가 보장되는 방
    }

    public class MapGenerator
    {
        private const int MapWidth = 4;
        private const int MapHeight = 4;
        private const int RoomWidth = 28;
        private const int RoomHeight = 16;
        private static readonly System.Random random = new System.Random();

        private int[,] horizontalDoors; // 좌/우 문의 Y 좌표
        private int[,] verticalDoors; // 상/하 문의 X 좌표
        public int StartRoomX { get; private set; } // 시작 방 위치 추적
        public int EndRoomX { get; private set; } // 끝 방 위치 추적
        
        
        //타일맵 매니저에서 호출할 맵 생성 메소드
        public char[][] GenerateValidLevel()
        {
            char[][] fullLevel = null; //생성 후 반환할 전체 방 구조
            int maxAttempts = 1000; //최대 재생성 시도 횟수
            int attempts = 0; //맵 생성 시도 횟수
            bool isValid = false; //맵 유효성 검사용 변수

            //맵이 아직 완성되지 않았으면서, 시도 횟수도 남아있다면 계속 반복
            while (!isValid && attempts < maxAttempts)
            {
                attempts++;
                //4x4 방의 통로 및 방별 타입 설정
                int[][] blueprint = GenerateMap();
                
                //맵을 생성하고, 하나의 거대한 배열로 가져오기
                fullLevel = AssembleFullLevel(blueprint);

                //전체 맵에 대해 클리어가 가능한지(플레이어가 시작점부터 끝점까지 갈 수 있는지) BFS 탐색 방식으로 검사
                isValid = ValidatePathBFS(fullLevel);

                //만약 검사에 실패하면 이 모든 과정을 다시 진행
                if (!isValid)
                {
                    Debug.Log($"BFS 탐색 실패. 맵을 재생성합니다... (시도: {attempts}/{maxAttempts})");
                }
            }

            if (!isValid)
                Debug.LogWarning("맵 재생성 한도 초과! 경로가 막힌 맵이 그대로 출력됩니다.");
            else
                Debug.Log($"맵 생성 성공! (총 시도 횟수: {attempts})");

            return fullLevel;
        }
        
        //4x4 방의 구조 정하기
        public int[][] GenerateMap()
        {
            //수평, 수직 통로 개수만큼 배열 크기 지정
            //좌우 통로는 세로로 4줄, 가로로 3줄이기 때문에 4,3 || 상하는 반대로 3,4
            horizontalDoors = new int[MapHeight, MapWidth - 1];
            verticalDoors = new int[MapHeight - 1, MapWidth];

            //각 배열을 전부 돌면서 방 사이사이 통로가 될 영역을 미리 정함
            for (int y = 0; y < MapHeight; y++)
            for (int x = 0; x < MapWidth - 1; x++)
                horizontalDoors[y, x] = random.Next(2, 11); //좌우 문 높이 (y: 2~10)

            for (int y = 0; y < MapHeight - 1; y++)
            for (int x = 0; x < MapWidth; x++)
                verticalDoors[y, x] = random.Next(3, 22); //상하 문 위치 (x: 3~21)

            //모든 방을 -1로 초기화
            int[][] map = new int[MapHeight][];
            for (int y = 0; y < MapHeight; y++)
            {
                map[y] = new int[MapWidth];
                for (int x = 0; x < MapWidth; x++)
                {
                    map[y][x] = -1;
                }
            }

            //방 단위의 경로 설정
            GeneratePath(map);

            //아직 타입이 정해지지 않은 방의 타입을 0,1 중에서 랜덤하게 결정
            FillEmptyRooms(map);

            return map;
        }

        //방 단위에서 경로를 설정해주는 메소드
        private void GeneratePath(int[][] map)
        {
            //시작 위치 결정
            int posX = random.Next(MapWidth); 
            int posY = 0;
            StartRoomX = posX;
            
            //우선 시작 위치는 1번 타입의 방으로 진행(변경 가능성 존재)
            map[posY][posX] = (int)RoomType.LeftRight;

            //첫번째 방이라면 1, 아니라면 가장 오른쪽 방인지 검토해서 -1, 아니라면 50% 확률로 -1 혹은 1
            int directionX = (posX == 0) ? 1 : (posX == MapWidth - 1) ? -1 : (random.Next(2) == 0 ? -1 : 1);
            bool justDropped = false; //경로가 방금 아래층으로 내려왔는지 확인하는 변수
            bool isFirstStep = true; //시작 방 판별용 변수

            //현재 확인중인 층이 층 범위 내에 있다면 계속 실행
            while (posY < MapHeight)
            {
                //마지막 층이 아닌경우
                if (posY < MapHeight - 1)
                {
                    //떨어져야 한다는걸 의미함
                    bool timeToDrop = false;

                    //이미 끝방인데 그 방향으로 더 가려고 하는경우, timeToDrop true 설정
                    if ((directionX == -1 && posX == 0) || (directionX == 1 && posX == MapWidth - 1))
                    {
                        timeToDrop = true;
                    }
                    //방금 막 떨어진 상태가 아니라면 떨어지는 방 생성 체크
                    else if (!justDropped)
                    {
                        //시작방이라면 5% 확률로 떨어지게 만듦
                        //방이 옆으로 길어질수록 출발 -> 도착이 어려워지기 때문에 확률을 극단적으로 올려줌
                        if (isFirstStep) timeToDrop = random.Next(100) < 5; // 시작방 95% 유지 규칙
                        //시작방이 아니라면 40% 확률로 떨어지게 만듦
                        else timeToDrop = random.Next(100) < 40; 
                    }
        
                    //시작방 변수 해제
                    isFirstStep = false; 

                    //떨어져야 한다면
                    if (timeToDrop)
                    {
                        //떨어져야 하는데 현재방이 3번방이라면
                        if (map[posY][posX] == (int)RoomType.LeftRightTop) 
                            //아래에 통로 뚫어주기(4번방으로 변경해주기)
                            map[posY][posX] = (int)RoomType.LeftRightTopBottom;
                        else 
                            //3번방이 아니라면 2번방으로 설정해서 떨어지게 만들어주기
                            map[posY][posX] = (int)RoomType.LeftRightBottom; 
            
                        //층수 증가, 이때 마지막층이 되어버리면 다음 루프에서 마지막층 전용 연산 진행
                        posY++;
                        //층수가 증가한 직후, 그러니까 위에서 내려와야 하기 때문에 3번방으로 설정
                        map[posY][posX] = (int)RoomType.LeftRightTop; 
            
                        //다시 한번 같은 방식으로 가야할 방향 지정
                        directionX = (posX == 0) ? 1 : (posX == MapWidth - 1) ? -1 : (random.Next(2) == 0 ? -1 : 1);
            
                        //떨어진 상태 변수 true
                        justDropped = true; 
                    }
                    //떨어지지 않아도 된다면
                    else
                    {
                        //이동 방향에 따른 x좌표 수정
                        posX += directionX;
                        //새로 이동한 방이 아직 설정되지 않은 -1 방이라면 1번방으로 변경
                        if (map[posY][posX] == -1) map[posY][posX] = (int)RoomType.LeftRight;
                        //떨어진 상태 변수 false
                        justDropped = false; 
                    }
                }
                //도착층 전용 로직
                else 
                {
                    //현재 이동중인 방향으로 몇칸 더 갈 수 있는지 계산
                    int availableSteps = (directionX == 1) ? (MapWidth - 1 - posX) : posX;
                    int walkSteps = 0; //이동하려는 칸

                    //더 이동할 수 있다면
                    if (availableSteps > 0)
                    {
                        //95% 확률로 옆으로 더 이동(1번방 유도)
                        //방이 옆으로 길어질수록 출발 -> 도착이 어려워지기 때문에 확률을 극단적으로 올려줌
                        if (random.Next(100) < 95) walkSteps = random.Next(1, availableSteps + 1);
                        else walkSteps = 0;
                    }

                    //더 이동하고자 한다면
                    for (int i = 0; i < walkSteps; i++)
                    {
                        //X 좌표 수정, 만약 아무것도 지정되지 않은 -1방이라면 1번방으로 변경
                        posX += directionX;
                        if (map[posY][posX] == -1) map[posY][posX] = (int)RoomType.LeftRight;
                    }

                    //모든 과정이 끝나면 마지막 방이라는 의미, 마지막 방 지정 후 루프 탈출
                    EndRoomX = posX;
                    break;
                }
            }
        }

        //아직 정해지지 않은 방(-1)을 0이나 1 타입 방으로 설정해주는 메소드
        private void FillEmptyRooms(int[][] map)
        {
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    if (map[y][x] == -1)
                    {
                        //0,1 중에서 방 타입 랜덤 설정
                        map[y][x] = random.Next(2);
                    }
                }
            }
        }
        
        //16개 방의 각 타일들을 하나로 합쳐주는 메소드
        private char[][] AssembleFullLevel(int[][] blueprint)
        {
            //방 개수 x 방 크기
            int totalWidth = MapWidth * RoomWidth;
            int totalHeight = MapHeight * RoomHeight;

            //fullLevel[totalHeight][totalWidth] 생성
            char[][] fullLevel = new char[totalHeight][];
            for (int y = 0; y < totalHeight; y++) fullLevel[y] = new char[totalWidth];

            //각 방을 돌면서 타입에 맞게 방 내부를 채워줌
            for (int mapY = 0; mapY < MapHeight; mapY++)
            {
                for (int mapX = 0; mapX < MapWidth; mapX++)
                {
                    //타입에 맞게 방 내부 생성
                    int roomType = blueprint[mapY][mapX];
                    char[][] roomGrid = GenerateRoom(roomType, mapX, mapY);

                    //roomGrid 이라는 개별 방의 도면을 fullLevel이라는 전체 도면의 알맞은 위치에 넣어줌
                    for (int y = 0; y < RoomHeight; y++)
                    {
                        for (int x = 0; x < RoomWidth; x++)
                        {
                            //방 번호 * 방 크기 + 현재 확인하는 타일에 roomGrid의 타일을 그대로 찍어냄
                            fullLevel[mapY * RoomHeight + y][mapX * RoomWidth + x] = roomGrid[y][x];
                        }
                    }
                }
            }

            return fullLevel;
        }

        //방의 내부를 채우는 메소드
        public char[][] GenerateRoom(int roomType, int mapX, int mapY)
        {
            //이 방의 타입에 맞춰 기본적인 타일 및 통로 배치 진행
            char[][] grid = InitializeGrid(roomType, mapX, mapY);

            //ApplyCellularAutomata 4번 실행
            for (int i = 0; i < 4; i++)
            {
                //현재 방에 CellularAutomata 적용
                grid = ApplyCellularAutomata(grid, mapX, mapY);
            }

            //통로 다시 뚫어주기
            EnforceExits(grid, roomType, mapX, mapY);

            //플레이어 시작점, 게임 출구 배치할 위치 찾기
            PlaceStartAndEndMarkers(grid, mapX, mapY);

            return grid;
        }
        
        //방의 타입에 맞춰 기본적인 타일 및 통로를 배치하는 메소드
        private char[][] InitializeGrid(int roomType, int mapX, int mapY)
        {
            //개별 방 크기에 맞는 배열 및 방 타입에 맞는 확률값 가져오기
            char[][] grid = new char[RoomHeight][];
            float density = GetDensity(roomType);

            //방 전체를 도는 반복문
            for (int y = 0; y < RoomHeight; y++)
            {
                grid[y] = new char[RoomWidth];
                for (int x = 0; x < RoomWidth; x++)
                {
                    //가장 위쪽 방의 위쪽 한줄, 가장 왼쪽방들의 왼쪽 한줄,
                    //가장 오른쪽방의 오른쪽 한줄, 그리고 각 방 아래 두줄을 강제로 타일로 채움
                    bool isGlobalTop = (mapY == 0 && y == 0);
                    bool isGlobalLeft = (mapX == 0 && x == 0);
                    bool isGlobalRight = (mapX == MapWidth - 1 && x == RoomWidth - 1);
                    bool isRoomFloor = (y >= RoomHeight - 2);

                    //조건에 맞다면 타일 생성 준비(1)
                    if (isGlobalTop || isGlobalLeft || isGlobalRight || isRoomFloor)
                        grid[y][x] = '1';
                    else
                        //조건에 맞지 않으면 방 확률에 맞춰서 타일 생성 준비
                        grid[y][x] = random.NextDouble() < density ? '1' : '0';
                }
            }

            //각 방 타입에 맞춰서 통로 생성
            CarveExitTunnels(grid, roomType, mapX, mapY);

            return grid;
        }
        
        //방 타입별 타일 생성 확률을 정해주는 메소드
        private float GetDensity(int roomType)
        {
            switch ((RoomType)roomType)
            {
                case RoomType.Side: return 0.5f;
                case RoomType.LeftRight: return 0.55f;
                case RoomType.LeftRightBottom: return 0.45f;
                case RoomType.LeftRightTop: return 0.45f;
                default: return 0.45f;
            }
        }

        //방 타입에 맞춰서 통로 생성하기
        private void CarveExitTunnels(char[][] grid, int roomType, int mapX, int mapY)
        {
            //문 크기
            int doorSize = 4;

            //0번 타입 방이 아닌경우
            if (roomType != (int)RoomType.Side)
            {
                //가장 왼쪽 방이 아닌 경우 왼쪽 문 뚫기
                if (mapX > 0)
                {
                    //현재 방의 왼쪽 통로 좌표 가져오기
                    int leftDoorY = horizontalDoors[mapY, mapX - 1];
                    
                    //문의 좌표부터 세로로 4칸, 가로로 6칸만큼의 공간을 0으로 지정
                    for (int y = leftDoorY; y < leftDoorY + doorSize; y++)
                    for (int x = 0; x <= 5; x++)
                        grid[y][x] = '0';
                }

                //가장 오른쪽 방이 아닌 경우 오른쪽 문 뚫기
                if (mapX < MapWidth - 1)
                {
                    //현재 방의 오른쪽 통로 좌표 가져오기
                    int rightDoorY = horizontalDoors[mapY, mapX];
                    
                    //문의 좌표부터 세로로 4칸, 가로로 6칸(-)만큼의 공간을 0으로 지정
                    for (int y = rightDoorY; y < rightDoorY + doorSize; y++)
                    for (int x = RoomWidth - 6; x < RoomWidth; x++)
                        grid[y][x] = '0';
                }
            }

            //2,4번방인 경우 아래쪽 문 뚫기
            if (roomType == (int)RoomType.LeftRightBottom || roomType == (int)RoomType.LeftRightTopBottom)
            {
                //가장 아래층 방이 아닌 경우
                if (mapY < MapHeight - 1)
                {
                    //현재 방의 아래쪽 통로 좌표 가져오기
                    int bottomDoorX = verticalDoors[mapY, mapX];
                    
                    //문의 좌표로부터 세로로 8칸, 가로로 4칸 만큼의 공간을 0으로 지정
                    for (int y = RoomHeight / 2; y < RoomHeight; y++)
                    for (int x = bottomDoorX; x < bottomDoorX + doorSize; x++)
                        grid[y][x] = '0';
                }
            }

            //3,4번방인 경우 위쪽 문 뚫기
            if (roomType == (int)RoomType.LeftRightTop || roomType == (int)RoomType.LeftRightTopBottom)
            {
                if (mapY > 0)
                {
                    //현재 방의 위쪽 통로 좌표 가져오기
                    int topDoorX = verticalDoors[mapY - 1, mapX];
                    
                    //문의 좌표로부터 세로로 8칸, 가로로 4칸 만큼의 공간을 0으로 지정
                    for (int y = 0; y < RoomHeight / 2; y++)
                    for (int x = topDoorX; x < topDoorX + doorSize; x++)
                        grid[y][x] = '0';
                }
            }
        }
        
        //셀룰러 오토마타 기법 실행
        private char[][] ApplyCellularAutomata(char[][] grid, int mapX, int mapY)
        {
            //전체 방 크기에 맞춰 새 배열 생성
            char[][] newGrid = new char[RoomHeight][];
            for (int y = 0; y < RoomHeight; y++)
            {
                newGrid[y] = new char[RoomWidth];
                for (int x = 0; x < RoomWidth; x++)
                {
                    //고정된 타일 범위 체크용 변수들
                    bool isGlobalTop = (mapY == 0 && y == 0);
                    bool isGlobalLeft = (mapX == 0 && x == 0);
                    bool isGlobalRight = (mapX == MapWidth - 1 && x == RoomWidth - 1);
                    bool isRoomFloor = (y >= RoomHeight - 2);

                    //현재 확인중인 타일이 고정범위 내에 들어있다면
                    if (isGlobalTop || isGlobalLeft || isGlobalRight || isRoomFloor)
                    {
                        //0인 곳은 통로이기 때문에 0 유지
                        if (grid[y][x] == '0') newGrid[y][x] = '0';
                        else newGrid[y][x] = '1'; //나머지는 벽 유지
                        continue;
                    }

                    //주변 블록 체크
                    int neighbors = CountNeighbors(grid, x, y);
                    
                    //본인 포함 주변 블록이 5개이상일 때, 1 아니면 0
                    newGrid[y][x] = (neighbors >= 5) ? '1' : '0';
                }
            }

            return newGrid;
        }

        //주변 블록중 벽이 얼마나 있는지 체크
        private int CountNeighbors(char[][] grid, int x, int y)
        {
            int count = 0;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < RoomWidth && ny >= 0 && ny < RoomHeight && grid[ny][nx] == '1')
                        count++;
                }
            }

            return count;
        }
        
        //ApplyCellularAutomata 실행 후 추가 보정
        private void EnforceExits(char[][] grid, int roomType, int mapX, int mapY)
        {
            //다시 한번 통로 확실하게 만들어주기
            CarveExitTunnels(grid, roomType, mapX, mapY);
        }

        //start, end 포인트 지정해주기
        private void PlaceStartAndEndMarkers(char[][] grid, int mapX, int mapY)
        {
            //시작 방에는 스타트 포인트 지정
            if (mapY == 0 && mapX == StartRoomX)
            {
                PlaceMarker(grid, 'S');
            }
            //마지막 방에는 엔드포인트 지정
            else if (mapY == MapHeight - 1 && mapX == EndRoomX)
            {
                PlaceMarker(grid, 'E');
            }
        }

        //플레이어 시작 위치, 도착지 위치 지정
        private void PlaceMarker(char[][] grid, char marker)
        {
            int centerX = RoomWidth / 2;

            //천장 y=2부터 바닥까지 스캔
            for (int y = 2; y < RoomHeight - 2; y++)
            {
                //0에서 12까지 하나씩 늘리면서 센터에서 좌우로 한칸씩 넓혀져가면서 탐색(leftX, rightX)
                for (int offset = 0; offset < RoomWidth / 2 - 1; offset++)
                {
                    //leftX는 offset이 늘어날 수록 중앙에서 왼쪽으로 한칸씩 이동
                    //rightX는 offset이 늘어날 수록 중앙에서 오른쪽으로 한칸씩 이동
                    int leftX = centerX - offset;
                    int rightX = centerX + offset;

                    //중앙 부근에 배치하기 위해 중앙부터 시작해서 양방향으로 탐색 진행하는 방식
                    //시작점 마커 동작인 경우
                    if (marker == 'S')
                    {
                        
                        //leftX 부터 체크 진행
                        if (IsSizeSafeAndGrounded(grid, leftX, y, 2, 3))
                        {
                            grid[y][leftX] = marker;
                            return;
                        }

                        //leftX에서 확정이 안된경우 rightX 체크 진행
                        if (IsSizeSafeAndGrounded(grid, rightX, y, 2, 3))
                        {
                            grid[y][rightX] = marker;
                            return;
                        }
                    }
                    //도착지 마커 동작인 경우
                    else if (marker == 'E')
                    {
                        //leftX 부터 체크 진행
                        if (IsSizeSafeAndGrounded(grid, leftX, y, 2, 2))
                        {
                            grid[y][leftX] = marker;
                            return;
                        }

                        //leftX에서 확정이 안된경우 rightX 체크 진행
                        if (IsSizeSafeAndGrounded(grid, rightX, y, 2, 2))
                        {
                            grid[y][rightX] = marker;
                            return;
                        }
                    }
                }
            }

            //만약 배치에 실패하면, y = 2에둬서 BFS 탐색 실패 -> 재생성 유도
            //애초에 y = 2에서 탐색이 실패했기 때문에 이렇게 지정해주면 실패하게 됨
            grid[2][centerX] = marker;
        }

        //어떤 크기의 물체를 현재 좌표에 설치할 수 있는지 체크하는 메소드
        private bool IsSizeSafeAndGrounded(char[][] grid, int x, int y, int width, int height)
        {
            //설치하려는 공간이 0보다 작거나 방 크기보다 크면 실패
            if (x < 0 || x + (width - 1) >= RoomWidth) return false;

            //지정된 좌표와 지정된 범위의 모든 블록을 검사
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    //하나라도 1이 있으면 실패
                    if (grid[y - h][x + w] == '1') return false;
                }
            }

            //모든 범위가 0인 경우 추가 검사
            for (int w = 0; w < width; w++)
            {
                //범위 바로 아래 영역들에 발판이 제대로 존재하는지 확인
                if (grid[y + 1][x + w] == '1') return true;
            }

            return false;
        }

        //공중에 뜬 1칸짜리 블록 등을 제거해주는 메소드
        //추후 이용 가능
        private void RemoveThinSolidSegments(char[][] grid)
        {
            bool[][] keepSolid = new bool[RoomHeight][];
            for (int y = 0; y < RoomHeight; y++)
            {
                keepSolid[y] = new bool[RoomWidth];
            }

            for (int y = 0; y < RoomHeight - 1; y++)
            {
                for (int x = 0; x < RoomWidth - 1; x++)
                {
                    if (grid[y][x] == '1' && grid[y][x + 1] == '1' && grid[y + 1][x] == '1' &&
                        grid[y + 1][x + 1] == '1')
                    {
                        keepSolid[y][x] = true;
                        keepSolid[y][x + 1] = true;
                        keepSolid[y + 1][x] = true;
                        keepSolid[y + 1][x + 1] = true;
                    }
                }
            }

            for (int y = 0; y < RoomHeight; y++)
            {
                for (int x = 0; x < RoomWidth; x++)
                {
                    if (grid[y][x] == '1' && !keepSolid[y][x])
                    {
                        grid[y][x] = '0';
                    }
                }
            }
        }

        //캐릭터의 크기(세로3, 가로2)를 고려한 BFS 탐색 메소드
        private bool ValidatePathBFS(char[][] fullLevel)
        {
            //전체 맵의 높이, 넓이 측정
            int totalHeight = fullLevel.Length;
            int totalWidth = fullLevel[0].Length;

            //각각 시작 위치 및 마지막 위치
            Vector2Int startPos = new Vector2Int(-1, -1);
            Vector2Int endPos = new Vector2Int(-1, -1);

            //맵 전체를 스캔해서 시작포인트, 마지막 포인트의 좌표를 가져옴
            for (int y = 0; y < totalHeight; y++)
            {
                for (int x = 0; x < totalWidth; x++)
                {
                    if (fullLevel[y][x] == 'S') startPos = new Vector2Int(x, y);
                    if (fullLevel[y][x] == 'E') endPos = new Vector2Int(x, y);
                }
            }

            //마커가 제대로 확인이 안돼서 초기화 값인 -1이 확인되는 경우 실패처리
            if (startPos.x == -1 || endPos.x == -1) return false;

            //탐색을 위한 큐, 방문확정용 배열 생성
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            bool[,] visited = new bool[totalHeight, totalWidth];

            //시작 위치를 큐에 넣어주고, 방문 배열에도 방문처리
            queue.Enqueue(startPos);
            visited[startPos.y, startPos.x] = true;

            //플레이어의 최대 점프 높이
            //이동 가능 경로를 찾는데에 쓰이기 때문에 적당한 수치조절 필요
            int maxJump = 6;

            //큐에 뭐가 들어있다면 계속 실행
            while (queue.Count > 0)
            {
                //이번 루프에서 검사학 데이터를 하나 꺼냄
                Vector2Int curr = queue.Dequeue();

                //도착 판정 - 2x3 크기의 플레이어 영역 중 단 한 칸이라도 E 마커에 닿았는지 확인
                bool reachedExit = false;
                for (int h = 0; h < 3; h++)
                {
                    for (int w = 0; w < 2; w++)
                    {
                        int checkY = curr.y - h;
                        int checkX = curr.x + w;

                        if (checkY >= 0 && checkY < totalHeight && checkX >= 0 && checkX < totalWidth)
                        {
                            if (fullLevel[checkY][checkX] == 'E')
                            {
                                reachedExit = true;
                                break;
                            }
                        }
                    }

                    //닿은게 확인됐다면 추가확인은 안해도 됨
                    if (reachedExit) break;
                }

                //닿은게 확인됐다면 시작점부터 도착지까지 도달 가능하다는 의미이므로 true 반환
                if (reachedExit) return true;

                //true 반환이 안됐다면 캐릭터의 위치를 옮기면서 체크해봐야함
                //현재 위치한 곳의 발 밑이 땅인지 확인
                bool isGrounded = false;
                if (curr.y + 1 < totalHeight)
                {
                    if (fullLevel[curr.y + 1][curr.x] == '1' || fullLevel[curr.y + 1][curr.x + 1] == '1')
                    {
                        isGrounded = true;
                    }
                }

                //현재 위치한곳이 땅이라면
                if (isGrounded)
                {
                    //왼쪽, 오른쪽 좌표 한칸씩 Enqueue 시도
                    TryEnqueue(fullLevel, curr.x - 1, curr.y, queue, visited, totalWidth, totalHeight);
                    TryEnqueue(fullLevel, curr.x + 1, curr.y, queue, visited, totalWidth, totalHeight);

                    //점프해서 이동하는 부분도 생각해봐야함
                    for (int h = 1; h <= maxJump; h++)
                    {
                        int jumpY = curr.y - h;

                        //머리(y-2)가 맵 밖(천장 0)으로 나가지 않도록 방어
                        if (jumpY < 2) break;

                        //올라가는 도중 2칸 너비의 머리(y-2)가 천장('1')에 부딪히면 그 이상 점프 불가
                        if (fullLevel[jumpY - 2][curr.x] == '1' || fullLevel[jumpY - 2][curr.x + 1] == '1')
                            break;

                        //수직 점프 및 점프 궤적 중 좌우 이동 Enqueue 시도
                        TryEnqueue(fullLevel, curr.x, jumpY, queue, visited, totalWidth, totalHeight);
                        TryEnqueue(fullLevel, curr.x - 1, jumpY, queue, visited, totalWidth, totalHeight);
                        TryEnqueue(fullLevel, curr.x + 1, jumpY, queue, visited, totalWidth, totalHeight);
                    }
                }
                else //공중 상태라면
                {
                    //중력에 의해 내려가기 때문에 y좌표를 하나 내려서 왼쪽, 중앙, 오른쪽 Enqueue 시도
                    TryEnqueue(fullLevel, curr.x, curr.y + 1, queue, visited, totalWidth, totalHeight);
                    TryEnqueue(fullLevel, curr.x - 1, curr.y + 1, queue, visited, totalWidth, totalHeight);
                    TryEnqueue(fullLevel, curr.x + 1, curr.y + 1, queue, visited, totalWidth, totalHeight);
                }
            }

            //큐에 남은게 없는데 true로 반환이 안됐다면 false 반환
            return false;
        }

        //새 좌표를 전달받아서 문제가 없다면 큐에 넣어주는 메소드
        private void TryEnqueue(char[][] fullLevel, int nx, int ny, Queue<Vector2Int> q, bool[,] v, int tWidth, int tHeight)
        {
            //nx가 가로 2칸을 차지하므로 nx + 1도 맵 경계 안이어야 함
            //ny가 세로 3칸(ny, ny-1, ny-2)을 차지하므로 ny >= 2 여야 함
            if (nx >= 0 && nx + 1 < tWidth && ny >= 2 && ny < tHeight)
            {
                //아직 방문 확정이 안된 곳이라면
                if (!v[ny, nx])
                {
                    //클리어 변수 선언
                    bool isClear = true;
                    
                    //(nx, ny) 좌표에 캐릭터를 세웠을 때, 몸의 면적 6칸중에 단 한칸이라도 타일(1)이 있는지 확인
                    for (int h = 0; h < 3; h++)
                    {
                        //h가 0이라면 가장 아래 두칸, 1이라면 중앙 두칸, 2라면 머리 두칸
                        if (fullLevel[ny - h][nx] == '1' || fullLevel[ny - h][nx + 1] == '1')
                        {
                            isClear = false;
                            break;
                        }
                    }

                    //새 좌표에 별 다른 문제가 없다면
                    if (isClear)
                    {
                        //현 좌표 방문 확정 및 큐에 넣어줌
                        v[ny, nx] = true;
                        q.Enqueue(new Vector2Int(nx, ny));
                    }
                }
            }
        }
    }
}