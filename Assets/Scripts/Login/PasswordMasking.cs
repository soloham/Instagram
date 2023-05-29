using UnityEngine;
using TMPro;

public class PasswordMasking : MonoBehaviour
{
    private TMP_InputField inputField;

    private void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.contentType = TMP_InputField.ContentType.Password;
        inputField.inputType = TMP_InputField.InputType.Password;
        inputField.asteriskChar = '•';
    }
}
