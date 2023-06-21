using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class VirtualScrollRect : ScrollRectFaster
{
    private int firstVisibleElementIndex;
    private int lastVisibleElementIndex;

    private VirtualScrollRectInspector Inspector;

    public delegate void RechedEnd();
    public static event RechedEnd OnReachedEnd;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        Inspector = GetComponent<VirtualScrollRectInspector>();
        AllowAntistalling = Inspector.AllowAntistalling;
    }

    public void InitialiseChatVisibility()
    {
        UpdateVisibleMessages(false);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if (Inspector.FirstItemIndexTMP != null)
        {
            Inspector.FirstItemIndexTMP.text = $"Current Item: {GetLastVisibleElementIndex()}";
        }
    }

    protected override void Decelerating()
    {
        base.Decelerating();

        if (Inspector.FirstItemIndexTMP != null)
        {
            Inspector.FirstItemIndexTMP.text = $"Current Item: {GetLastVisibleElementIndex()}";
        }
        UpdateVisibleMessages();
    }

    private void Update()
    {
        if (Inspector.ActiveTMP != null)
        {
            Inspector.ActiveTMP.text = $"Active: {firstVisibleElementIndex - lastVisibleElementIndex}";
        }

        if (Inspector.VelocityTMP != null)
        {
            Inspector.VelocityTMP.text = $"Velocity: {velocity.y}";
        }

        if (Inspector.UpdateCountTMP != null)
        {
            Inspector.UpdateCountTMP.text = $"Updates: {PositionUpdateCount}";
        }

        if (Inspector.StutteringTMP != null)
        {
            Inspector.StutteringTMP.text = Stuttering ? "STUTTERING!" : "";
        }

        if (Inspector.SizeUpdateTMP != null)
        {
            Inspector.SizeUpdateTMP.text = $"Size Updates: {SizeUpdateCount}";
        }
    }

    public void UpdateVisibleMessages(bool updatePosition = true)
    {
        var firstVisibleIndex = GetFirstVisibleElementIndex();
        var lastVisibleIndex = GetLastVisibleElementIndex();

        if (!firstVisibleIndex.HasValue && !lastVisibleIndex.HasValue)
        {
            return;
        }

        if (firstVisibleIndex == 0 && lastVisibleIndex == 0)
        {
            return;
        }

        var isLastMessage = firstVisibleIndex == content.childCount - 1;
        if (isLastMessage)
        {
            if (OnReachedEnd != null)
            {
                Debug.Log("Reached End");
                Debug.Log("FVI: " + firstVisibleIndex + ", LVI: " + lastVisibleIndex);
                OnReachedEnd();
            }
        }

        if (Inspector.ChatAreaManager.isLoadingPage)
        {
            Debug.Log("Loading Page");
            return;
        }

        var goingUp = velocity.y < 0;

        for (var i = content.childCount - 1; i >= 0; i--)
        {
            //if (i >= Inspector.ChatAreaManager.TotalMessages || i < 0)
            //{
            //    continue;
            //}

            var isBeforeAllowed = i > firstVisibleIndex + Inspector.elementsBeforeVisible;
            var isAfterAllowed = i < lastVisibleIndex - Inspector.elementsAfterVisible;

            var isActive = !isBeforeAllowed && !isAfterAllowed;// i <= firstVisibleIndex + Inspector.elementsBeforeVisible && i >= lastVisibleIndex - Inspector.elementsAfterVisible;

            var obj = content.GetChild(i).gameObject;

            var wasActive = obj.activeSelf;

            if (updatePosition && (i >= lastVisibleIndex - Inspector.elementsAfterVisible - 10 && i < lastVisibleIndex) && !wasActive && isActive && !Inspector.ChatAreaManager.isLoadingPage)
            {
                SizeUpdateCount++;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));
                Debug.Log("Activated CP.Y: " + content.anchoredPosition.y + ", SizeY: " + obj.GetComponent<RectTransform>().sizeDelta.y + ", SID: " + Inspector.ScrollIncrementDivisor + ", YInc: " + obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor, obj);
            }

            if (wasActive && !isActive)
            {
            }

            if (!wasActive && isActive)
            {

            }

            obj.SetActive(isActive);

            if (updatePosition && (goingUp || (i >= lastVisibleIndex - Inspector.elementsAfterVisible - 10 && i < lastVisibleIndex)) && wasActive && !isActive && !Inspector.ChatAreaManager.isLoadingPage)
            {
                SizeUpdateCount++;
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));
                Debug.Log("Deactivated CP.Y: " + content.anchoredPosition.y + ", SizeY: " + obj.GetComponent<RectTransform>().sizeDelta.y + ", SID: " + Inspector.ScrollIncrementDivisor + ", YInc: " + obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor, obj);
            }
        }
    }

    public int? GetFirstVisibleElementIndex()
    {
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        Vector3 viewportBottomLeft = content.InverseTransformPoint(viewportCorners[0]);
        Vector3 viewportTopRight = content.InverseTransformPoint(viewportCorners[2]);

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            RectTransform elementRect = content.GetChild(i) as RectTransform;
            if (!elementRect.gameObject.activeSelf)
            {
                continue;
            }

            Vector3 elementPosition = elementRect.localPosition;

            // Check if element is within viewport boundaries
            if (elementPosition.x > viewportBottomLeft.x &&
                elementPosition.x < viewportTopRight.x &&
                elementPosition.y > viewportBottomLeft.y &&
                elementPosition.y < viewportTopRight.y)
            {
                return i;
            }
        }

        return null;
    }

    public int? GetLastVisibleElementIndex()
    {
        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        Vector3 viewportBottomLeft = content.InverseTransformPoint(viewportCorners[0]);
        Vector3 viewportTopRight = content.InverseTransformPoint(viewportCorners[2]);

        RectTransform elementRect = null;
        var found = false;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            elementRect = content.GetChild(i) as RectTransform;
            if (!elementRect.gameObject.activeSelf)
            {
                continue;
            }

            Vector3 elementPosition = elementRect.localPosition;

            // Check if element is within viewport boundaries
            if (elementPosition.x > viewportBottomLeft.x &&
                elementPosition.x < viewportTopRight.x &&
                elementPosition.y > viewportBottomLeft.y &&
                elementPosition.y < viewportTopRight.y)
            {
                found = true;
            }
            else if (found)
            {
                return i + 1;
            }
        }

        return null;
    }
}
