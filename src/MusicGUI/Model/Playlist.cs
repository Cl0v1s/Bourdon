using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace Music
{
    /// <summary>
    /// Classe permettant de stocker un Json les informations sur la playlist
    /// </summary>
    class PlaylistMinified
    {
        public List<PlayListEntryMinified> to_play { get; set; }
        public List<PlayListEntryMinified> played { get; set; }
        public List<string> banned { get; set; }

        public PlaylistMinified()
        {
            this.to_play = new List<PlayListEntryMinified>();
            this.played = new List<PlayListEntryMinified>();
            this.banned = new List<string>();
        }

        /// <summary>
        /// Transforme une mini playlist en réelle playlist
        /// </summary>
        /// <param name="youtube_client"></param>
        /// <param name="soundcloud_client"></param>
        /// <returns></returns>
        public Playlist expand(Youtube.Youtube youtube_client, SoundCloud.SoundCloud soundcloud_client)
        {
            Playlist p = new Playlist();
            foreach (PlayListEntryMinified e in to_play)
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

    /// <summary>
    /// Représente une entrée de playlist minifiée pour la sauvegarde en JSON
    /// </summary>
    class PlayListEntryMinified
    {
        public string user { get; set; }
        public string url { get; set; }

        public PlayListEntryMinified(string user, string url)
        {
            this.user = user;
            this.url = url;
        }

        /// <summary>
        /// Convertit l'entrée de playlist minifiée en réelle entrée de playlist
        /// </summary>
        /// <param name="youtube_client"></param>
        /// <param name="soundcloud_client"></param>
        /// <returns></returns>
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

    /// <summary>
    /// Reprénte un entrée de playlist
    /// </summary>
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
            if (this.track == null)
                return "Inconnu";
            return this.track.getRemote();
        }

        public string getTitle()
        {
            return this.track.getTitle();
        }
    }

    public class Playlist
    {
        public PlayListEntry playing { get; set; }
        public List<PlayListEntry> to_play { get; set; }
        public List<PlayListEntry> played;
        public List<string> banned { get; set; }
        public DateTime lastSwitch;

        public Playlist()
        {
            this.to_play = new List<PlayListEntry>();
            this.played = new List<PlayListEntry>();
            this.banned = new List<string>();
            this.lastSwitch = DateTime.Now;
        }

        /// <summary>
        /// Ajoute le morceau à la playlist
        /// </summary>
        /// <param name="entry">L'entrée de playlist à ajouter</param>
        /// <returns>Vrai si ajouté, Faux sinon</returns>
        public bool add(PlayListEntry entry)
        {
            Console.WriteLine("Adding " + entry.getRemote());
            if (entry == null)
                return false;
            if (entry.track == null)
                return false;
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

        /// <summary>
        /// Ajoute une liste d'entrées de playlist à la playlist
        /// </summary>
        /// <param name="entries">Liste d'entrées de playlistes à ajouter</param>
        public void add(List<PlayListEntry> entries)
        {
            foreach (PlayListEntry e in entries)
                this.add(e);
        }

        /// <summary>
        /// Supprime une entrée de playliste
        /// </summary>
        /// <param name="entry">Entrée à supprimer</param>
        public void remove(PlayListEntry entry)
        {
            this.to_play.Remove(entry);
            this.played.Remove(entry);
        }

        /// <summary>
        /// Passe à l'entrée suivante
        /// </summary>
        public void next()
        {
            this.lastSwitch = DateTime.Now;
            //Déchargement de la chanson actuelle
            if (this.playing != null)
            {
                this.playing.stop();
                this.playing.dispose();
                this.played.Add(this.playing);
                this.playing = null;
            }

            //Si la liste à jouer est vide, on charge ce qui a deja été joué 
            if (this.to_play.Count() <= 0)
            {
                this.to_play = new List<PlayListEntry>(this.played);
                this.played.Clear();
            }

            //SI la liste n'est pas vide
            if (this.to_play.Count() > 0)
            {
                this.playing = this.to_play[0];
                this.playing.load();
                this.playing.play();
                this.to_play.RemoveAt(0);
            }
        }

        /// <summary>
        /// Banni l'utilisateur ayant proposé le morceau courant
        /// </summary>
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

        /// <summary>
        /// Banni l'utilisateur dont le nom est passé en paramètre
        /// </summary>
        /// <param name="user">Utitlisateur à bannir</param>
        public void ban(string user)
        {
            IEnumerable<string> users = from entry in this.to_play where entry.user == user select entry.user;
            users.Concat(from entry in this.played where entry.user == user select entry.user);
            if ((users.Count() > 0 || (this.playing != null && this.playing.user == user)) && this.playing != null && (from entry in this.banned where entry == this.playing.user select entry).Count() <= 0)
            {
                this.banned.Add(user);
                Console.WriteLine("Banned " + user);
            }
            else
                Console.WriteLine("Unable to ban " + user);
        }

        /// <summary>
        /// Déban l'utilisateur 
        /// </summary>
        /// <param name="user">Utilisateur à débannir</param>
        public void unban(string user)
        {
            this.banned.Remove(user);
            Console.WriteLine("UnBanned " + user);
        }

        /// <summary>
        /// Met à jour la playliste, en vérifiant si le morceau courant est terminé
        /// </summary>
        public void update()
        {
            if (this.playing != null && (this.playing.track.isTerminated() || this.lastSwitch.AddMinutes(5) > DateTime.Now ))
            {
                this.playing.track.reset();
                this.next();
            }
        }

        /// <summary>
        /// Sauvegarde la playlist au format JSON
        /// </summary>
        public void save()
        {
            //Déclaration de l'objet à sauvegarder
            if (this.playing != null)
            {
                this.to_play.Insert(0, this.playing);
                this.playing.stop();
                this.playing.dispose();
            }
            this.playing = null;
            PlaylistMinified p = new PlaylistMinified();
            foreach (PlayListEntry e in this.to_play)
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

        /// <summary>
        /// Charge la playlist depuis un format JSON
        /// </summary>
        /// <param name="youtube_client">Client youtube</param>
        /// <param name="soundcloud_client">Client Soundcloud</param>
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
