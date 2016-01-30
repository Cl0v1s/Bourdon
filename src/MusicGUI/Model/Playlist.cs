using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Music
{
    class PlaylistMinified
    {
        public List<PlayListEntryMinified> to_play{get;set;}
        public List<PlayListEntryMinified> played { get; set; }
        public List<string> banned { get; set; }

        public PlaylistMinified()
        {
            this.to_play = new List<PlayListEntryMinified>();
            this.played = new List<PlayListEntryMinified>();
            this.banned = new List<string>();
        }

        public Playlist expand(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            Playlist p = new Playlist();
            foreach(PlayListEntryMinified e in to_play)
            {
                p.to_play.Add(e.expand(youtube_client, soundcloud_client));
            }
            foreach (PlayListEntryMinified e in played)
            {
                p.played.Add(e.expand(youtube_client, soundcloud_client));
            }
            p.banned = this.banned;
            return p;
        }
    }


    class PlayListEntryMinified
    {
        public string user { get; set; }
        public string url { get; set; }

        public PlayListEntryMinified(string user, string url)
        {
            this.user = user;
            this.url = url;
        }

        public PlayListEntry expand(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            PlayListEntry p = null;
            if (this.url.StartsWith("https://www.youtube.com") || this.url.StartsWith("https://m.youtube.com"))
                p = new PlayListEntry(youtube_client.resolveTrack(this.url), this.user, false);
            else if (this.url.StartsWith("https://soundcloud.com/") || this.url.StartsWith("https://m.soundcloud.com/"))
                p = new PlayListEntry(soundcloud_client.resolveTrack(this.url), this.user, false);
            return p;
        }
    }

    public class PlayListEntry
    {
        public ITrack track { get; set; }
        public string user { get; set; }
        public bool loaded { get; set; }

        public PlayListEntry(ITrack track, string user, bool loaded)
        {
            this.track = track;
            this.user = user;
            this.loaded = loaded;
        }

        public bool load()
        {
            bool r = this.track.load();
            this.loaded = r;
            return r;
        }

        public void play()
        {
            this.track.play();
        }

        public void stop()
        {
            this.track.stop();
        }

        public void dispose()
        {
            this.track.dispose();
        }

        public string getRemote()
        {
            return this.track.getRemote();
        }

        public string getTitle()
        {
            return this.track.getTitle();
        }
    }

    public class Playlist
    {
        public PlayListEntry playing{get;set;}
        public List<PlayListEntry> to_play{get;set;}
        public List<PlayListEntry> played;
        public List<string> banned { get; set; }

        public Playlist()
        {
            this.to_play = new List<PlayListEntry>();
            this.played = new List<PlayListEntry>();
            this.banned = new List<string>();
        }

        public bool add(PlayListEntry entry)
        {
            Console.WriteLine("Adding " + entry.getRemote());
            IEnumerable<string> is_banned = from ban in this.banned where ban == entry.user select ban;
            if (is_banned.Count() > 0)
                return false;
            IEnumerable<PlayListEntry> occ = from ent in this.to_play where ent.track.getRemote() == entry.track.getRemote() select ent;
            if (occ.Count() > 0)
                return false;
            this.to_play.Add(entry);
            Console.WriteLine("Added " + entry.getRemote());
            return true;
        }
        

        public void remove(PlayListEntry entry)
        {
            this.to_play.Remove(entry);
            this.played.Remove(entry);
        }

        public void next()
        {
            Console.WriteLine("Next of "+this.to_play.Count());
            if (this.playing != null)
            {
                this.playing.stop();
                this.playing.dispose();
            }
            if (this.to_play.Count() <= 0 && this.played.Count() > 0)
                this.to_play = this.played;
            else if (this.to_play.Count() <= 0)
            {
                this.playing = null;
                return;
            }
            if(this.playing != null)
                this.played.Add(this.playing);
            this.playing = this.to_play[0];
            this.to_play.RemoveAt(0);
            this.playing.play();
            if (this.to_play.Count() <= 0)
                return;
            while (this.to_play.Count() >0 && this.to_play[0].user == this.playing.user)
                this.to_play.RemoveAt(0);
        }

        public void banCurrent()
        {
            if (this.playing == null)
                return;
            Console.WriteLine("Banned " + this.playing.user);
            if ((from entry in this.banned where entry == this.playing.user select entry).Count() <= 0)
            {
                this.banned.Add(this.playing.user);
                this.next();
            }
        }

        public void ban(string user)
        {
            IEnumerable<string> users = from entry in this.to_play where entry.user == user select entry.user;
            users.Concat(from entry in this.played where entry.user == user select entry.user);
            if ((users.Count() > 0 || this.playing.user == user) && (from entry in this.banned where entry == this.playing.user select entry).Count() <= 0)
            {
                this.banned.Add(user);
                Console.WriteLine("Banned " + user);
            }
            else
                Console.WriteLine("Unable to ban " + user);
        }

        public void unban(string user)
        {
            this.banned.Remove(user);
            Console.WriteLine("UnBanned " + user);
        }

        public void update()
        {
            if (this.playing != null && this.playing.track.isTerminated())
            {
                this.playing.track.reset();
                this.next();
            }
        }

        public void save()
        {
            //Déclaration de l'objet à sauvegarder
            this.to_play.Insert(0,this.playing);
            this.playing.stop();
            this.playing.dispose();
            this.playing = null;
            PlaylistMinified p = new PlaylistMinified();
            foreach(PlayListEntry e in this.to_play)
            {
                p.to_play.Add(new PlayListEntryMinified(e.user, e.track.getUrl()));
            }
            foreach (PlayListEntry e in this.played)
            {
                p.played.Add(new PlayListEntryMinified(e.user, e.track.getUrl()));
            }
            p.banned = this.banned;
            string data = JsonConvert.SerializeObject(p);
            if (File.Exists("save.json"))
                File.Delete("save.json");
            StreamWriter stream = new StreamWriter(File.OpenWrite("save.json"));
            stream.Write(data);
            stream.Close();
        }

        public void load(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            if (File.Exists("save.json") == false)
                return;
            string data = new StreamReader(File.OpenRead("save.json")).ReadToEnd();
            Playlist p = (JsonConvert.DeserializeObject<PlaylistMinified>(data)).expand(youtube_client, soundcloud_client);
            this.to_play = p.to_play;
            this.played = p.played;
            this.playing = p.playing;
            this.banned = p.banned;
        }



    }
}
