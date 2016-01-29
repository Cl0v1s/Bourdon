using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    class PlayListEntry
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

    class Playlist
    {
        public PlayListEntry playing{get;set;}
        private List<PlayListEntry> to_play;
        private List<PlayListEntry> played;
        private List<string> banned;

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
            while (this.to_play[0].user == this.playing.user)
                this.to_play.RemoveAt(0);
            if(this.to_play.Count() > 0)
                this.to_play[0].load();
        }

        public void banCurrent()
        {
            if (this.playing == null)
                return;
            Console.WriteLine("Banned " + this.playing.user);
            this.banned.Add(this.playing.user);
            this.next();
        }

        public void ban(string user)
        {
            IEnumerable<string> users = from entry in this.to_play where entry.user == user select entry.user;
            users.Concat(from entry in this.played where entry.user == user select entry.user);
            if (users.Count() > 0 || this.playing.user == user)
            {
                this.banned.Add(user);
                Console.WriteLine("Banned " + user);
            }
        }

        public void unban(string user)
        {
            this.banned.Remove(user);
            Console.WriteLine("UnBanned " + user);
        }

        public void update()
        {
            //TODO.
        }



    }
}
