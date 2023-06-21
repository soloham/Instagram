using AdvancedInputFieldPlugin;

using Cysharp.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

public class MessageField : MonoBehaviour
{
    public Image LeftImage;
    public Sprite CameraIcon;
    public Sprite SearchIcon;

    public RectTransform ActionsTransform;
    public GameObject SendObj;

    private AdvancedInputField MessageInput;

    public delegate void MessageSent(string message);
    public static event MessageSent OnMessageSent;

    // Start is called before the first frame update
    void Start()
    {
        MessageInput = GetComponent<AdvancedInputField>();
        MessageInput.OnValueChanged.AddListener((value) =>
        {
            var hasValue = !string.IsNullOrWhiteSpace(value);

            LeftImage.sprite = hasValue ? SearchIcon : CameraIcon;

            foreach (Transform action in ActionsTransform)
            {
                action.gameObject.SetActive(!hasValue);
            }
            SendObj.SetActive(hasValue);
        });
    }

    public void SendMessage()
    {
        var message = MessageInput.Text;
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (OnMessageSent != null)
        {
            OnMessageSent.Invoke(message);
        }

        MessageInput.SetText(string.Empty);
    }
}
