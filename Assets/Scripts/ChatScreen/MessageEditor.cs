using Assets.Scripts;

using System;

using TMPro;

using UnityEngine;

public class MessageEditor : MonoBehaviour
{
    public TMP_InputField MessageTextField;
    public TMP_InputField MessageDateField;

    private Message message;

    public void Initialise(Message _message)
    {
        this.message = _message;

        MessageTextField.text = this.message.Text;
        MessageDateField.text = this.message.DeliveredAt.ToString();
    }

    public void Save()
    {
        this.message.Text = MessageTextField.text;
        this.message.SentAt = new UDateTime();
        this.message.SentAt.dateTime = DateTime.Parse(MessageDateField.text);
    }

    public void Discard()
    {
        MessageTextField.text = this.message.Text;
        MessageDateField.text = this.message.DeliveredAt.ToString();
    }
}
