using Assets.Scripts.ChatScreen;
using Assets.Scripts.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class VirtualScrollRect : ScrollRectFaster
{
    private int totalElements;
    private int visibleMessages = 16;
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
        UpdateVisibleMessages();
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
    }

    public void UpdateVisibleMessages()
    {
        if (content.childCount == 0)
        {
            for (int i = 0; i < 16; i++)
            {
                Inspector.ChatAreaManager.InstantiateMessage(i);
            }
        }

        var firstVisibleIndex = GetFirstVisibleElementIndex();
        var lastVisibleIndex = GetLastVisibleElementIndex();

        var isLastMessage = firstVisibleIndex == content.childCount - 1;
        if (isLastMessage)
        {
            if (OnReachedEnd != null)
                OnReachedEnd();
        }

        if (Inspector.ChatAreaManager.isLoadingPage)
        {
            return;
        }

        // + 1 to include the new one to be added
        var beforeFirstVisibleIndex = firstVisibleIndex + Inspector.elementsBeforeVisible + 1;

        // - 1 to include the old one to be destroyed
        var afterLastVisibleIndex = lastVisibleIndex - Inspector.elementsAfterVisible - 1;

        if (beforeFirstVisibleIndex > content.childCount - 1)
        {
            ChatMessage lastChatMessage = null;
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                var childMessageUI = child.GetComponent<MessageUI>();
                if (childMessageUI != null)
                {
                    lastChatMessage = childMessageUI.ChatMessage;
                    break;
                }
            }

            Inspector.ChatAreaManager.InstantiateMessage(lastChatMessage, 1);
        }
        else if (beforeFirstVisibleIndex == content.childCount - 1)
        {
            var obj = content.GetChild(beforeFirstVisibleIndex);
            if (!Inspector.ChatAreaManager.isLoadingPage)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));
            }

            Destroy(obj);
        }

        if (afterLastVisibleIndex < 0)
        {
            var firstChatMessage = content.GetChild(0).GetComponent<MessageUI>().ChatMessage;
            Inspector.ChatAreaManager.InstantiateMessage(firstChatMessage, 1);
        }
        else if (afterLastVisibleIndex == 0)
        {
            var obj = content.GetChild(afterLastVisibleIndex);
            var objHeight = obj.GetComponent<RectTransform>().sizeDelta.y;

            Destroy(obj);

            if (!Inspector.ChatAreaManager.isLoadingPage)
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (Inspector.ScrollIncrementDivisor == 0 ? 0 : objHeight / Inspector.ScrollIncrementDivisor));
            }
        }

        //for (var i = ; i >= lastVisibleIndex - Inspector.elementsAfterVisible - 1; i--)
        //{
        //    if (i >= Inspector.ChatAreaManager.TotalMessages || i < 0)
        //    {
        //        continue;
        //    }

        //    var isBeforeAllowed = i > firstVisibleIndex + Inspector.elementsBeforeVisible;
        //    var isAfterAllowed = i < lastVisibleIndex - Inspector.elementsAfterVisible;

        //    var isActive = !isBeforeAllowed && !isAfterAllowed;// i <= firstVisibleIndex + Inspector.elementsBeforeVisible && i >= lastVisibleIndex - Inspector.elementsAfterVisible;

        //    if (i < content.childCount)
        //    {
        //        var obj = content.GetChild(i).gameObject;

        //    }



        //    if (!isActive)
        //    {
        //        Destroy(obj);
        //    }
        //    else
        //    {
        //        var chatMessage = obj.GetComponent<MessageUI>().ChatMessage;

        //        // Workout Chat Message Index To Instantiate
        //    }
        //    obj.SetActive(isActive);

        //    if ((i >= lastVisibleIndex - Inspector.elementsAfterVisible && i < lastVisibleIndex) && isActive && !Inspector.ChatAreaManager.isLoadingPage)
        //    {
        //        content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));
        //    }
        //}
    }

    public int GetFirstVisibleElementIndex()
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

        return content.childCount - 1;
    }

    public int GetLastVisibleElementIndex()
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

        return 0;
    }
}
