using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Music;
using Tweetinvi.Core;
using Tweetinvi.Core.Interfaces;
using Youtube;
using SoundCloud;

namespace Twitter
{
    class Twitter
    {
        private DateTime last_update;

        public Twitter()
        {
            Tweetinvi.Auth.SetUserCredentials(APIKeyProvider.Twitter_ConsumerKey, APIKeyProvider.Twitter_ConsumerSecret, APIKeyProvider.Twitter_AccessToken, APIKeyProvider.Twitter_AccessTokenSecret);
            last_update = DateTime.Now.AddHours(-1);
        }

        public IEnumerable<IMention> getTweets()
        {
            var tweets = Timeline.GetMentionsTimeline();
            Console.WriteLine("getting tweets");
            foreach(IMention m in tweets)
            {
                Console.WriteLine(m.Text);
            }
            return tweets;
        }

        public List<Music.PlayListEntry> getPlaylistEntriesFromTweets(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            List<Music.PlayListEntry> res = new List<PlayListEntry>();
            List<IMention> tweets = (from t in this.getTweets().ToList() where t.CreatedAt >= this.last_update && t.Urls.Count() > 0 select t).ToList();
            Console.WriteLine("analizing tweets");
            foreach(IMention t in tweets)
            {
                string user = t.CreatedBy.Name;
                string url = t.Urls[0].ExpandedURL;
                ITrack track = null;
                if (Youtube.Youtube.isCompatible(url))
                    track = youtube_client.resolveTrack(url);
                else if (SoundCloud.SoundCloud.isCompatible(url))
                    track = soundcloud_client.resolveTrack(url);
                if(track != null)
                    res.Add(new PlayListEntry(track, user, false));
            }

            this.last_update = DateTime.Now;
            return res;
        }

    }
}
