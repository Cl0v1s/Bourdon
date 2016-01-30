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

        void outer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            terminated = true;
            this.stop();
        }

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

        public string getRemote()
        {
            return this.stream_url;
        }

        public string getTitle()
        {
            return this.title;
        }

        public string getUrl()
        {
            return base_url;
        }
        public bool isTerminated()
        {
            return this.terminated;
        }

        public void reset()
        {
            this.terminated = false;
        }
    }


    public class SoundCloud
    {

        private string _public_key;

        public SoundCloud(string public_key)
        {
            this._public_key = public_key;
        }

        public Track resolveTrack(string uri)
        {
            HttpWebRequest request;
            request = (HttpWebRequest)WebRequest.Create("http://api.soundcloud.com/resolve?url=" + uri + "&client_id=" + this._public_key);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            string data = stream.ReadToEnd();
            Track track = JsonConvert.DeserializeObject<Track>(data);
            track.client_id = this._public_key;
            track.base_url = uri;
            return track;

        }
    }
}