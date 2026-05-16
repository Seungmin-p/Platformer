using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class LinqGcTest : MonoBehaviour
{
    public class Item
    {
        public string Name;
        public int Price;
        public int Level;
    }

    private List<Item> _items = new List<Item>();

    [Header("테스트 설정")]
    [Tooltip("체크하면 LINQ 실행, 해제하면 for문 실행")]
    public bool useLinq = true; 
    public int itemCount = 5000; 

    // for문에서 재사용할 캐싱 리스트 (배열 재할당 GC 방지)
    private List<Item> _cachedList = new List<Item>(5000);
    
    // for문의 정렬을 위한 대리자(Delegate) 캐싱 (매 프레임 생성되는 GC 방지)
    private System.Comparison<Item> _sortDelegate;

    void Start()
    {
        for (int i = 0; i < itemCount; i++)
        {
            _items.Add(new Item { Name = "Item" + i, Price = Random.Range(10, 10000), Level = Random.Range(1, 60) });
        }

        // 레벨 내림차순 정렬 규칙 미리 정의 (b.Level과 a.Level 비교)
        _sortDelegate = (a, b) => b.Level.CompareTo(a.Level);
    }

    void Update()
    {
        if (useLinq)
        {
            // [LINQ 방식] : 매 프레임 무자비한 배열 생성 및 클로저 할당 발생
            Profiler.BeginSample("1. LINQ_TEST_Linq");
            
            var expensiveItems = _items
                .Where(x => x.Price > 5000)
                .OrderByDescending(x => x.Level)
                .ToList();
                
            Profiler.EndSample();
        }
        else
        {
            // [전통적 for문 방식] : 완벽히 동일한 동작을 수행하지만 GC Alloc은 0
            Profiler.BeginSample("2. LINQ_TEST_for");
            
            // 1. 기존 리스트 비우기 (메모리 재사용)
            _cachedList.Clear(); 
            
            // 2. 조건 검색 (Where 역할)
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].Price > 5000)
                {
                    _cachedList.Add(_items[i]);
                }
            }
            
            // 3. 정렬 (OrderByDescending 역할)
            _cachedList.Sort(_sortDelegate);
            
            Profiler.EndSample();
        }
    }
}