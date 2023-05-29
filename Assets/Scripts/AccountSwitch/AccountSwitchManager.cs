using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class AccountSwitchManager : MonoBehaviour
{
    public static AccountSwitchManager Instance;

    public GameObject MainHolder;
    public Image Backdrop;
    public RectTransform SwitchContainer;

    public TextMeshProUGUI LoggedInUserTMP;
    public Image LoggedInUserPicture;

    public GameObject LoginScreen;

    public float FadeInRate = 1f;
    public float FadeOutRate = 10f;

    public float RollInRate = 10f;
    public float RollOutRate = 60f;

    public void ShowSwitchScreen()
    {
        MainHolder.SetActive(true);

        LoggedInUserTMP.text = ProfileManager.Instance.LoggedInProfileHandle;
        LoggedInUserPicture.sprite = ProfileManager.Instance.LoggedInProfile.Picture;

        StartCoroutine(FadeBackdrop());
        StartCoroutine(RollContainer());
    }
    public void OnHideSwitchScreen()
    {
        if (!MainHolder.activeSelf)
        {
            return;
        }

        StopAllCoroutines();
        HideSwitchScreen();
    }

    public void ShowLoginScreen()
    {
        LoginScreen.SetActive(true);

        StopAllCoroutines();
        HideSwitchScreen();
    }

    private void HideSwitchScreen()
    {
        StartCoroutine(FadeBackdrop(false));
        StartCoroutine(RollContainer(false));
    }

    private IEnumerator FadeBackdrop(bool fadeIn = true)
    {
        Backdrop.color = fadeIn ? new Color(0, 0, 0, 0) : Backdrop.color;

        var rate = fadeIn ? FadeInRate : FadeOutRate;

        var targetAlpha = fadeIn ? 0.5f : 0f;
        Color curColor = Backdrop.color;
        while (Mathf.Abs(curColor.a - targetAlpha) > 0.1f)
        {
            curColor.a = Mathf.Lerp(curColor.a, targetAlpha, rate * Time.deltaTime);
            Backdrop.color = curColor;
            yield return null;
        }
    }

    private IEnumerator RollContainer(bool rollIn = true)
    {
        SwitchContainer.anchoredPosition = rollIn && SwitchContainer.anchoredPosition.y >= 0
            ? new Vector2(SwitchContainer.anchoredPosition.x, -SwitchContainer.sizeDelta.y)
            : SwitchContainer.anchoredPosition;

        var rate = rollIn ? RollInRate : RollOutRate;

        var targetY = rollIn ? 0 : -SwitchContainer.sizeDelta.y;
        float curY = SwitchContainer.anchoredPosition.y;
        while (Mathf.Abs(curY - targetY) > 0.1f)
        {
            SwitchContainer.anchoredPosition = new Vector2(SwitchContainer.anchoredPosition.x, Mathf.Lerp(curY, targetY, rate * Time.deltaTime));
            curY = SwitchContainer.anchoredPosition.y;
            yield return null;
        }

        if (!rollIn)
        {
            MainHolder.SetActive(false);
        }
    }

    private void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnHideSwitchScreen();
            LoginScreen.SetActive(false);
        }
    }
}
