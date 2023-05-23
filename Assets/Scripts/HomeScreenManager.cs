using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenManager : MonoBehaviour
{
    public Image HeaderDivider;
    public Image ProfileImage;
    public Image AddStoryProfileImage;
    public RectTransform FeedsScrollAreaTransform;

    // Start is called before the first frame update
    void Start()
    {
        ProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.Picture;
        AddStoryProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.Picture;
    }

    // Update is called once per frame
    void Update()
    {
        HeaderDivider.gameObject.SetActive(FeedsScrollAreaTransform.anchoredPosition.y > 0);
    }
}
