using System.Collections.Generic;
using Butterfly.Messages;

namespace Butterfly.HabboHotel.ChatMessageStorage
{
    public class ChatMessageManager 
    {
        private List<ChatMessage> listOfMessages;

        public ChatMessageManager()
        {
            listOfMessages = new List<ChatMessage>();
        }

        internal void AddMessage(ChatMessage message)
        {
            listOfMessages.Insert(0, message);
            if (listOfMessages == null)
                listOfMessages = new List<ChatMessage>();
            if (listOfMessages.Count >= 100)
                listOfMessages.RemoveAt(listOfMessages.Count - 1);
        }

        internal Dictionary<uint, List<ChatMessage>> GetSortedMessages()
        {
            uint actualRoom = 0;
            ChatMessage chatMessageInfo = null;
            Dictionary<uint, List<ChatMessage>> dictionaryInfo = new Dictionary<uint, List<ChatMessage>>();

            for (int i = 0; i < listOfMessages.Count; i++ )
            {
                chatMessageInfo = listOfMessages[i];
                if(chatMessageInfo.roomID != actualRoom)
                {
                    actualRoom = chatMessageInfo.roomID;

                    List<ChatMessage> cm = new List<ChatMessage>();
                    if (!dictionaryInfo.ContainsKey(chatMessageInfo.roomID))
                        dictionaryInfo.Add(actualRoom, cm);
                }

                dictionaryInfo[actualRoom].Add(chatMessageInfo);
            }

            return dictionaryInfo;
        }

        internal int messageCount
        {
            get
            {
                return listOfMessages.Count;
            }
        }

        internal List<ChatMessage> GetRoomChatMessage()
        {
            List<ChatMessage> proList = new List<ChatMessage>(listOfMessages);

            List<ChatMessage> newList = new List<ChatMessage>();
            for (int i = (proList.Count - 1); i >= 0; i--)
            {
                newList.Add(proList[i]);
            }

            proList.Clear();
            proList = null;

            return newList;
        }

        internal void Serialize(ref ServerMessage message)
        {
            foreach (var chatMessage in listOfMessages)
            {
                chatMessage.Serialize(ref message);
            }
        }

        internal void Destroy()
        {
            listOfMessages.Clear();
        }
    }
}
