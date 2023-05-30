using UnityEngine;

public class ChatEditorManager : MonoBehaviour
{
    [SerializeField] private ChatAreaManager chatAreaManager;
    [SerializeField] private MessageEditor messageEditor;

    private void Start()
    {
        MessageUI.OnEdit += Edit;
        MessageUI.OnDelete += Delete;
        MessageUI.OnUndelete += Undelete;

        ProfileManager.OnEditModeChanged += Instance_OnEditModeChanged;
    }

    private void Instance_OnEditModeChanged(bool isEditingAllowed)
    {
        if (messageEditor.gameObject.activeSelf)
        {
            messageEditor.gameObject.SetActive(isEditingAllowed);
        }
    }

    private void OnDestroy()
    {
        MessageUI.OnEdit -= Edit;
        MessageUI.OnDelete -= Delete;
        MessageUI.OnUndelete -= Undelete;

        ProfileManager.OnEditModeChanged -= Instance_OnEditModeChanged;
    }

    public void Edit(MessageUI messageUI)
    {
        messageEditor.gameObject.SetActive(true);
        messageEditor.Initialise(messageUI);
    }

    public void Delete(MessageUI messageUI)
    {
        var message = messageUI.ChatMessage.Message;

        var currentChat = chatAreaManager.ChatScreenManager.Chat;
        var chat = messageUI.ChatMessage.From.Handle != currentChat.FromProfileHandle ? currentChat.TheirChat : currentChat;

        chat.Messages.Remove(message);

        messageUI.MarkAsDeleted();
    }

    public void Undelete(MessageUI messageUI)
    {
        var message = messageUI.ChatMessage.Message;

        var currentChat = chatAreaManager.ChatScreenManager.Chat;
        var chat = messageUI.ChatMessage.From.Handle != currentChat.FromProfileHandle ? currentChat.TheirChat : currentChat;

        chat.Messages.Add(message);

        messageUI.UnmarkAsDeleted();
    }
}
