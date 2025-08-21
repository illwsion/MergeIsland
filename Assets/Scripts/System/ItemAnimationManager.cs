using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI; // Image 컴포넌트를 사용하기 위해 추가

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
    /// 타겟 아이템 주변에 충격파 이펙트 생성
    /// </summary>
    /// <param name="targetItemView">타겟 아이템의 ItemView</param>
    /// <param name="attackType">공격 타입 (도끼, 곡괭이, 무기 등)</param>
    /// <param name="onComplete">이펙트 완료 시 호출될 콜백</param>
    public void CreateShockwaveEffect(ItemView targetItemView, string attackType = "default", System.Action onComplete = null)
    {
        if (targetItemView == null) return;

        // 공격 타입에 따른 색상 결정
        Color shockwaveColor = GetShockwaveColor(attackType);
        
        // 충격파 이펙트 생성
        StartCoroutine(CreateShockwaveCoroutine(targetItemView, shockwaveColor, onComplete));
    }
    
    /// <summary>
    /// 공격 타입에 따른 충격파 색상 반환
    /// </summary>
    private Color GetShockwaveColor(string attackType)
    {
        switch (attackType.ToLower())
        {
            case "axe":
                return new Color(0.6f, 0.4f, 0.2f); // 갈색 (나무)
            case "pickaxe":
                return new Color(0.5f, 0.5f, 0.5f); // 회색 (돌/철)
            case "weapon":
                return new Color(0.8f, 0.2f, 0.2f); // 빨간색 (몬스터)
            case "merge":
                return new Color(1.0f, 0.8f, 0.0f); // 노란색 (머지)
            case "supply":
                return new Color(1.0f, 0.8f, 0.0f); // 노란색 (공급)
            default:
                return new Color(0.7f, 0.7f, 0.7f); // 기본 회색
        }
    }
    
    /// <summary>
    /// 충격파 이펙트를 생성하는 코루틴
    /// </summary>
    private IEnumerator CreateShockwaveCoroutine(ItemView targetItemView, Color color, System.Action onComplete)
    {
        // 타겟 아이템의 위치 가져오기
        Vector3 targetPosition = targetItemView.transform.position;
        
        // 충격파 이펙트 오브젝트 생성
        GameObject shockwaveObj = new GameObject("ShockwaveEffect");
        shockwaveObj.transform.position = targetPosition;
        
        // 이펙트 오브젝트가 마우스 이벤트를 차단하지 않도록 설정
        CanvasGroup effectCanvasGroup = shockwaveObj.AddComponent<CanvasGroup>();
        effectCanvasGroup.blocksRaycasts = false;
        effectCanvasGroup.interactable = false;
        
        // UI 레이어에 배치하기 위해 Canvas의 자식으로 설정
        Canvas topCanvas = FindTopCanvas()?.GetComponent<Canvas>();
        if (topCanvas != null)
        {
            shockwaveObj.transform.SetParent(topCanvas.transform);
        }
        
        // 충격파 이미지 생성 (원형)
        GameObject imageObj = new GameObject("ShockwaveImage");
        imageObj.transform.SetParent(shockwaveObj.transform);
        imageObj.transform.localPosition = Vector3.zero;
        
        // Image 컴포넌트 추가
        Image shockwaveImage = imageObj.AddComponent<Image>();
        shockwaveImage.color = color;
        
        // 원형 이미지 생성 (프로그래밍 방식으로 원형 만들기)
        CreateCircleImage(shockwaveImage);
        
        // RectTransform 설정
        RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(0.2f, 0.2f); // 시작 크기를 작게 (50 → 20)
        
        // 충격파 애니메이션: 크기 증가 + 투명도 감소
        Sequence shockwaveSequence = DOTween.Sequence();
        
        // 크기 증가 (20 → 80) - 아이템 주위에만 나타나도록 작게
        shockwaveSequence.Join(rectTransform.DOSizeDelta(new Vector2(1.5f, 1.5f), 0.3f).SetEase(Ease.OutQuad));
        
        // 투명도 감소 (1 → 0)
        shockwaveSequence.Join(shockwaveImage.DOFade(0f, 0.3f).SetEase(Ease.OutQuad));
        
        // 애니메이션 완료 후 정리
        shockwaveSequence.OnComplete(() => {
            Destroy(shockwaveObj);
            onComplete?.Invoke();
        });
        
        yield return null;
    }
    
    /// <summary>
    /// 원형 이미지를 프로그래밍 방식으로 생성
    /// </summary>
    private void CreateCircleImage(Image image)
    {
        // 간단한 원형 텍스처 생성
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        
        // Sprite 생성 및 적용
        Sprite circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        image.sprite = circleSprite;
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

