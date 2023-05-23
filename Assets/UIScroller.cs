using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScroller : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
{
    public enum MovementType
    {
        Unrestricted, // Unrestricted movement -- can scroll forever
        Elastic, // Restricted but flexible -- can go past the edges, but springs back in place
        Clamped, // Restricted movement where it's not possible to go past the edges
    }

    [SerializeField]
    private MovementType m_MovementType = MovementType.Elastic;
    public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }


    // The offset from handle position to mouse down position
    private Vector2 m_PointerStartLocalCursor = Vector2.zero;
    private Vector2 m_ContentStartPosition = Vector2.zero;

    [SerializeField]
    private RectTransform m_Content;
    public RectTransform content { get { return m_Content; } set { m_Content = value; } }


    private Bounds m_ContentBounds;
    private Bounds m_ViewBounds;

    private Vector2 m_Velocity;
    public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

    private bool m_Dragging;

    private RectTransform m_ViewRect;

    private DrivenRectTransformTracker m_Tracker;

    [SerializeField]
    private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
    public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }

    private Vector2 m_PrevPosition = Vector2.zero;
    private Bounds m_PrevContentBounds;
    private Bounds m_PrevViewBounds;

    [SerializeField]
    private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic
    public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }

    protected RectTransform viewRect
    {
        get
        {
            if (m_ViewRect == null)
                m_ViewRect = (RectTransform)transform;
            return m_ViewRect;
        }
    }

    public virtual float minWidth { get { return -1; } }
    public virtual float preferredWidth { get { return -1; } }
    public virtual float flexibleWidth { get; private set; }

    public virtual float minHeight { get { return -1; } }
    public virtual float preferredHeight { get { return -1; } }
    public virtual float flexibleHeight { get { return -1; } }

    public virtual int layoutPriority { get { return 1; } }


    // Start is called before the first frame update
    void Start()
    {

    }

    private void UpdatePrevData()
    {
        if (m_Content == null)
            m_PrevPosition = Vector2.zero;
        else
            m_PrevPosition = m_Content.anchoredPosition;
        m_PrevViewBounds = m_ViewBounds;
        m_PrevContentBounds = m_ContentBounds;
    }


    private void EnsureLayoutHasRebuilt()
    {
        if (!CanvasUpdateRegistry.IsRebuildingLayout())
            Canvas.ForceUpdateCanvases();
    }

    protected virtual void LateUpdate()
    {
        if (!m_Content)
            return;

        EnsureLayoutHasRebuilt();
        UpdateBounds();
        float deltaTime = Time.unscaledDeltaTime;
        Vector2 offset = CalculateOffset(Vector2.zero);

        if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
        {
            Vector2 position = m_Content.anchoredPosition;
            for (int axis = 0; axis < 2; axis++)
            {
                // Apply spring physics if movement is elastic and content has an offset from the view.
                if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                {
                    float speed = m_Velocity[axis];
                    position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                    m_Velocity[axis] = speed;
                }
                // If we have neither elaticity or friction, there shouldn't be any velocity.
                else
                {
                    m_Velocity[axis] = 0;
                }
            }

            if (m_Velocity != Vector2.zero)
            {
                if (m_MovementType == MovementType.Clamped)
                {
                    offset = CalculateOffset(position - m_Content.anchoredPosition);
                    position += offset;
                }

                SetContentAnchoredPosition(position);
            }
        }

        if (m_Dragging )
        {
            Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
            m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime); //* m_InertiaVelocityMultuplier);
        }

        if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
        {
            UpdatePrevData();
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        UpdateBounds();

        m_PointerStartLocalCursor = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
        m_ContentStartPosition = m_Content.anchoredPosition;
        m_Dragging = true;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Dragging = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        UpdateBounds();

        var pointerDelta = (localCursor - m_PointerStartLocalCursor); //* m_ScrollFactor;
        Vector2 position = m_ContentStartPosition + pointerDelta;

        // Offset to get content into place in the view.
        Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
        position += offset;
        if (m_MovementType == MovementType.Elastic)
        {
            if (offset.x != 0)
                position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x);
            if (offset.y != 0)
                position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y);
        }

        SetContentAnchoredPosition(position);
    }

    protected virtual void SetContentAnchoredPosition(Vector2 position)
    {
        position.y = m_Content.anchoredPosition.y;

        if (position != m_Content.anchoredPosition)
        {
            m_Content.anchoredPosition = position;
            UpdateBounds();
        }
    }

    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
    }

    private void UpdateBounds()
    {
        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        m_ContentBounds = GetBounds();

        if (m_Content == null)
            return;

        // Make sure content bounds are at least as large as view by adding padding if not.
        // One might think at first that if the content is smaller than the view, scrolling should be allowed.
        // However, that's not how scroll views normally work.
        // Scrolling is *only* possible when content is *larger* than view.
        // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
        // E.g. if pivot is at top, bounds are expanded downwards.
        // This also works nicely when ContentSizeFitter is used on the content.
        Vector3 contentSize = m_ContentBounds.size;
        Vector3 contentPos = m_ContentBounds.center;
        Vector3 excess = m_ViewBounds.size - contentSize;
        if (excess.x > 0)
        {
            contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
            contentSize.x = m_ViewBounds.size.x;
        }
        if (excess.y > 0)
        {
            contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
            contentSize.y = m_ViewBounds.size.y;
        }

        m_ContentBounds.size = contentSize;
        m_ContentBounds.center = contentPos;
    }


    public virtual void SetLayoutHorizontal()
    {
        m_Tracker.Clear();

        m_Tracker.Add(this, viewRect,
                   DrivenTransformProperties.Anchors |
                   DrivenTransformProperties.SizeDelta |
                   DrivenTransformProperties.AnchoredPosition);

        // Make view full size to see if content fits.
        viewRect.anchorMin = Vector2.zero;
        viewRect.anchorMax = Vector2.one;
        viewRect.sizeDelta = Vector2.zero;
        viewRect.anchoredPosition = Vector2.zero;

        // Recalculate content layout with this size to see if it fits when there are no scrollbars.
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
        m_ContentBounds = GetBounds();
    }


    private Vector2 CalculateOffset(Vector2 delta)
    {
        Vector2 offset = Vector2.zero;
        if (m_MovementType == MovementType.Unrestricted)
            return offset;

        Vector2 min = m_ContentBounds.min;
        Vector2 max = m_ContentBounds.max;

        min.x += delta.x;
        max.x += delta.x;
        if (min.x > m_ViewBounds.min.x)
            offset.x = m_ViewBounds.min.x - min.x;
        else if (max.x < m_ViewBounds.max.x)
            offset.x = m_ViewBounds.max.x - max.x;


        return offset;
    }


    private readonly Vector3[] m_Corners = new Vector3[4];
    private Bounds GetBounds()
    {
        if (m_Content == null)
            return new Bounds();

        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        var toLocal = viewRect.worldToLocalMatrix;
        m_Content.GetWorldCorners(m_Corners);
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        m_Velocity = Vector2.zero;
    }

    public void OnScroll(PointerEventData eventData)
    {
        throw new NotImplementedException();
    }

    public void Rebuild(CanvasUpdate executing)
    {
        throw new NotImplementedException();
    }

    public void LayoutComplete()
    {
        throw new NotImplementedException();
    }

    public void GraphicUpdateComplete()
    {
        throw new NotImplementedException();
    }

    public void CalculateLayoutInputHorizontal()
    {
        throw new NotImplementedException();
    }

    public void CalculateLayoutInputVertical()
    {
        throw new NotImplementedException();
    }

    public void SetLayoutVertical()
    {
        throw new NotImplementedException();
    }
}
