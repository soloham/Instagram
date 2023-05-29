using System.Collections;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField UsernameField;
    public TMP_InputField PasswordField;

    public Color LoginBgInactiveColor;
    public Color LoginBgActiveColor;
    public Image LoginBg;

    public Color LoginTextInactiveColor;
    public Color LoginTextActiveColor;
    public TextMeshProUGUI LoginText;
    public GameObject LoginLoader;

    private bool IsLoginEnabled => !string.IsNullOrWhiteSpace(UsernameField.text) && !string.IsNullOrWhiteSpace(PasswordField.text);

    private bool isLoggingIn;

    // Update is called once per frame
    void Update()
    {
        if (isLoggingIn)
        {
            return;
        }

        ToggleLoginBtn(IsLoginEnabled);
    }

    void ToggleLoginBtn(bool enabled)
    {
        LoginBg.color = enabled ? LoginBgActiveColor : LoginBgInactiveColor;
        LoginText.color = enabled ? LoginTextActiveColor : LoginTextInactiveColor;
    }

    public void OnLogin()
    {
        if (isLoggingIn)
        {
            return;
        }

        StartCoroutine(Login());
    }

    private IEnumerator Login()
    {
        if (!IsLoginEnabled)
        {
            LoginText.gameObject.SetActive(true);
            LoginLoader.gameObject.SetActive(false);
            yield break;
        }

        isLoggingIn = true;

        LoginBg.color = LoginBgInactiveColor;
        LoginText.color = LoginTextInactiveColor;

        LoginText.gameObject.SetActive(false);
        LoginLoader.gameObject.SetActive(true);

        yield return new WaitForSeconds(UnityEngine.Random.Range(0.9f, 1.9f));

        var username = UsernameField.text;

        var userProfile = ProfileManager.Instance.Profiles.Find(x => x.Handle == username);

        if (userProfile == null)
        {
            LoginText.gameObject.SetActive(true);
            LoginLoader.gameObject.SetActive(false);

            isLoggingIn = false;
            yield break;
        }

        ProfileManager.Instance.LoggedInProfileHandle = username;

        DMScreenHeaderManager.Instance.Initialise();
        DMSreenMessagesManager.Instance.Initialise();
        HomeScreenManager.Instance.InitialiseFeeds();

        isLoggingIn = false;

        UsernameField.text = "";
        PasswordField.text = "";

        LoginText.gameObject.SetActive(true);
        LoginLoader.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
