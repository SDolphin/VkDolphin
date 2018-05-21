using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;

using System.IO;

using System.Reflection;
using System.Net;

namespace VkDolphin.Foto
{
    public enum Quality
    {
        Minimum,
        Maximum
    }

    public enum SaveFolderType
    {
        AllToOneFilder,
        SortByName,
        SortByNameAndOwner
    }

    public class Fotos
    {
        private VkApi api;

        private PhotoInfo photoInfo;
        private WebProxy _proxy;

        
        public Fotos(VkApi api, WebProxy proxy = null)
        {
            this.api = api;
            photoInfo = new PhotoInfo();
            _proxy = proxy;
        }

        public void GetFotosToFolder(Dictionary<User, Message[]> keyValuePairs, string folderPath, SaveFolderType saveFolderType,
         Quality quality = Quality.Maximum)
        {
            foreach (KeyValuePair<User, Message[]> keyValue in keyValuePairs)
            {

                if (saveFolderType == SaveFolderType.SortByNameAndOwner ||
                           saveFolderType == SaveFolderType.SortByName)
                {
                    User user = keyValue.Key;
                    folderPath += "\\" + user.FirstName + "_" + user.LastName;
                }

                GetFotosToFolder(keyValue.Value, folderPath, saveFolderType, quality);
            }
        }


        public int GetFotosToFolder( Message[] messages, string folderPath, SaveFolderType saveFolderType
            , Quality quality = Quality.Maximum)
        {
         
            int i = 0;
            foreach (Message mess in messages)
            {
                List<Photo> photos = GetPhoto(mess);

                if (photos.Count != 0)
                {

                    foreach (Photo photo in photos)
                    {
                        string finalPath = folderPath;
                        if (saveFolderType == SaveFolderType.SortByNameAndOwner)
                        {

                            if (mess.FromId == api.UserId)
                            {
                                finalPath += "\\My";
                            }
                            else
                            {
                                finalPath += "\\Buddy";
                            }
                        }


                        Uri uri = GetPhotoAdress(photo, quality);
                        if (SaveFoto(uri, finalPath))
                        {
                            i++;
                        }
                        
                    }

                }

            }

            return i;
        }


        public List<Photo> GetPhoto(Message message)
        {
            List<Photo> photos = new List<Photo>();

            if (message.Attachments.Count != 0)
            {
                foreach (Attachment attachment in message.Attachments)
                {
                    if (attachment.Type.Name == "Photo")
                    {
                        Photo photo = attachment.Instance as Photo;

                        photos.Add(photo);
                    }
                }
            }

            return photos;
        }

        private class PhotoInfo
        {
            private const string _photo2560 = "Photo2560";
            private const string _photo1280 = "Photo1280";
            private const string _photo807 = "Photo807";
            private const string _photo604 = "Photo604";
            private const string _photo130 = "Photo130";
            private const string _photo75 = "Photo75";

            public string Photo2560 { get => _photo2560; }
            public string Photo1280 { get => _photo1280; }
            public string Photo807 { get => _photo807; }
            public string Photo604 { get => _photo604; }
            public string Photo130 { get => _photo130; }
            public string Photo75 { get => _photo75; }

            private struct PhotoI
            {
                public int resolution;
                public string name;
            }

            private List<PhotoI> photoIs = new List<PhotoI>();
            public PhotoInfo()
            {
                photoIs.Add(new PhotoI() { resolution = 2560, name = _photo2560 });
                photoIs.Add(new PhotoI() { resolution = 1280, name = _photo1280 });
                photoIs.Add(new PhotoI() { resolution = 807, name = _photo807 });
                photoIs.Add(new PhotoI() { resolution = 604, name = _photo604 });
                photoIs.Add(new PhotoI() { resolution = 130, name = _photo130 });
                photoIs.Add(new PhotoI() { resolution = 75, name = _photo75 });
            }

            public List<string> Sort(Quality quality)
            {
                if (quality == Quality.Minimum)
                {
                    return photoIs.OrderBy(u => u.resolution).Select(u => u.name).ToList();
                }
                else
                {
                    return photoIs.OrderByDescending(u => u.resolution).Select(u => u.name).ToList();
                }
            }
        }

        public Uri GetPhotoAdress(Photo photo, Quality quality)
        {

            foreach (string pQuality in photoInfo.Sort(quality))
            {
                PropertyInfo m = photo.GetType().GetProperty(pQuality);// GetMethod(pQuality);
                try
                {
                    object obj = m.GetValue(photo);
                    if (obj == null)
                    {
                        continue;
                    }
                    else
                    { 
                        return obj as Uri;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
                
                
            }

            return null;
        }

        private void CheckAndCreateDir(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

        }


        public bool SaveFoto(Uri uri, string directory)
        {

            CheckAndCreateDir(directory);

            string finalPath = directory + "\\" + GetFileNameFromUri(uri);


            WebClient webClient = new WebClient();
            webClient.Proxy = _proxy;
            try
            {
                webClient.DownloadFile(uri, finalPath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private  string GetFileNameFromUri(Uri uri)
        {
            return Path.GetFileName(uri.LocalPath);
        }

    }

    


    
}
