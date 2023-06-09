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

    private TMP_InputField MessageInput;

    public delegate UniTask MessageSent(string message);
    public static event MessageSent OnMessageSent;

    // Start is called before the first frame update
    void Start()
    {
        MessageInput = GetComponent<TMP_InputField>();
        MessageInput.onValueChanged.AddListener((value) =>
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
        MessageInput.ActivateInputField();

        var message = MessageInput.text;
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (OnMessageSent != null)
        {
            OnMessageSent.Invoke(message);
        }

        MessageInput.text = "";
    }
}
