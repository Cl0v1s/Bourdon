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

        public bool load()
        {
            return this.track.load();
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
    }

    class Playlist
    {
        private PlayListEntry playing;
        private List<PlayListEntry> to_play;
        private List<PlayListEntry> played;
        private List<string> banned;

        public Playlist()
        {
            this.to_play = new List<PlayListEntry>();
            this.played = new List<PlayListEntry>();
        }

        public bool add(PlayListEntry entry)
        {
            IEnumerable<string> is_banned = from ban in this.banned where ban == entry.user select ban;
            if (is_banned.Count() > 0)
                return false;
            IEnumerable<PlayListEntry> occ = from ent in this.to_play where ent.track.getRemote() == entry.track.getRemote() select ent;
            if (occ.Count() > 0)
                return false;
            this.to_play.Add(entry);
            return true;
        }

        public void next()
        {
            if (this.to_play.Count() <= 0 && this.played.Count() > 0)
                this.to_play = this.played;
            else
                return;
            if(this.playing != null)
            {
                this.playing.stop();
                this.playing.dispose();
                this.played.Add(this.playing);
                this.playing = null;
            }
            this.playing = this.to_play[0];
            this.to_play.RemoveAt(0);
            this.playing.play();

            while (this.to_play[0].user == this.playing.user)
                this.to_play.RemoveAt(0);
            if(this.to_play.Count > 0)
                this.to_play[0].load();
        }

        public void banCurrent()
        {
            this.banned.Add(this.playing.user);
            this.next();
        }

        public void ban(string user)
        {
            this.banned.Add(user);
        }

        public void unban(string user)
        {
            this.banned.Remove(user);
        }



    }
}
