using System;
using System.Globalization;

using TMPro;

using UnityEngine;

public class MessageEditor : MonoBehaviour
{
    public TMP_InputField MessageTextField;
    public TMP_InputField MessageDateField;
    public TextMeshProUGUI StatusTMP;

    private MessageUI messageUI;

    public void Initialise(MessageUI _messageUI)
    {
        this.messageUI = _messageUI;

        MessageTextField.text = this.messageUI.ChatMessage.Message.Text;
        MessageDateField.text = this.messageUI.ChatMessage.Message.DeliveredAt.dateTime.ToString(CultureInfo.InvariantCulture);
    }

    public void Save()
    {
        var isValidDate = DateTime.TryParse(MessageDateField.text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

        if (!isValidDate)
        {
            StatusTMP.text = "Date is invalid";
            return;
        }
        else
        {
            StatusTMP.text = "";
        }

        this.messageUI.ChatMessage.Message.Text = MessageTextField.text;
        this.messageUI.ChatMessage.Message.SentAt.dateTime = dateTime;

        this.messageUI.MessageText.text = this.messageUI.ChatMessage.Message.Text;

        gameObject.SetActive(false);
    }

    public void Discard()
    {
        MessageTextField.text = "";
        MessageDateField.text = "";
        StatusTMP.text = "";

        gameObject.SetActive(false);
    }
}
