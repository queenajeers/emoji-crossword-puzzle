using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System;

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

    private Vector2 returnPosition;
    private Vector2 originalPosition;
    private Vector2 currentPointerPosition;
    private Vector2 pointerOffset;
    private bool isDragging = false;
    private Canvas parentCanvas;
    private RectTransform rectTransform;
    private Coroutine returnCoroutine;

    [Header("Properties")]
    public TextMeshProUGUI letterIndicator;
    public char letter;

    public static Action<DraggableLetter> OnLetterDraggingStarted;
    public static Action<DraggableLetter> OnLetterDraggingEnded;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        returnPosition = rectTransform.localPosition;
    }

    public Vector2 CurrentLocalPosition()
    {
        return rectTransform.localPosition;
    }

    public void LoadOriginalPosition()
    {
        originalPosition = rectTransform.localPosition;
        returnPosition = rectTransform.localPosition;
    }
    public void SetReturnPositionOriginalPosition()
    {
        returnPosition = originalPosition;
    }

    public void SetReturnPosition(Vector2 localPosition)
    {
        returnPosition = localPosition;
    }

    public void AssignLetter(char letter)
    {
        letterIndicator.text = letter.ToString();
        this.letter = letter;
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

        pointerOffset = (Vector2)rectTransform.localPosition - localPoint;
        // Add vertical offset to the initial pointer offset
        pointerOffset.y += verticalOffset;

        UpdatePosition(eventData);

        OnLetterDraggingStarted?.Invoke(this);
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
            rectTransform.localPosition = Vector2.Lerp(
                rectTransform.localPosition,
                currentPointerPosition,
                Time.deltaTime * dragSmoothness
            );
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        OnLetterDraggingEnded?.Invoke(this);
        isDragging = false;
        returnCoroutine = StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.localPosition;

        while (elapsedTime < returnSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, (elapsedTime / returnSpeed) * returnSmoothness);
            rectTransform.localPosition = Vector2.Lerp(startPosition, returnPosition, t);
            yield return null;
        }

        rectTransform.localPosition = returnPosition;
    }

    public void ResetPosition()
    {
        isDragging = false;
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
        }
        rectTransform.localPosition = returnPosition;
    }
}