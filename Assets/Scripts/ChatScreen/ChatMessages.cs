namespace Assets.Scripts.ChatScreen
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProfileRaw
    {
        public string Name;

        public string Handle;

        public string PictureUID;
    }

    public class ChatMessagesRaw
    {
        public List<string> Participants;
        public List<ChatMessageRaw> Messages { get; set; }

        public bool Blocked;
    }
    public class ChatMessages
    {
        public List<string> Handles;
        public List<ChatMessage> Messages { get; set; }

        public bool Blocked;
    }

    public class AllChatMessagesRaw
    {
        public List<ProfileRaw> Profiles { get; set; }

        public List<ChatMessagesRaw> Conversations { get; set; } = new List<ChatMessagesRaw>();
    }
}
