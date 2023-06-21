using Assets.Scripts.Helpers;

using Cysharp.Threading.Tasks;

using Nobi.UiRoundedCorners;

using System.Linq;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.Android;
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
    public Color DeletedColor;

    public ChatMessage ChatMessage;

    public Image MessagePhoto;

    public bool isImageLayoutDestroyed;

    public delegate void ManageDelegate(MessageUI messageUI);
    public static event ManageDelegate OnEdit;
    public static event ManageDelegate OnDelete;
    public static event ManageDelegate OnUndelete;

    public GameObject EditHolder;

    public GameObject DeleteButton;
    public GameObject UndeleteButton;

    private Color UnmarkedColor;

    // Start is called before the first frame update
    void Start()
    {
        ProfileManager.OnEditModeChanged += Instance_OnEditModeChanged;
    }

    private void OnDestroy()
    {
        ProfileManager.OnEditModeChanged -= Instance_OnEditModeChanged;
    }

    private void Instance_OnEditModeChanged(bool isEditingAllowed)
    {
        EditHolder.SetActive(isEditingAllowed);
    }

    public void Edit()
    {
        if (OnEdit != null)
        {
            OnEdit.Invoke(this);
        }
    }

    public void Delete()
    {
        if (OnDelete != null)
        {
            OnDelete.Invoke(this);

            DeleteButton.SetActive(false);
            UndeleteButton.SetActive(true);
        }
    }

    public void Undelete()
    {
        if (OnUndelete != null)
        {
            OnUndelete.Invoke(this);

            DeleteButton.SetActive(true);
            UndeleteButton.SetActive(false);
        }
    }

    public void MarkAsDeleted()
    {
        UnmarkedColor = MessageBackground.color;
        MessageBackground.color = DeletedColor;
    }

    public void UnmarkAsDeleted()
    {
        MessageBackground.color = UnmarkedColor;
    }

    public async UniTask Initialise(ChatMessage chatMessage)
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
        else
        {
            ProfilePicture.sprite = ChatMessage.From.PictureBorderless;
        }

        MessageText.text = ChatMessage.Message.Text;

        if (ChatMessage.Message != null && ChatMessage.Message.Photos != null && ChatMessage.Message.Photos.Any() && MessagePhoto.sprite == null)
        {
            var photo = ChatMessage.Message.Photos.First();

            float w = ChatMessage.Message.Photos.First().Width;
            float h = ChatMessage.Message.Photos.First().Height;

            MessageText.enabled = false;

            AdjustPhotoScale(w, h);

            await MessagePhotoManager.EnsurePhotoExists(photo.Uri);

            if (Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                MessagePhoto.sprite = MessagePhotoManager.LoadSprite(photo.Uri);
                MessagePhoto.preserveAspect = true;
                MessagePhoto.color = Color.white;
            }

            RoundnessComponent.radius = 25;

            await Task.Delay(1000);

            DestroyLayoutComponents();
            isImageLayoutDestroyed = true;
            //MessagePhoto.rectTransform.localScale = new Vector3(0.8f, 0.8f, 1);
        }

        EditHolder.SetActive(ProfileManager.Instance.IsInEditMode);
    }

    private void AdjustPhotoScale(float w, float h)
    {
        DisableImageLayoutComponents();

        var ratio = w / h;

        if (ratio < ProfileManager.Instance.Settings.MinimumMessagePhotoScallingRatio)
        {
            w *= (ratio / ProfileManager.Instance.Settings.MessagePhotoWidthScallingRatioFactor);
            h *= (ratio / ProfileManager.Instance.Settings.MessagePhotoHeightScallingRatioFactor);
        }

        MessagePhoto.rectTransform.anchorMax = MessagePhoto.rectTransform.anchorMin;
        MessagePhoto.rectTransform.sizeDelta = new Vector2(w, h);

        var fillerRect = MessagePhoto.transform.parent.GetChild(1).GetComponent<RectTransform>();
        fillerRect.sizeDelta = new Vector2(fillerRect.sizeDelta.x, fillerRect.sizeDelta.y);
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
