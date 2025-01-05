using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [Tooltip("Vertical offset from the cursor/touch position")]
    [SerializeField] private float verticalOffset = 30f;

    [Tooltip("How quickly the element moves to match cursor position (higher = more responsive)")]
    [Range(1f, 20f)]
    [SerializeField] private float dragSmoothness = 10f;

    [Header("Return Animation")]
    [Tooltip("How quickly the element returns to its original position")]
    [Range(0.1f, 10f)]
    [SerializeField] private float returnSpeed = 5f;

    [Tooltip("Smoothing factor for the return animation (higher = smoother)")]
    [Range(0.1f, 10f)]
    [SerializeField] private float returnSmoothness = 2f;

    private Vector2 originalAnchoredPosition;
    private Vector2 currentPointerPosition;
    private Vector2 pointerOffset;
    private bool isDragging = false;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Coroutine returnCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        originalAnchoredPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }

        // Calculate the initial offset between pointer and object position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint
        );

        pointerOffset = (Vector2)rectTransform.anchoredPosition - localPoint;
        // Add vertical offset to the initial pointer offset
        pointerOffset.y += verticalOffset;

        UpdatePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdatePosition(eventData);
    }

    private void UpdatePosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            currentPointerPosition = localPoint + pointerOffset;
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                rectTransform.anchoredPosition,
                currentPointerPosition,
                Time.deltaTime * dragSmoothness
            );
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        returnCoroutine = StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.anchoredPosition;

        while (elapsedTime < returnSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, (elapsedTime / returnSpeed) * returnSmoothness);
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, originalAnchoredPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = originalAnchoredPosition;
    }

    public void ResetPosition()
    {
        isDragging = false;
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        rectTransform.anchoredPosition = originalAnchoredPosition;
    }
}