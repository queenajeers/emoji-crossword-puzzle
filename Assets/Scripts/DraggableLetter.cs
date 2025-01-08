using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using System;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.UI;

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
    [SerializeField] private float returnTime = 5f;

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

    public GameObject shadow;

    private CanvasGroup canvasGroup;

    public static Action<DraggableLetter> OnLetterDraggingStarted;
    public static Action<DraggableLetter> OnLetterDraggingEnded;
    public static Action<DraggableLetter> OnLetterReturned;

    public bool idleState;
    float gridCellSize;
    float originalCellSize;

    public Vector2Int placedAtLocation;

    public bool placedCorrectly;
    public bool avoidTouch;

    [SerializeField] List<GameObject> correctPlacementEffects;
    [SerializeField] List<GameObject> wrongPlacementEffects;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        returnPosition = rectTransform.localPosition;
        TryGetComponent<CanvasGroup>(out canvasGroup);

    }

    public Vector2 CurrentLocalPosition()
    {
        return rectTransform.localPosition;
    }

    public void SetGridBlockSize(float blockSize)
    {
        gridCellSize = blockSize;
    }

    public void LoadOriginalSizes()
    {
        originalCellSize = rectTransform.sizeDelta.x;
        gridCellSize = GridLayer.Instance.GetCellSize();
    }

    public void LoadOriginalPosition()
    {
        idleState = true;
        originalPosition = rectTransform.localPosition;
        returnPosition = rectTransform.localPosition;
    }
    public void SetReturnPositionOriginalPosition()
    {
        idleState = true;
        returnPosition = originalPosition;
    }

    public void SetToSelectedPosition(Vector2 localPosition)
    {
        idleState = false;
        returnPosition = localPosition;
    }

    public void AssignLetter(char letter)
    {
        letterIndicator.text = letter.ToString();
        this.letter = letter;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (avoidTouch) return;
        if (placedCorrectly) return;
        Debug.Log("#1");
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
        shadow.SetActive(true);
        OnLetterDraggingStarted?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (avoidTouch) return;
        if (placedCorrectly) return;
        Debug.Log("#2");
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
            rectTransform.sizeDelta = Vector2.Lerp(
                rectTransform.sizeDelta,
                Vector2.one * originalCellSize,
                Time.deltaTime * dragSmoothness
            );

        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (avoidTouch) return;
        if (placedCorrectly) return;
        Debug.Log("#3");
        OnLetterDraggingEnded?.Invoke(this);
        isDragging = false;
        returnCoroutine = StartCoroutine(ReturnToOriginalPosition());
    }

    public void GoBackToOriginalPosition(RemoveFromPlacedLocationEvent removeFromPlacedLocationEvent)
    {
        StartCoroutine(GoBackToOriginalPositionCor(removeFromPlacedLocationEvent));
    }

    public void PopIn(float delay)
    {
        transform.localScale = Vector3.one * 0f;
        transform.DOScale(1f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);
    }

    IEnumerator GoBackToOriginalPositionCor(RemoveFromPlacedLocationEvent removeFromPlacedLocationEvent)
    {

        rectTransform.DOPunchRotation(new Vector3(0, 0, 30), 0.5f, 10, .4f);
        yield return new WaitForSeconds(.4f);
        removeFromPlacedLocationEvent(placedAtLocation);
        placedAtLocation = new Vector2Int(-1, -1);
        idleState = true;
        SetReturnPositionOriginalPosition();
        StartCoroutine(ReturnToOriginalPosition());
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        avoidTouch = true;
        float elapsedTime = 0f;
        Vector2 startPosition = rectTransform.localPosition;
        Vector2 currentSize = rectTransform.sizeDelta;
        Vector2 targetSize = Vector2.one * (idleState ? originalCellSize : gridCellSize);
        while (true)
        {
            elapsedTime += Time.deltaTime; // Increase elapsed time by time passed
                                           // Apply SmoothStep with the normalized time

            float x = Mathf.Clamp(elapsedTime / returnTime, 0f, 1f);

            x = x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
            // Lerp position and size with smooth transition
            rectTransform.localPosition = Vector2.Lerp(startPosition, returnPosition, x);
            rectTransform.sizeDelta = Vector2.Lerp(currentSize, targetSize, x);
            if (x >= 1f)
            {
                if (!idleState)
                {
                    OnLetterReturned?.Invoke(this);
                    transform.DOScale(.85f, .2f).SetEase(Ease.OutBack).OnComplete(() =>
                    {
                        transform.DOScale(1f, .2f).SetEase(Ease.OutBack);
                    });
                }
                if (idleState)
                {
                    avoidTouch = false;
                }
                shadow.SetActive(idleState);
                break;
            }
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

    public void ActivateCorrectPlacementEffects()
    {
        foreach (var effect in correctPlacementEffects)
        {
            effect.SetActive(true);
        }
        FadeOut();
    }
    public void ActivateWrongPlacementEffects()
    {
        foreach (var effect in wrongPlacementEffects)
        {
            effect.SetActive(true);
        }
    }

    void FadeOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0, .4f)
            .SetDelay(.55f);
        }
    }
}