using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis;
using System.Text.RegularExpressions;


namespace Google
{

    /// <summary>
    /// Entité représentant un email google simplifié
    /// </summary>
    class Gmail
    {
        /// <summary>
        /// Sender
        /// </summary>
        public string user;

        /// <summary>
        /// Lien contenu dans le mail
        /// </summary>
        public string content;

        public Gmail(string user, string content)
        {
            this.user = user;
            this.content = content;
        }
    }


    class Google
    {

        /// <summary>
        /// Client de l'apI google permettant de se connecter à la boite Gmail
        /// </summary>
        private GmailService client;

        public Google()
        {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = Music.APIKeyProvider.Google_ClientId,
                        ClientSecret = Music.APIKeyProvider.Google_Secret
                    },
                    new string[] { GmailService.Scope.MailGoogleCom },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("./", true)).Result;
            this.client = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "bourdon",
            });

        }

        /// <summary>
        /// Récupère les mails depuis la boite Gmail et les ordonne sous forme de liste
        /// </summary>
        /// <returns></returns>
        public List<Gmail> getMessages()
        {
            List<Gmail> res = new List<Gmail>();
            IList<Message> request = this.client.Users.Messages.List("me").Execute().Messages;
            if (request == null)
            {
                Console.WriteLine("No Mail found");
                return res;
            }
            List<Message> response = request.ToList();
            Console.WriteLine("Listing messages");
            foreach (Message mes in response)
            {
                UsersResource.MessagesResource.GetRequest requ = this.client.Users.Messages.Get("me", mes.Id);
                requ.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
                Message r = requ.Execute();
                string correct = Encoding.UTF8.GetString(Convert.FromBase64String(r.Raw.Replace('-', '+').Replace('_', '/')));
                string user = Regex.Split(correct, "Return-Path: <")[1].Split('>')[0];
                string content = Regex.Split(Regex.Split(correct, "Content-Type: text/plain;.*\n")[1], "--")[0].Trim();
                if (content[content.Length - 1] == '=')
                    content = content.Remove(content.Length - 1, 1);
                content = content.Replace("v=3D", "v=");
                Console.WriteLine(content + " de " + user);
                res.Add(new Gmail(user, content));
                //Supression du message
                this.client.Users.Messages.Delete("me", mes.Id).Execute();
            }
            return res;
        }

        /// <summary>
        /// Transforme les messages présents dans la boite mail en entrées de playlist pour la lecture
        /// </summary>
        /// <param name="youtube_client">Instance du client youtube permettant d'interragir avec le service de vidéo en ligne</param>
        /// <param name="soundcloud_client">Instance du client soundcloud permettant d'interragir avec le service de son en ligne</param>
        /// <returns></returns>
        public List<Music.PlayListEntry> getPlaylistEntriesFromMail(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            List<Music.PlayListEntry> res = new List<Music.PlayListEntry>();
            List<Gmail> msg = this.getMessages();
            foreach (Gmail g in msg)
            {
                Music.ITrack track = null;
                if (Youtube.Youtube.isCompatible(g.content))
                    track = youtube_client.resolveTrack(g.content);
                else if (SoundCloud.SoundCloud.isCompatible(g.content))
                    track = soundcloud_client.resolveTrack(g.content);
                if (track != null)
                {
                    res.Add(new Music.PlayListEntry(track, g.user, false));
                }
            }
            return res;
        }
    }
}
