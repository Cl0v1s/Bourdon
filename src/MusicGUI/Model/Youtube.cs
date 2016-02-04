using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using NAudio;
using NAudio.Wave;
using Newtonsoft.Json;
using Music;

namespace Youtube
{

    /// <summary>
    /// Instance de morceau propre à Youtube
    /// </summary>
    public class Track : ITrack
    {
        public string link { get; set; }
        public string base_url { get; set; }
        IWavePlayer outer = null;
        WaveStream data = null;
        bool terminated = false;
        public string title { get; set; }
        public DateTime lastPlayed = DateTime.Now;

        /// <summary>
        /// Charge le morceau en mémoire
        /// </summary>
        /// <param name="play">Si vrai, joue le morceau en fin de chargement</param>
        /// <returns></returns>
        public bool load(bool play = false)
        {
            if (link.Length <= 0)
                return false;
            terminated = false;
            Console.WriteLine("loading " + link);
            var response = WebRequest.Create(link).GetResponse();
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
            this.lastPlayed = DateTime.Now;
            if (play)
                this.play();
            return true;
        }

        /// <summary>
        /// Joue le morceau
        /// </summary>
        public void play()
        {
            if (data != null || this.lastPlayed.AddMinutes(13) < DateTime.Now)
            {
                terminated = false;
                Console.WriteLine("Playing " + link);
                outer = new WaveOut();
                outer.Init(this.data);
                outer.Play();
                outer.PlaybackStopped += outer_PlaybackStopped;
            }
            else
                this.load(true);
        }

        /// <summary>
        /// Appelé lorsque le morceau est terminé
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void outer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            terminated = true;
            this.stop();
        }

        /// <summary>
        /// Arrete la leture du morceau
        /// </summary>
        public void stop()
        {
            if (outer != null)
            {
                outer.Stop();
                if(outer != null)
                    outer.Dispose();
                outer = null;
            }
        }

        /// <summary>
        /// Libère la mémoire occupée par le stream du morceau
        /// </summary>
        public void dispose()
        {
            if (outer != null)
                throw new Exception("Le Track doit etre stoppé avant de dispose");
            if (this.data == null)
                return;
            this.data.Dispose();
            this.data = null;
        }


        /// <summary>
        /// Récupère l'URL de stream
        /// </summary>
        /// <returns></returns>
        public string getRemote()
        {
            return this.link;
        }

        /// <summary>
        /// Récupère le titre du morceau
        /// </summary>
        /// <returns></returns>
        public string getTitle()
        {
            return this.title;
        }


        /// <summary>
        /// écupère l'URL de base du morceau
        /// </summary>
        /// <returns></returns>
        public string getUrl()
        {
            return base_url;
        }

        /// <summary>
        /// Retourne si le morceau est fini ou non
        /// </summary>
        /// <returns></returns>
        public bool isTerminated()
        {
            return terminated;
        }

        /// <summary>
        /// Remet les stats du morceau à l'orignal
        /// </summary>
        public void reset()
        {
            this.terminated = false;
        }

    }

    class Response
    {
        public string link {get;set;}
    }

    public class Youtube
    {

        /// <summary>
        /// Convertit un lien youtube en Morceau jouable 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public Track resolveTrack(string uri)
        {
            if(Youtube.isCompatible(uri) == false)
                throw new Exception("Vous devez vérifier que le lien est compatible.");

            Console.WriteLine("Retrieving " + uri);
            HttpWebResponse response = null;
            response = (HttpWebResponse)WebRequest.Create("http://www.youtubeinmp3.com/fetch/?format=JSON&video=" + uri).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            Console.WriteLine("Retrieved :" + uri+":");
            string data = stream.ReadToEnd();
            if (data == "{\"error\":\"no video\"}")
                return null;
            Track r = JsonConvert.DeserializeObject<Track>(data);
            r.base_url = uri;
            return r;
        }

        /// <summary>
        /// Retourne si le lien pass en paramètre est bien un lien youtube
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool isCompatible(string uri)
        {
            return uri.StartsWith("https://m.youtube.com") || uri.StartsWith("https://www.youtube.com");
        }

    }
}
