using Assets.Scripts.Extensions;
using Assets.Scripts.Helpers;

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

    public GameObject FeedPrefab;

    public static HomeScreenManager Instance;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    public void Initialise()
    {
        ProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.Picture;
        AddStoryProfileImage.sprite = ProfileManager.Instance.LoggedInProfile.Picture;

        foreach (Transform child in FeedsScrollAreaTransform)
        {
            if (child.tag != "Feed")
            {
                continue;
            }

            Destroy(child.gameObject);
        }

        var settings = ProfileManager.Instance.Settings;
        var feeds = settings.Feeds;

        if (settings.RandomiseFeed)
        {
            feeds.Shuffle();
        }

        foreach (var feed in feeds)
        {
            var feedObj = Instantiate(FeedPrefab, FeedsScrollAreaTransform);
            var feedRect = feedObj.GetComponent<RectTransform>();
            var feedImage = feedObj.GetComponent<Image>();

            feedRect.sizeDelta = new Vector2(feed.Width, feed.Height);
            feedImage.sprite = MessagePhotoManager.LoadSprite(feed.FeedUID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HeaderDivider.gameObject.SetActive(FeedsScrollAreaTransform.anchoredPosition.y > 0);
    }
}
