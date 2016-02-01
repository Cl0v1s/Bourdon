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

    class Gmail
    {
        public string user;
        public string content;

        public Gmail(string user, string content)
        {
            this.user = user;
            this.content = content;
        }
    }


    class Google
    {

        private GmailService client;
        private List<Gmail> checke;

        public Google()
        {
            this.checke = new List<Gmail>();
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = Music.APIKeyProvider.Google_ClientId,
                        ClientSecret = Music.APIKeyProvider.Google_Secret
                    },
                    new string[] { GmailService.Scope.MailGoogleCom},
                    "user",
                    CancellationToken.None,
                    new FileDataStore("./", true)).Result;
            this.client = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "bourdon",
            });

        }

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
            foreach(Message mes in response)
            {
                UsersResource.MessagesResource.GetRequest requ = this.client.Users.Messages.Get("me", mes.Id);
                requ.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
                Message r = requ.Execute();
                string correct = Encoding.UTF8.GetString(Convert.FromBase64String(r.Raw.Replace('-', '+').Replace('_', '/')));
                string user = Regex.Split(correct,"Return-Path: <")[1].Split('>')[0];
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

        public List<Music.PlayListEntry> getPlaylistEntriesFromMail(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            List<Music.PlayListEntry> res = new List<Music.PlayListEntry>();
            List<Gmail> msg = this.getMessages();
            foreach(Gmail g in msg)
            {
                bool f = false;
                foreach(Gmail gi in checke)
                {
                    if(g.user == gi.user && g.content == gi.content)
                    {
                        f = true;
                        break;
                    }
                }
                if (f == true)
                    continue;
                else
                    this.checke.Add(g);
                Music.ITrack track = null;
                if (Youtube.Youtube.isCompatible(g.content))
                    track = youtube_client.resolveTrack(g.content);
                else if (SoundCloud.SoundCloud.isCompatible(g.content))
                    track = soundcloud_client.resolveTrack(g.content);
                if(track != null)
                {
                    res.Add(new Music.PlayListEntry(track, g.user, false));
                }

            }
            return res;
        }
    }
}
