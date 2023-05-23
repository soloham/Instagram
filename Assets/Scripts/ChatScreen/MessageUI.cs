using Nobi.UiRoundedCorners;

using System.Collections;
using System.Collections.Generic;

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

    public ChatMessage ChatMessage;

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
    }
}
