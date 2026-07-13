using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIImageDrag : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 防止拖拽时其他点击事件干扰
        Debug.Log(666666);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 将屏幕坐标转换为Canvas本地坐标并设置Image位置
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPosition))
        {
            rectTransform.anchoredPosition = localPosition;
        }
    }
}