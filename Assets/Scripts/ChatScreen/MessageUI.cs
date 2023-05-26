using Assets.Scripts.Helpers;

using Cysharp.Threading.Tasks;

using Nobi.UiRoundedCorners;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public Image ProfilePicture;
    public Image ProfileBGPicture;
    public Image MessageBackground;
    public HorizontalLayoutGroup BodyLayourGroup;
    public TextMeshProUGUI MessageText;
    public GameObject ProfileAreaObject;
    public TextMeshProUGUI StatusText;
    public ImageWithRoundedCorners RoundnessComponent;
    public ImageWithIndependentRoundedCorners IndependentRoundnessComponent;

    public Color OurMessageColor;
    public Color TheirMessageColor;
    public Color ImageLoadingColor;

    public ChatMessage ChatMessage;

    public Image MessagePhoto;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Initialise(ChatMessage chatMessage)
    {
        ChatMessage = chatMessage;
        var isOurs = ChatMessage.From == ProfileManager.Instance.LoggedInProfile;

        if (isOurs)
        {
            ProfilePicture.gameObject.SetActive(false);
            ProfileBGPicture.gameObject.SetActive(false);
            MessageBackground.color = OurMessageColor;

            var parentHLG = MessageBackground.transform.parent.GetComponent<HorizontalLayoutGroup>();
            parentHLG.reverseArrangement = true;
            parentHLG.childAlignment = TextAnchor.UpperRight;
            BodyLayourGroup.childAlignment = TextAnchor.UpperRight;

            var bodyRectTransform = MessageBackground.GetComponent<RectTransform>();
            bodyRectTransform.anchorMin = new Vector2(1, 0.5f);
            bodyRectTransform.anchorMax = new Vector2(1, 0.5f);
            bodyRectTransform.pivot = new Vector2(1, 0.5f);
            bodyRectTransform.anchoredPosition = Vector2.zero;
        }

        MessageText.text = ChatMessage.Message.Text;

        if (ChatMessage.Message != null && ChatMessage.Message.Photos != null && ChatMessage.Message.Photos.Any() && MessagePhoto.sprite == null)
        {
            DisableImageLayoutComponents();

            MessagePhoto.color = ImageLoadingColor;

            float w = ChatMessage.Message.Photos.First().Width;
            float h = ChatMessage.Message.Photos.First().Height;
            MessagePhoto.rectTransform.anchorMax = MessagePhoto.rectTransform.anchorMin;
            MessagePhoto.rectTransform.sizeDelta = new Vector2(w, h);
        }
    }

    public async UniTask OnEnteredView()
    {
        if (ChatMessage.Message != null && ChatMessage.Message.Photos != null && ChatMessage.Message.Photos.Any() && MessagePhoto.sprite == null)
        {
            var photo = ChatMessage.Message.Photos.First();

            await MessagePhotoManager.EnsurePhotoExists(photo.Uri);

            ToggleLayoutComponents(true);

            MessagePhoto.sprite = MessagePhotoManager.LoadSprite(photo.Uri);
            MessagePhoto.preserveAspect = true;
            MessagePhoto.color = Color.white;

            RoundnessComponent.radius = 25;

            await Task.Delay(50);
            DestroyLayoutComponents();
        }
    }

    void DisableImageLayoutComponents()
    {
        MessageBackground.GetComponent<ContentSizeFitter>().enabled = false;
        MessageBackground.transform.parent.GetComponent<HorizontalLayoutGroup>().childControlHeight = false;
        MessageBackground.transform.parent.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;
    }

    void ToggleLayoutComponents(bool enable = false)
    {
        GetComponent<VerticalLayoutGroup>().enabled = enable;
        GetComponent<LayoutElement>().enabled = enable;
        GetComponent<ContentSizeFitter>().enabled = enable;

        GetComponentsInChildren<VerticalLayoutGroup>().ToList().ForEach(x => x.enabled = enable);
        GetComponentsInChildren<HorizontalLayoutGroup>().ToList().ForEach(x => x.enabled = enable);
        GetComponentsInChildren<LayoutElement>().ToList().ForEach(x => x.enabled = enable);
        GetComponentsInChildren<ContentSizeFitter>().ToList().ForEach(x => x.enabled = enable);
    }

    void DestroyLayoutComponents()
    {
        Destroy(GetComponent<VerticalLayoutGroup>());
        Destroy(GetComponent<LayoutElement>());
        Destroy(GetComponent<ContentSizeFitter>());

        GetComponentsInChildren<VerticalLayoutGroup>().ToList().ForEach(x => Destroy(x));
        GetComponentsInChildren<HorizontalLayoutGroup>().ToList().ForEach(x => Destroy(x));
        GetComponentsInChildren<LayoutElement>().ToList().ForEach(x => Destroy(x));
        GetComponentsInChildren<ContentSizeFitter>().ToList().ForEach(x => Destroy(x));
    }
}
