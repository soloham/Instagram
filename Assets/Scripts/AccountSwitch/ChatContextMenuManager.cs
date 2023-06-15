using Cysharp.Threading.Tasks;

using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static UnityEngine.GraphicsBuffer;

public class ChatContextMenuManager : MonoBehaviour
{
    public static ChatContextMenuManager Instance;

    public GameObject MainHolder;
    public Image Backdrop;
    public RectTransform Container;

    public TextMeshProUGUI LoggedInUserTMP;

    public RectTransform DeleteConfirmation;

    public float FadeInRate = 1f;
    public float FadeOutRate = 10f;

    public float RollInRate = 10f;
    public float RollOutRate = 60f;

    public float ScaleInRate = 10f;
    public float ScaleOutRate = 60f;

    public Vector2 ScaleInStartSize = new Vector2(190.105f, 261.6037f);
    public Vector2 ScaleInEndSize = new Vector2(198.75f, 273.5f);

    private ChatUI chatUI;

    public void Show(ChatUI Chat)
    {
        chatUI = Chat;

        MainHolder.SetActive(true);

        LoggedInUserTMP.text = Chat.Chat.WithProfileHandle;

        StartCoroutine(FadeBackdrop());
        StartCoroutine(RollContainer());
    }

    private void ShowConfirmation()
    {
        DeleteConfirmation.gameObject.SetActive(true);

        StartCoroutine(FadeBackdrop());
        StartCoroutine(ScaleConfirmationContainer());
    }

    public void OnHide()
    {
        if (!MainHolder.activeSelf)
        {
            return;
        }

        StopAllCoroutines();
        HideContextMenuScreen();
    }

    public void Delete()
    {
        StopAllCoroutines();
        StartCoroutine(FadeBackdrop(false));
        StartCoroutine(RollContainer(false, true));
    }

    public void OnDeleteConfirmed()
    {
        DeleteConfirmed();
    }

    public async UniTask DeleteConfirmed()
    {
        HideContextMenuScreen();

        await UniTask.Delay(300);
        Destroy(chatUI.gameObject);

        ProfileManager.Instance.LoggedInProfile.Chats.Remove(chatUI.Chat);
    }

    private void HideContextMenuScreen()
    {
        StartCoroutine(FadeBackdrop(false));
        StartCoroutine(ScaleConfirmationContainer(false));
        StartCoroutine(RollContainer(false));
    }

    private IEnumerator FadeBackdrop(bool fadeIn = true)
    {
        Backdrop.color = fadeIn ? new Color(0, 0, 0, 0) : Backdrop.color;

        var rate = fadeIn ? FadeInRate : FadeOutRate;

        var targetAlpha = fadeIn ? 0.7f : 0f;
        Color curColor = Backdrop.color;
        while (Mathf.Abs(curColor.a - targetAlpha) > 0.1f)
        {
            curColor.a = Mathf.Lerp(curColor.a, targetAlpha, rate * Time.deltaTime);
            Backdrop.color = curColor;
            yield return null;
        }
    }

    private IEnumerator RollContainer(bool rollIn = true, bool showConfirmationAtEnd = false)
    {
        if (!rollIn && Container.anchoredPosition.y == -Container.sizeDelta.y)
        {
            yield return null;
        }
        else
        {
            Container.anchoredPosition = rollIn && Container.anchoredPosition.y >= 0
                ? new Vector2(Container.anchoredPosition.x, -Container.sizeDelta.y)
                : Container.anchoredPosition;

            var rate = rollIn ? RollInRate : RollOutRate;

            var targetY = rollIn ? 0 : -Container.sizeDelta.y;
            float curY = Container.anchoredPosition.y;
            while (Mathf.Abs(curY - targetY) > 0.1f)
            {
                Container.anchoredPosition = new Vector2(Container.anchoredPosition.x, Mathf.Lerp(curY, targetY, rate * Time.deltaTime));
                curY = Container.anchoredPosition.y;
                yield return null;
            }

            Container.anchoredPosition = new Vector2(Container.anchoredPosition.x, targetY);

            if (!rollIn && !showConfirmationAtEnd)
            {
                MainHolder.SetActive(false);
            }

            if (showConfirmationAtEnd)
            {
                yield return new WaitForSeconds(0.2f);

                ShowConfirmation();
            }
        }
    }

    private IEnumerator ScaleConfirmationContainer(bool scaleIn = true)
    {
        DeleteConfirmation.localScale = scaleIn && new Vector2(DeleteConfirmation.localScale.x, DeleteConfirmation.localScale.y) != ScaleInStartSize
            ? ScaleInStartSize
            : DeleteConfirmation.localScale;

        var rate = scaleIn ? ScaleInRate : ScaleOutRate;

        var targetX = scaleIn ? ScaleInEndSize.x : ScaleInStartSize.x;
        var targetY = scaleIn ? ScaleInEndSize.y : ScaleInStartSize.y;
        float curX = DeleteConfirmation.localScale.x;
        float curY = DeleteConfirmation.localScale.y;
        while (Mathf.Abs(curY - targetY) > 0.1f || Mathf.Abs(curX - targetX) > 0.1f)
        {
            DeleteConfirmation.localScale = new Vector2(Mathf.Lerp(curX, targetX, rate * Time.deltaTime), Mathf.Lerp(curY, targetY, rate * Time.deltaTime));
            curY = DeleteConfirmation.localScale.y;
            curX = DeleteConfirmation.localScale.x;
            yield return null;
        }

        DeleteConfirmation.localScale = new Vector2(targetX, targetY);

        if (!scaleIn)
        {
            DeleteConfirmation.gameObject.SetActive(false);
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
            OnHide();
            MainHolder.SetActive(false);
        }
    }
}
