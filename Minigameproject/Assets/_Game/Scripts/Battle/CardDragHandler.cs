using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CardData cardData;
    private Vector2 originalPosition;
    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    public BoardManager boardManager;
    public HandManager handManager;
    // [수정] 배치된 카드를 버린 카드 더미에 추가하기 위해 DeckManager 참조 추가
    public DeckManager deckManager;

    public GameObject tilePreviewPrefab;
    // [추가] 카드가 배치된 셀을 덮는 프리팹
    public GameObject placedTilePrefab;
    private List<GameObject> previewTiles = new List<GameObject>();

    private int originalSiblingIndex;

    // [추가-2026.05.18] 드래그 중 카드 알파값. 0이면 카드가 안 보여 UX가 나쁨 → 0.6으로 변경.
    //   값이 마음에 안 들면 인스펙터/상수만 바꿔서 조정 가능.
    private const float DragAlpha = 0f;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(CardData data)
    {
        cardData = data;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rect.anchoredPosition;
        originalSiblingIndex = transform.GetSiblingIndex();

        // [추가-2026.05.18] Hand에 LayoutGroup이 붙어있는 경우, 카드를 Hand의 자식으로 둔 채
        //   anchoredPosition을 바꿔봐야 LayoutGroup이 매 프레임 자기 슬롯으로 끌어와서
        //   드래그가 동작 안 하거나 다른 카드들 위치까지 영향을 받음.
        //   드래그 동안엔 카드를 Canvas 루트로 잠시 옮겨 LayoutGroup 통제에서 빼냄.
        //   worldPositionStays=true → 화면상 위치는 그대로 유지된 채 부모만 바뀜.
        if (canvas != null)
            transform.SetParent(canvas.transform, true);

        transform.SetAsLastSibling();
        canvasGroup.alpha = DragAlpha;
        canvasGroup.blocksRaycasts = false;
    }
    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;

        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            DestroyPreview();
            return;
        }

        CreatePreview(cellIndex);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyPreview();

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        int cellIndex = GetNearestCellIndex(eventData.position);

        if (cellIndex == -1)
        {
            // [수정-2026.05.18] OnBeginDrag에서 Canvas로 옮겼던 카드를 다시 Hand로 복귀시켜야 함.
            //   단순 anchoredPosition 복구만으로는 부족 (부모가 Canvas라 Hand 좌표계랑 다름).
            ReturnToHand();
            return;
        }

        bool success = boardManager.PlaceCard(cardData, cellIndex);

        if (success)
        {
            Debug.Log($"{cardData.cardName} 배치 성공 : {cellIndex}");
            // [수정] 배치 성공 시 버린 카드 더미에 추가 (덱 리셔플 동작을 위해)
            deckManager?.DiscardCard(cardData);
            // [추가] 배치된 셀 위에 프리팹 덮어씌우기
            PlaceOverlay(cellIndex);
            // [수정] SetActive(false)만 하면 handCards 리스트에 잔존하는 버그 수정
            //        RemoveCard를 호출해 리스트에서도 제거하고 손패 재정렬
            handManager.RemoveCard(GetComponent<CardView>());
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"{cardData.cardName} 배치 실패");
            // [수정-2026.05.18] 취소 분기와 동일한 사유로 ReturnToHand 호출.
            ReturnToHand();
        }
    }

    // [추가-2026.05.18] 드래그 취소/배치 실패 시 카드를 Hand로 복귀시키는 헬퍼.
    //   Hand에 LayoutGroup이 붙어있으면 reparent만 해도 LayoutGroup이 다음 프레임에
    //   자기 규칙대로 카드를 슬롯에 끼워넣음. SetSiblingIndex로 원래 자리에 복귀.
    //   LayoutGroup이 없다면 ArrangeHand가 anchoredPosition을 잡아줌.
    private void ReturnToHand()
    {
        if (handManager != null && handManager.handArea != null)
        {
            // worldPositionStays=false → Hand 좌표계로 즉시 들어가도록.
            //   (true로 두면 드래그 끝난 화면 위치를 유지한 채 좌표 변환되어 한 프레임 시각적 점프가 보일 수 있음)
            transform.SetParent(handManager.handArea, false);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        rect.anchoredPosition = originalPosition;
        handManager.ArrangeHand();
    }

    // [추가] 배치 성공 시 해당 셀들 위에 placedTilePrefab을 인스턴스화해 덮음
    private void PlaceOverlay(int cellIndex)
    {
        if (placedTilePrefab == null) return;

        int startRow = cellIndex / BoardManager.Width;
        int startCol = cellIndex % BoardManager.Width;

        foreach (Vector2Int offset in cardData.tileShape)
        {
            int targetCol = startCol + offset.x;
            int targetRow = startRow + offset.y;

            if (targetCol < 0 || targetCol >= BoardManager.Width ||
                targetRow < 0 || targetRow >= BoardManager.Height)
                continue;

            int targetIndex = targetCol + targetRow * BoardManager.Width;
            GameObject cell = boardManager.gridCells[targetIndex];

            if (cell == null) continue;

            GameObject overlay = Instantiate(placedTilePrefab, cell.transform);
            RectTransform overlayRect = overlay.GetComponent<RectTransform>();

            // [수정-2026.05.18] sizeDelta = cellRect.sizeDelta 방식은 셀이 Stretch 앵커나
            //   GridLayoutGroup 영향을 받을 때 잘못된 값(0이나 패딩값)이 들어가 오버레이가 안 맞음.
            //   앵커를 (0,0)~(1,1)로 깔고 offset/sizeDelta를 0으로 두면
            //   부모(=셀) 크기에 자동으로 꽉 차서 안전함.
            FitToParent(overlayRect);
        }
    }

    private void CreatePreview(int cellIndex)
    {
        DestroyPreview();

        bool canPlace = boardManager.CanPlace(cardData, cellIndex);
        Color previewColor = canPlace ? Color.green : Color.red;
        previewColor.a = 0.45f;

        // [수정] 하드코딩된 3, 4 대신 BoardManager 상수 사용 (보드 크기 변경 시 자동 반영)
        int startRow = cellIndex / BoardManager.Width;
        int startCol = cellIndex % BoardManager.Width;

        foreach (Vector2Int offset in cardData.tileShape)
        {
            int targetCol = startCol + offset.x;
            int targetRow = startRow + offset.y;

            if (targetCol < 0 || targetCol >= BoardManager.Width ||
                targetRow < 0 || targetRow >= BoardManager.Height)
                continue;

            int targetIndex = targetCol + targetRow * BoardManager.Width;

            GameObject cell = boardManager.gridCells[targetIndex];

            if (cell == null)
                continue;

            GameObject preview = Instantiate(tilePreviewPrefab, cell.transform);
            RectTransform previewRect = preview.GetComponent<RectTransform>();

            // [수정-2026.05.18] PlaceOverlay와 동일한 사유. cellRect.sizeDelta 대신
            //   앵커 stretch로 부모에 꽉 차게 만들어 셀 크기 산출 방식과 무관하게 작동시킴.
            FitToParent(previewRect);

            Image previewImage = preview.GetComponent<Image>();
            if (previewImage != null)
                previewImage.color = previewColor;

            previewTiles.Add(preview);
        }
    }

    // [추가-2026.05.18] RectTransform을 부모(셀) 사이즈에 꽉 차게 맞추는 헬퍼.
    //   sizeDelta 복사 방식의 약점(앵커/레이아웃 의존)을 회피하기 위해 추출.
    private static void FitToParent(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private void DestroyPreview()
    {
        for (int i = 0; i < previewTiles.Count; i++)
        {
            if (previewTiles[i] != null)
                Destroy(previewTiles[i]);
        }

        previewTiles.Clear();
    }

    private int GetNearestCellIndex(Vector2 screenPosition)
    {
        if (boardManager == null || boardManager.gridCells == null)
            return -1;

        // [수정-2026.05.18] 기존엔 셀 중심으로부터의 픽셀 거리 + 하드코딩 threshold(60f)로 판정해서
        //   해상도나 캔버스 스케일이 바뀌면 카드를 정확히 셀 위에 올려도 -1로 빠지는 버그가 있었음.
        //   RectTransformUtility.RectangleContainsScreenPoint를 쓰면 셀 자체 영역 안에 있는지를
        //   직접 판정하므로 해상도/스케일/캔버스 모드(Overlay/Camera/World)와 무관하게 동작함.
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? canvas.worldCamera
            : null;

        for (int i = 0; i < boardManager.gridCells.Length; i++)
        {
            GameObject cell = boardManager.gridCells[i];
            if (cell == null) continue;

            RectTransform cellRect = cell.GetComponent<RectTransform>();
            if (cellRect == null) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(cellRect, screenPosition, cam))
            {
                return i;
            }
        }

        return -1;
    }
}
