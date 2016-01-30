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
    public class Track : ITrack
    {
        public string link { get; set; }
        public string base_url { get; set; }
        IWavePlayer outer = null;
        WaveStream data = null;
        bool terminated = false;

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
            if (play)
                this.play();
            return true;
        }

        public void play()
        {
            if (data != null)
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

        void outer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            terminated = true;
            this.stop();
        }

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

        public void dispose()
        {
            if (outer != null)
                throw new Exception("Le Track doit etre stoppé avant de dispose");
            if (this.data == null)
                return;
            this.data.Dispose();
            this.data = null;
        }

        public string getRemote()
        {
            return this.link;
        }

        public string getTitle()
        {
            return this.getRemote();
        }

        public string getUrl()
        {
            return base_url;
        }

        public bool isTerminated()
        {
            return terminated;
        }

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
        public Track resolveTrack(string uri)
        {
            Console.WriteLine("Retrieving " + uri);
            var response = WebRequest.Create("http://www.youtubeinmp3.com/fetch/?format=JSON&video=" + uri).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            Console.WriteLine("Retrieved " + uri);
            Track r = JsonConvert.DeserializeObject<Track>(stream.ReadToEnd());
            r.base_url = uri;
            return r;
        }

    }
}
