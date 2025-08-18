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
}

