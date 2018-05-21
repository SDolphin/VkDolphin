using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;


namespace VkDolphin.Dialogs
{

    
    public class Dialogs
    {
        private VkApi api;

        private uint _count;

        private const short __MAX_BUFFER_SIZE = 200;


        public Dialogs(VkApi api)
        {
            this.api = api;
        }

        public uint Count
        {
            get { return _count = api.Messages.GetDialogs(new MessagesDialogsGetParams()).TotalCount; }
        }

        public Message[] GetDialogsIds()
        {
            Message[] mas = new Message[Count];

            uint count = __MAX_BUFFER_SIZE > mas.Length ? (uint)mas.Length : (uint)__MAX_BUFFER_SIZE;

            for (uint k = 0, i = 0; i < mas.Length; i += count)
            {
                if ((mas.Length - i) < count)
                {
                    count = (uint)mas.Length - i;
                }

                MessagesGetObject dialogs = api.Messages.GetDialogs(new MessagesDialogsGetParams
                {
                    Count = count,
                    Offset = (int)i
                });

                foreach (Message dialog in dialogs.Messages)
                {
                    mas[k] = dialog;
                    k++;
                }
            }

            return mas;
        }

        

        public Dictionary<User,Message []> GetDialogsHistory()
        {
            Message[] mas = GetDialogsIds();

            Dictionary<User, Message []> dictionary = new Dictionary<User, Message []>();

            Dictionary<long, User> userInfos = GetUserInfo(mas);

            foreach (Message message in mas)
            {
                if (message.UsersCount == null)
                {
                    Message[] mass = DialogHistory((uint)message.UserId);

                    User user = new User();
                    userInfos.TryGetValue((long)message.UserId, out user);
                    dictionary.Add(user, mass);
                }
            }

            return dictionary;
        }

        private Dictionary<long,User> GetUserInfo(Message[] mas)
        {
            List<long> listOfIds = new List<long>();

            Dictionary<long, User> dict = new Dictionary<long, User>(); 

            foreach (Message message in mas)
            {
                listOfIds.Add((long)message.UserId);
            }

            foreach (User user in  api.Users.Get(listOfIds).ToList())
            {
                dict.Add(user.Id, user);
            }

            return dict;
        }

        public Message[] DialogHistory(uint id)
        {
            Message[] mas  =  new Message[api.Messages.GetHistory(new MessagesGetHistoryParams { UserId = id }).TotalCount];

            uint count = __MAX_BUFFER_SIZE > mas.Length ? (uint)mas.Length : (uint)__MAX_BUFFER_SIZE;

            for (uint k = 0, i = 0; i < mas.Length; i += count)
            {
                if ((mas.Length - i) < count)
                {
                    count = (uint)mas.Length - i;
                }

                MessagesGetObject dialogHistory = api.Messages.GetHistory(new MessagesGetHistoryParams
                {
                    UserId = id,
                    Count = count,
                    Offset = (int)i
                });

                foreach (Message dialog in dialogHistory.Messages)
                {
                    mas[k] = dialog;
                    k++;
                }
            }

            return mas;
        }



    }
}
