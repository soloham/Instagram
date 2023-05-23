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
        //UpdateVisibleMessages();
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

    int oldFirstVisibleIndex = 0;
    int oldLastVisibleIndex = 0;
    public void UpdateVisibleMessages()
    {
        if (Inspector.ChatAreaManager.ChatObjectTemplates == null)
        {
            return;
        }

        if (content.childCount == 0)
        {
            for (int i = 0; i < 50; i++)
            {
                Inspector.ChatAreaManager.InstantiateChatObject(i);
            }

            return;
        }

        var firstVisibleIndex = GetFirstVisibleElementIndex();
        var lastVisibleIndex = GetLastVisibleElementIndex();

        if (oldFirstVisibleIndex == 0)
        {
            oldFirstVisibleIndex = firstVisibleIndex;
        }

        if (oldLastVisibleIndex == 0)
        {
            oldLastVisibleIndex = lastVisibleIndex;
        }

        if (firstVisibleIndex > oldFirstVisibleIndex && velocity.y < 0)
        {
            var lastChatObjectTemplateIndex = int.Parse(content.GetChild(content.childCount - 1).name);
            Inspector.ChatAreaManager.InstantiateChatObject(lastChatObjectTemplateIndex + 1);
        }
        else if (firstVisibleIndex < oldFirstVisibleIndex && velocity.y > 0)
        {
            var obj = content.GetChild(content.childCount - 1);
            //content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));

            Destroy(obj.gameObject);
        }

        if (oldLastVisibleIndex > lastVisibleIndex && velocity.y > 0)
        {
            var firstChatObjectIndex = int.Parse(content.GetChild(0).name);
            var obj = Inspector.ChatAreaManager.InstantiateChatObject(firstChatObjectIndex - 1);

            if (obj != null)
            {
                obj.transform.SetAsFirstSibling();
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - (Inspector.ScrollIncrementDivisor == 0 ? 0 : obj.GetComponent<RectTransform>().sizeDelta.y / Inspector.ScrollIncrementDivisor));
            }
        }
        else if (oldLastVisibleIndex < lastVisibleIndex && velocity.y < 0)
        {
            var obj = content.GetChild(0);
            var objHeight = obj.GetComponent<RectTransform>().sizeDelta.y;

            Destroy(obj.gameObject);

            content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + (Inspector.ScrollIncrementDivisor == 0 ? 0 : objHeight / Inspector.ScrollIncrementDivisor));
        }

        oldFirstVisibleIndex = firstVisibleIndex;
        oldLastVisibleIndex = lastVisibleIndex;
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
