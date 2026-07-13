using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 添加Image所在的命名空间

public class CanvasClickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private GameObject cubePrefab;
    
    private RectTransform cardRect;
    private Vector2 startPosition;
    private bool isDragging = false;
    private bool isInsidePanel = true;
    private GameObject currentCube;

    private void Awake()
    {
        cardRect = GetComponent<RectTransform>();
        startPosition = cardRect.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Point Down");
        isDragging = true;
        isInsidePanel = true;
        cardRect.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            eventData.position, 
            canvas.worldCamera, 
            out localPosition))
        {
            cardRect.anchoredPosition = localPosition;
        }

        bool newIsInsidePanel = RectTransformUtility.RectangleContainsScreenPoint(panelRect, eventData.position, canvas.worldCamera);

        if (isInsidePanel && !newIsInsidePanel)
        {
            HideCardAndCreateCube(eventData); // 传入正确的PointerEventData参数
        }
        else if (!isInsidePanel && newIsInsidePanel)
        {
            ShowCardAndDestroyCube();
        }

        isInsidePanel = newIsInsidePanel;

        if (!isInsidePanel && currentCube != null)
        {
            UpdateCubePosition(eventData); // 传入正确的PointerEventData参数
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        if (isInsidePanel)
        {
            cardRect.anchoredPosition = startPosition;
        }
        else
        {
            if (currentCube != null)
            {
                UpdateCubePosition(eventData);
                Destroy(gameObject);
            }
        }
    }

    private bool IsPointInPanel(Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, screenPoint, canvas.worldCamera);
    }

    private void HideCardAndCreateCube(PointerEventData eventData)
    {
        GetComponent<Image>().enabled = false; // 现在可以找到Image类型
        Vector3 worldPosition = ScreenToWorldPosition(eventData.position);
        currentCube = Instantiate(cubePrefab, worldPosition, Quaternion.identity);
    }

    private void ShowCardAndDestroyCube()
    {
        GetComponent<Image>().enabled = true; // 现在可以找到Image类型
        if (currentCube != null)
        {
            Destroy(currentCube);
            currentCube = null;
        }
    }

    private void UpdateCubePosition(PointerEventData eventData)
    {
        if (currentCube != null)
        {
            Vector3 worldPosition = ScreenToWorldPosition(eventData.position);
            currentCube.transform.position = worldPosition;
        }
    }

    private Vector3 ScreenToWorldPosition(Vector2 screenPoint)
    {
        return canvas.worldCamera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, 10f));
    }
}