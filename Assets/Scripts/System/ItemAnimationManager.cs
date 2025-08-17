using System.Collections;
using UnityEngine;
using DG.Tweening;

public class ItemAnimationManager : MonoBehaviour
{
    public static ItemAnimationManager Instance { get; private set; }

    [Header("애니메이션 설정")]
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private float bounceDuration = 0.1f;
    [SerializeField] private float bounceStrength = 0.05f;
    [SerializeField] private float spawnDuration = 0.2f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private Ease bounceEase = Ease.OutBounce;

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
    /// 드롭 실패 시 원래 위치로 돌아가는 애니메이션
    /// </summary>
    /// <param name="itemView">돌아갈 아이템의 ItemView</param>
    /// <param name="originalCell">원래 셀의 Transform</param>
    /// <param name="onComplete">애니메이션 완료 시 호출될 콜백</param>
    public void ReturnToOriginalPosition(ItemView itemView, Transform originalCell, System.Action onComplete = null)
    {
        if (itemView == null || originalCell == null) return;

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        Vector3 targetPosition = originalCell.position;

        // 부모를 원래 셀로 변경
        itemRect.SetParent(originalCell);
        
        // 애니메이션으로 원래 위치로 이동
        itemRect.DOMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() => {
                // 로컬 위치를 0으로 설정하여 셀 중앙에 정렬
                itemRect.localPosition = Vector3.zero;
                onComplete?.Invoke();
            });
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
        
        dropSequence.OnComplete(() => {
            onComplete?.Invoke();
        });
    }

    /// <summary>
    /// 한 셀에서 다른 셀로 이동하는 애니메이션
    /// </summary>
    /// <param name="itemView">이동할 아이템의 ItemView</param>
    /// <param name="fromCell">시작 셀의 Transform</param>
    /// <param name="toCell">목표 셀의 Transform</param>
    /// <param name="onComplete">애니메이션 완료 시 호출될 콜백</param>
    public void MoveBetweenCells(ItemView itemView, Transform fromCell, Transform toCell, System.Action onComplete = null)
    {
        if (itemView == null || fromCell == null || toCell == null) return;

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        Vector3 targetPosition = toCell.position;

        // 시작 위치에 배치
        itemRect.SetParent(fromCell);
        itemRect.localPosition = Vector3.zero;

        // 이동 애니메이션
        itemRect.DOMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(() => {
                // 목표 셀로 부모 변경
                itemRect.SetParent(toCell);
                itemRect.localPosition = Vector3.zero;
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

        RectTransform itemRect = itemView.GetComponent<RectTransform>();
        
        // 부모를 목표 셀로 설정
        itemRect.SetParent(targetCell);
        itemRect.localPosition = Vector3.zero;
        
        // 시작 크기를 0으로 설정
        itemRect.localScale = Vector3.zero;
        
        // 크기 애니메이션: 0에서 원래 크기로
        itemRect.DOScale(Vector3.one, spawnDuration)
            .SetEase(Ease.OutBack) // 살짝 튀어오르는 효과
            .OnComplete(() => {
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
    }
}

