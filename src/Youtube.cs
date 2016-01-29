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


namespace Youtube
{
    class Track
    {
        public string link { get; set; }
        IWavePlayer outer = null;

        public void play()
        {
            if (outer == null && link.Length > 0)
            {
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

    class Response
    {
        public string link {get;set;}
    }

    class Youtube
    {
        public Track resolveTrack(string uri)
        {
            var response = WebRequest.Create("http://www.youtubeinmp3.com/fetch/?format=JSON&video=" + uri).GetResponse();
            StreamReader stream = new StreamReader(response.GetResponseStream());
            Track r = JsonConvert.DeserializeObject<Track>(stream.ReadToEnd());
            return r;
        }

    }
}
