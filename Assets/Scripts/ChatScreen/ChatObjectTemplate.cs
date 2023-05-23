namespace Assets.Scripts.ChatScreen
{
    using UnityEngine;

    public abstract class ChatObjectTemplate
    {
        public ChatObjectTemplate(ChatObjectType type)
        {
            Type = type;
        }

        public ChatObjectType Type { get; set; }
    }

    public class ChatMessageObjectTemplate : ChatObjectTemplate
    {
        public ChatMessageObjectTemplate()
            : base(ChatObjectType.Message)
        {

        }

        public ChatMessage ChatMessage { get; set; }

        public bool IsContinuation { get; set; }
        public bool IsLocalLast { get; set; }
        public bool IsLast { get; set; }
        public bool IsAlone { get; set; }

        public bool EnableProfileImage { get; set; }
    }
    public class ChatTimeBreakObjectTemplate : ChatObjectTemplate
    {
        public ChatTimeBreakObjectTemplate()
            : base(ChatObjectType.TimeBreak)
        {

        }

        public string TimestampText { get; set; }
    }
    public class ChatDelayObjectTemplate : ChatObjectTemplate
    {
        public ChatDelayObjectTemplate()
            : base(ChatObjectType.Delay)
        {

        }
    }
    public class ChatStatusObjectTemplate : ChatObjectTemplate
    {
        public ChatStatusObjectTemplate()
            : base(ChatObjectType.Status)
        {

        }
    }

    public enum ChatObjectType
    {
        Message = 1,
        Delay = 2,
        TimeBreak = 3,
        Status = 4
    }
}
