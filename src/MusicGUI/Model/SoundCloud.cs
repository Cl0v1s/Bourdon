using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using NAudio;
using NAudio.Wave;
using Music;

namespace SoundCloud
{

    /// <summary>
    /// Instance de track spécial soundcloud
    /// </summary>
    public class Track: ITrack
    {
        IWavePlayer outer = null;
        private WaveStream data = null;
        public string base_url { get; set; }
        private bool terminated = false;

        //Ressources JSON
        public string client_id { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public bool streamable { get; set; }
        public string stream_url { get; set; }

        /// <summary>
        /// Charge le flux du morceau en mémoire
        /// </summary>
        /// <param name="play">si play est vrai, joue le morceau à la fin du chargement</param>
        /// <returns>faux si échec du chargement</returns>
        public bool load(bool play = false)
        {
            if (stream_url.Length > 0 && streamable == true)
            {
                terminated = false;
                var response = WebRequest.Create(stream_url + "?client_id=" + client_id).GetResponse();
                MemoryStream ms = new MemoryStream();
                Stream stream = response.GetResponseStream();
                byte[] buffer = new byte[65536]; // 64KB chunks
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    var pos = ms.Position;
                    ms.Position = ms.Length;
                    ms.Write(buffer, 0, read);
                    ms.Position = pos;
                }
                this.data = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)));
                if (play)
                    this.play();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Joue le morceau
        /// </summary>
        public void play()
        {
            if (this.data != null)
            {
                terminated = false;
                outer = new WaveOut();
                outer.Init(this.data);
                outer.Play();
                outer.PlaybackStopped += outer_PlaybackStopped;
            }
            else
                this.load(true);
        }

        /// <summary>
        /// evenement triggered à la fin de la lecture du morceau courant
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void outer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            terminated = true;
            this.stop();
        }

        /// <summary>
        /// Arrete de jouer le morceau courant
        /// </summary>
        public void stop()
        {
            if (outer != null)
            {
                terminated = true;
                outer.Stop();
                if(outer != null)
                    outer.Dispose();
                outer = null;
            }
        }

        /// <summary>
        /// Décharge la mémoire du morceau courant
        /// </summary>
        public void dispose()
        {
            if (outer != null)
                throw new Exception("Le Track doit etre stoppé avant de dispose");
            if (this.data != null)
            {
                this.data.Dispose();
                this.data = null;
            }
        }

        /// <summary>
        /// Retourne l'url de stream
        /// </summary>
        /// <returns></returns>
        public string getRemote()
        {
            return this.stream_url;
        }

        /// <summary>
        /// Retourne le titre du morceau
        /// </summary>
        /// <returns></returns>
        public string getTitle()
        {
            return this.title;
        }

        /// <summary>
        /// Retourne l'url de base de ce morceau (telle qu'elle a été proposée)
        /// </summary>
        /// <returns></returns>
        public string getUrl()
        {
            return base_url;
        }

        /// <summary>
        /// Retourne si le morceau est terminé ou non
        /// </summary>
        /// <returns></returns>
        public bool isTerminated()
        {
            return this.terminated;
        }

        /// <summary>
        /// Remet les stats du morceau à leur état original
        /// </summary>
        public void reset()
        {
            this.terminated = false;
        }
    }


    public class SoundCloud
    {

        /// <summary>
        /// Clef public permettant de se connecter à l'API
        /// </summary>
        private string _public_key;

        public SoundCloud(string public_key)
        {
            this._public_key = public_key;
        }

        /// <summary>
        /// Retourne une instance de morceau correspondant au lien demandé
        /// </summary>
        /// <param name="uri">Len à récupérer</param>
        /// <returns></returns>
        public Track resolveTrack(string uri)
        {
            if (SoundCloud.isCompatible(uri) == false)
                throw new Exception("Vous devez vérifier que le lien est compatible.");
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create("http://api.soundcloud.com/resolve?url=" + uri + "&client_id=" + this._public_key);
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch(WebException e)
            {
                return null;
            }
            StreamReader stream = new StreamReader(response.GetResponseStream());
            string data = stream.ReadToEnd();
            Track track = JsonConvert.DeserializeObject<Track>(data);
            track.client_id = this._public_key;
            track.base_url = uri;
            return track;

        }

        /// <summary>
        /// Retourne vrai si le lien passé en paramètre est un lien soundcloud
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool isCompatible(string uri)
        {
            return uri.StartsWith("https://soundcloud.com") || uri.StartsWith("https://m.soundcloud.com");
        }
    }
}