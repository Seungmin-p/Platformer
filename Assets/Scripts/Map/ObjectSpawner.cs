using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    //방 단위로 각종 오브젝트를 스폰해주는 메소드
    //방 전체 정보, 방의 좌표(몇번째 방인지), 시작방 플래그를 매개변수로 전달받음
    public static void SpawnObjInRoom(char[][] roomGrid, int roomX, int roomY, bool isStartRoom)
    {
        int roomWidth = roomGrid[0].Length;
        int roomHeight = roomGrid.Length;

        float dice;
        char spawnType;
        
        int monsterCount = 0;
        int trapCount = 0;
        int itemCount = 0;

        for (int y = 1; y < roomHeight; y++)
        {
            for (int x = 0; x < roomWidth; x++)
            {
                //당연히 빈 공간에만 스폰해야함
                if (roomGrid[y][x] == '0')
                {
                    //각 빈칸별로 랜덤값 체크
                    dice = Random.Range(0f, 100f);
                    spawnType = ' ';
                    
                    //시작방이 아니라면 몬스터, 트랩 스폰, 이때 몬스터라면 2칸 이상의 땅이 있는지 체크 필요
                    //트랩이라면 트랩 종류에 따른 로직이 들어가야 하는데, 현재는 스파이크 함정만 있으니까 간단하게

                    if (dice < 5f)
                        spawnType = 'I';
                    else if (dice < 8f)
                        spawnType = 'M';
                    else if (dice < 13f)
                        spawnType = 'T';
                    
                    //아이템 스폰은 시작방이여도 상관 없음
                    if (spawnType == 'I' && itemCount < 3)
                    {
                        roomGrid[y][x] = 'I';
                        itemCount++;
                    }
                    
                    if (!isStartRoom)
                    {
                        //3% 확률로 몬스터 랜덤 생성, 만약 이미 3마리가 스폰됐다면 패스
                        if (spawnType == 'M' && monsterCount < 3)
                        {
                            //몬스터 스폰하고, 몬스터 카운트 늘림
                            //몬스터 스폰은 아래 발판이 제대로 있어야하고, 스폰 공간이 충분해야함
                            if (IsSizeSafeAndGrounded(roomGrid, x, y, 2, 2) )
                            {
                                roomGrid[y][x] = 'M';
                                monsterCount++;
                            }
                        }
                        //5% 확률로 함정 생성
                        else if (spawnType == 'T' && trapCount < 5)
                        {
                            //추후 새 트랩을 추가하게 되면, 한번 더 랜덤값 돌려서 어떤 함정으로 할지 정해야함
                            //스파이크 방향 체크는 아래, 위, 왼쪽, 오른쪽 순서
                            if ( y + 1 < roomHeight && roomGrid[y+1][x] == '1')
                            {
                                roomGrid[y][x] = '^';
                                trapCount++;
                            } 
                            else if ( y - 1 >= 0 && roomGrid[y-1][x] == '1')
                            {
                                roomGrid[y][x] = 'v';
                                trapCount++;
                            }
                            else if ( x - 1 >= 0 && roomGrid[y][x-1] == '1')
                            {
                                roomGrid[y][x] = '>';
                                trapCount++;
                            }
                            else if ( x + 1 < roomWidth && roomGrid[y][x+1] == '1')
                            {
                                roomGrid[y][x] = '<';
                                trapCount++;
                            }
                        }
                    }
                }
            }
        }
    }
    
    //어떤 크기의 물체를 현재 좌표에 설치할 수 있는지 체크하는 메소드
    //일단은 결합도를 위해 맵 생성기에 있는 메소드 그대로 복사
    //추후 수정 가능
    private static bool IsSizeSafeAndGrounded(char[][] grid, int x, int y, int width, int height)
    {
        //설치하려는 공간이 0보다 작거나 방 크기보다 크면 실패
        if (x < 0 || x + (width - 1) >= grid[0].Length) return false;
        if (y - (height - 1) < 0 || y + 1 >= grid.Length) return false;

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
            if (grid[y + 1][x + w] == '0') return false;
        }

        //모두 문제 없다면 통과
        return true;
    }
}