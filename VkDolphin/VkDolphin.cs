using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace VkDolphin
{

    public class AythInfo
    {
        public ulong ApplicationId;
        public string Login;
        public string Password;
        public WebProxy webProxy;
    }

    public class VkDolphinLib
    {


        private VkApi api;

        public VkDolphinLib(AythInfo aythInfo)
        {
            api = new VkApi();
            Initializtion(aythInfo);
        }

        public Dialogs.Dialogs Dialog;
        public Foto.Fotos Foto;

        private void Initializtion(AythInfo aythInfo)
        {

            if (aythInfo.webProxy == null)
            {
                api.Authorize(new ApiAuthParams
                {
                    ApplicationId = aythInfo.ApplicationId,
                    Login = aythInfo.Login,
                    Password = aythInfo.Password,
                    Settings = Settings.All,
                });
            }
            else
            {
                NetworkCredential networkCredential = aythInfo.webProxy.Credentials as NetworkCredential;

                api.Authorize(new ApiAuthParams
                {
                    ApplicationId = aythInfo.ApplicationId,
                    Login = aythInfo.Login,
                    Password = aythInfo.Password,
                    Settings = Settings.All,

                    Host = aythInfo.webProxy.Address.Host,
                    Port = aythInfo.webProxy.Address.Port,
                    ProxyLogin = networkCredential.UserName,
                    ProxyPassword = networkCredential.Password
                });
            }


            Dialog = new Dialogs.Dialogs(api);
            Foto = new Foto.Fotos(api);



            var messages = api.Messages.GetHistory(new MessagesGetHistoryParams { UserId = 3173802, Count = 200 });
            Foto.GetFotosToFolder(messages.Messages.ToArray(),"Folder",VkDolphin.Foto.SaveFolderType.SortByNameAndOwner);
        }


    }


    

}
