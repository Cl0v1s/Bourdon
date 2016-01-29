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

namespace SoundCloud
{

    class Track
    {
        IWavePlayer outer = null;

        //Ressources JSON
        public string client_id { get; set; }
        public int id { get; set; }
        //TODO: insérer ici les valeurs non utilisées par le projet
        public string title { get; set; }
        public string permalink_url { get; set; }
        public string uri { get; set; }
        public int duration { get; set; }
        public bool streamable { get; set; }
        public string stream_url { get; set; }

        public void play()
        {
            if (outer == null && stream_url.Length > 0 && streamable == true && client_id != null && client_id != "")
            {
                var response = WebRequest.Create(stream_url+"?client_id="+client_id).GetResponse();
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
                WaveStream blockAlignedStream = new BlockAlignReductionStream(WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(ms)));
                outer = new WaveOut();
                outer.Init(blockAlignedStream);
                outer.Play();

            }
        }

        public void stop()
        {
            if (outer != null)
            {
                outer.Stop();
                outer.Dispose();
                outer = null;
            }
        }
    }


    class SoundCloud
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
            return track;

        }
    }
}