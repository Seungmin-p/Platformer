using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace MyInventory.UGUI
{
    public class ItemDragManipulatorUGUI : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        //인벤토리 최상위 엘리먼트, 드래그 시 보일 고스트 아이콘
        private RectTransform _screenRoot;
        private GameObject _ghostIcon;
        private RectTransform _ghostRect;
        
        //시작 인덱스, 드래그 상태 플래그
        private int _srcIndex;
        private bool _isDragging;

        //아이템이 존재하는지 확인, 아이템 이동, 아이템 삭제
        private Func<bool> _hasItemCheck;
        private Action<int, int> _onDropRequest;
        private Action<int> _onDeleteRequest;
        
        //분할 팝업 호출용 이벤트
        public event Action<int, int> OnSplitDropRequest;

        private Canvas _cachedCanvas; 
        private Image _targetIconComponent; //주입형 이미지 컴포넌트 캐시 저장소

        private Transform _inventoryMainTransform; 

        private void Awake()
        {
            InventoryUGUIView mainView = GetComponentInParent<InventoryUGUIView>();
            if (mainView != null)
            {
                _inventoryMainTransform = mainView.transform;
            }
        }

        public void Setup(RectTransform screenRoot, int srcIndex, Func<bool> hasItemCheck, 
                          Image srcIconComponent, Action<int, int> onDropRequest, Action<int> onDeleteRequest)
        {
            _screenRoot = screenRoot;
            _srcIndex = srcIndex;
            _hasItemCheck = hasItemCheck;
            _targetIconComponent = srcIconComponent; 
            _onDropRequest = onDropRequest;
            _onDeleteRequest = onDeleteRequest;
            _isDragging = false;

            if (_screenRoot == null)
            {
                Canvas parentCanvas = GetComponentInParent<Canvas>();
                if (parentCanvas != null)
                {
                    _screenRoot = parentCanvas.transform as RectTransform;
                }
            }

        }

        //마우스 클릭 시 실행
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_hasItemCheck == null || !_hasItemCheck() || eventData.button != PointerEventData.InputButton.Left) return;

            _isDragging = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDragging || _targetIconComponent == null || _targetIconComponent.canvas == null) return;
            
            Canvas activeCanvas = _targetIconComponent.canvas;
            Camera uiCamera = (activeCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : activeCanvas.worldCamera;

            // 1. 순수 빈 컨테이너 오브젝트 선제 생성
            _ghostIcon = new GameObject("GhostIcon");
            _ghostRect = _ghostIcon.AddComponent<RectTransform>();
            
            // 2. 부모 계층 구조를 액티브 캔버스로 즉시 편입
            _ghostIcon.transform.SetParent(activeCanvas.transform, false);
            _ghostIcon.transform.SetAsLastSibling(); 
            _ghostIcon.layer = activeCanvas.gameObject.layer; 

            // [핵심 보정] 계층 변경 직후 엔진의 레이아웃 행렬을 강제로 리프레시하여 (0,0) 좌표 오차를 파괴합니다.
            Canvas.ForceUpdateCanvases();

            RectTransform srcIconRect = _targetIconComponent.rectTransform;
            if (srcIconRect != null) _ghostRect.sizeDelta = srcIconRect.sizeDelta;
            else _ghostRect.sizeDelta = new Vector2(100f, 100f);

            _ghostRect.anchorMin = new Vector2(0.5f, 0.5f);
            _ghostRect.anchorMax = new Vector2(0.5f, 0.5f);
            _ghostRect.pivot = new Vector2(0.5f, 0.5f);
            _ghostRect.localScale = Vector3.one;

            // 3. 이미지가 생성되기 전에 커서 위치 로컬 좌표계를 칼같이 선제 선점
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(activeCanvas.transform as RectTransform, eventData.position, uiCamera, out Vector2 localPoint))
            {
                _ghostRect.anchoredPosition = localPoint;
            }

            // 4. 위치 셋업이 100% 끝난 안전 구역 상태에서 비로소 이미지 컴포넌트 결속
            Image ghostImage = _ghostIcon.AddComponent<Image>();
            ghostImage.sprite = _targetIconComponent.sprite;
            ghostImage.color = new Color(1f, 1f, 1f, 0.6f); 
            ghostImage.raycastTarget = false; 

            CanvasGroup canvasGroup = _ghostIcon.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;

            // 5. 최종 렌더 파이프라인 진입 전 완벽한 좌표 고정 플러시
            Canvas.ForceUpdateCanvases();
        }

        //마우스를 움직일 때 실행
        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging || _ghostIcon == null) return;

            //고스트 아이콘의 위치를 마우스 위치와 동기화
            UpdateGhostPosition(eventData.position);
        }

        //마우스 클릭을 땔 때 실행
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _isDragging = false;

            if (_ghostIcon != null)
            {
                Destroy(_ghostIcon);
                _ghostIcon = null;
                _ghostRect = null;
            }

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            if (EventSystem.current != null)
            {
                EventSystem.current.RaycastAll(eventData, raycastResults);
            }
            
            if (raycastResults.Count == 0)
            {
                _onDeleteRequest?.Invoke(_srcIndex);
                return;
            }

            ItemSlotUGUIView targetSlot = FindParentSlot(raycastResults);
            if (targetSlot != null)
            {
                int destIndex = targetSlot.Index;

                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    OnSplitDropRequest?.Invoke(_srcIndex, destIndex);
                }
                else
                {
                    _onDropRequest?.Invoke(_srcIndex, destIndex);
                }
                return;
            }

            if (IsInsideInventoryWindow(raycastResults))
            {
                return;
            }

            _onDeleteRequest?.Invoke(_srcIndex);
        }

        //고스트 아이콘의 위치를 마우스와 동기화 해주는 메소드
        private void UpdateGhostPosition(Vector2 panelPosition)
        {
            if (_ghostRect == null || _targetIconComponent == null || _targetIconComponent.canvas == null) return;
            
            Canvas activeCanvas = _targetIconComponent.canvas;
            Camera uiCamera = (activeCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : activeCanvas.worldCamera;

            // [최종 좌표 오차 파괴] 로컬 좌표 변환 시 발생하던 치우침 현상을 막기 위해, 마우스 스크린 포인트를 곧바로 월드 좌표로 환산하여 3D 스페이스 변환 포지션에 직결합니다.
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(activeCanvas.transform as RectTransform, panelPosition, uiCamera, out Vector3 worldPoint))
            {
                _ghostIcon.transform.position = worldPoint;
            }
        }

        //슬롯을 찾는 메소드
        private ItemSlotUGUIView FindParentSlot(List<RaycastResult> raycastResults)
        {
            foreach (var result in raycastResults)
            {
                ItemSlotUGUIView slot = result.gameObject.GetComponentInParent<ItemSlotUGUIView>();
                if (slot != null) return slot;
            }
            return null;
        }

        //인벤토리 범위 내를 판단하는 메소드
        private bool IsInsideInventoryWindow(List<RaycastResult> raycastResults)
        {
            if (_inventoryMainTransform == null) return false;

            foreach (var result in raycastResults)
            {
                if (result.gameObject.transform.IsChildOf(_inventoryMainTransform))
                {
                    return true;
                }
            }
            return false;
        }
    }
}