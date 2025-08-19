using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ItemAnimationManager : MonoBehaviour
{
    public static ItemAnimationManager Instance { get; private set; }

    [Header("애니메이션 설정")]
    [SerializeField] private float moveDuration = 0.05f;
    [SerializeField] private float bounceDuration = 0f;
    [SerializeField] private float bounceStrength = 0f;
    [SerializeField] private float spawnDuration = 0.2f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Ease bounceEase = Ease.OutBounce;

    // 현재 진행 중인 애니메이션을 추적
    private Dictionary<ItemView, Tween> activeAnimations = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    /// <summary>
    /// 빈칸에 드롭 시 셀 중앙으로 이동하는 애니메이션
    /// </summary>
    /// <param name="itemView">이동할 아이템의 ItemView</param>
    /// <param name="targetCell">목표 셀의 Transform</param>
    /// <param name="onComplete">애니메이션 완료 시 호출될 콜백</param>
    public void MoveToCellCenter(ItemView itemView, Transform targetCell, System.Action onComplete = null)
    {
        if (itemView == null || targetCell == null) return;

        // 같은 아이템의 이전 애니메이션이 있다면 중단
        if (activeAnimations.TryGetValue(itemView, out var existingTween))
        {
            existingTween.Kill();
            activeAnimations.Remove(itemView);
        }

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        Vector3 targetPosition = targetCell.position;

        // 부모를 목표 셀로 변경
        itemRect.SetParent(targetCell);

        // 시퀀스 애니메이션: 이동 → 바운스
        Sequence dropSequence = DOTween.Sequence();
        
        // 1단계: 목표 위치로 이동
        dropSequence.Append(itemRect.DOMove(targetPosition, moveDuration).SetEase(moveEase));
        
        // 2단계: 바운스 효과 (위로 살짝 올라갔다가 내려오기)
        dropSequence.Append(itemRect.DOLocalMoveY(bounceStrength, bounceDuration * 0.5f).SetEase(Ease.OutQuad));
        dropSequence.Append(itemRect.DOLocalMoveY(0f, bounceDuration * 0.5f).SetEase(bounceEase));
        
        // 애니메이션을 추적하고 완료 시 정리
        activeAnimations[itemView] = dropSequence;
        
        dropSequence.OnComplete(() => {
            activeAnimations.Remove(itemView);
            onComplete?.Invoke();
        });
    }



    /// <summary>
    /// 새 아이템이 나타날 때 크기 변화 애니메이션
    /// </summary>
    /// <param name="itemView">생성될 아이템의 ItemView</param>
    /// <param name="targetCell">목표 셀의 Transform</param>
    /// <param name="onComplete">애니메이션 완료 시 호출될 콜백</param>
    public void SpawnItem(ItemView itemView, Transform targetCell, System.Action onComplete = null)
    {
        if (itemView == null || targetCell == null) return;

        // 같은 아이템의 이전 애니메이션이 있다면 중단
        if (activeAnimations.TryGetValue(itemView, out var existingTween))
        {
            existingTween.Kill();
            activeAnimations.Remove(itemView);
        }

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        
        // 부모를 목표 셀로 설정
        itemRect.SetParent(targetCell);
        itemRect.localPosition = Vector3.zero;
        
        // 시작 크기를 0으로 설정
        itemRect.localScale = Vector3.zero;
        
        // 크기 애니메이션: 0에서 원래 크기로
        var scaleTween = itemRect.DOScale(Vector3.one, spawnDuration)
            .SetEase(Ease.OutBack); // 살짝 튀어오르는 효과
        
        // 애니메이션을 추적하고 완료 시 정리
        activeAnimations[itemView] = scaleTween;
        
        scaleTween.OnComplete(() => {
            activeAnimations.Remove(itemView);
            onComplete?.Invoke();
        });
    }

    public void ProduceAndMoveItem(ItemView itemView, Transform producerCell, Transform targetCell, System.Action onComplete = null)
    {
        if (itemView == null || producerCell == null || targetCell == null) return;

        // 같은 아이템의 이전 애니메이션이 있다면 중단
        if (activeAnimations.TryGetValue(itemView, out var existingTween))
        {
            existingTween.Kill();
            activeAnimations.Remove(itemView);
        }

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        
        // 애니메이션 중인 아이템을 최상위에 표시하기 위한 강력한 레이어링
        Canvas itemCanvas = itemView.GetComponent<Canvas>();
        if (itemCanvas != null)
        {
            itemCanvas.sortingOrder = 999; // 매우 높은 sorting order
            itemCanvas.overrideSorting = true; // 강제로 sorting order 적용
        }
        
        // 부모 계층에서도 가장 위로 이동
        itemRect.SetAsLastSibling();
        
        // 애니메이션 중인 아이템을 최상위 캔버스로 이동 (필요시)
        Transform topCanvas = FindTopCanvas();
        if (topCanvas != null)
        {
            itemRect.SetParent(topCanvas);
        }
        
        // 출발 위치 결정: producerCell이 항상 올바른 보드 위치
        Vector3 startPos = producerCell.position;
        
        // 시작 위치로 이동
        itemRect.position = startPos;
        itemRect.localScale = Vector3.zero;
        
        // 시퀀스 애니메이션: 이동 + 크기 변화
        Sequence productionSequence = DOTween.Sequence();
        
        // 이동 애니메이션 (월드 좌표 기준)
        Vector3 targetWorldPos = targetCell.position;
        productionSequence.Join(itemRect.DOMove(targetWorldPos, moveDuration).SetEase(moveEase));
        
        // 크기 변화 애니메이션 (0 → 1)
        productionSequence.Join(itemRect.DOScale(Vector3.one, moveDuration).SetEase(Ease.OutBack));
        
        // 바운스 효과 추가
        productionSequence.Append(itemRect.DOLocalMoveY(bounceStrength, bounceDuration * 0.5f));
        productionSequence.Append(itemRect.DOLocalMoveY(0f, bounceDuration * 0.5f));
        
        // 애니메이션을 추적하고 완료 시 정리
        activeAnimations[itemView] = productionSequence;
        
        // 완료 시 부모 변경 및 콜백
        productionSequence.OnComplete(() => {
            // 원래 목표 셀로 부모 변경
            itemRect.SetParent(targetCell);
            itemRect.localPosition = Vector3.zero;
            
            // 애니메이션 완료 후 sorting order 복원
            if (itemCanvas != null)
            {
                itemCanvas.sortingOrder = 0;
                itemCanvas.overrideSorting = false;
            }
            
            activeAnimations.Remove(itemView);
            onComplete?.Invoke();
        });
    }
    
    // 최상위 캔버스를 찾는 헬퍼 메서드
    private Transform FindTopCanvas()
    {
        // 현재 씬에서 Canvas 컴포넌트를 가진 모든 오브젝트 찾기
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        
        if (canvases.Length == 0) return null;
        
        // sorting order가 가장 높은 캔버스 찾기
        Canvas topCanvas = canvases[0];
        foreach (Canvas canvas in canvases)
        {
            if (canvas.sortingOrder > topCanvas.sortingOrder)
            {
                topCanvas = canvas;
            }
        }
        
        return topCanvas.transform;
    }

    /// <summary>
    /// 애니메이션 설정을 동적으로 변경
    /// </summary>
    public void SetAnimationSettings(float moveDur, float bounceDur, float bounceStr, float spawnDur)
    {
        moveDuration = moveDur;
        bounceDuration = bounceDur;
        bounceStrength = bounceStr;
        spawnDuration = spawnDur;
    }

    /// <summary>
    /// 모든 진행 중인 애니메이션을 즉시 완료
    /// </summary>
    public void CompleteAllAnimations()
    {
        DOTween.Complete(true);
    }

    /// <summary>
    /// 모든 진행 중인 애니메이션을 중지
    /// </summary>
    public void StopAllAnimations()
    {
        DOTween.KillAll();
        activeAnimations.Clear();
    }

    /// <summary>
    /// 특정 아이템에 대한 애니메이션이 진행 중인지 확인
    /// </summary>
    /// <param name="mergeItem">확인할 아이템</param>
    /// <returns>애니메이션 진행 중이면 true</returns>
    public bool HasActiveAnimation(MergeItem mergeItem)
    {
        if (mergeItem == null) return false;

        // activeAnimations에서 해당 MergeItem에 연결된 ItemView 찾기
        foreach (var kvp in activeAnimations)
        {
            ItemView itemView = kvp.Key;
            if (itemView != null && itemView.mergeItem == mergeItem)
            {
                return true;
            }
        }
        return false;
    }
}

