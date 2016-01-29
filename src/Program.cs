using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundCloud;
using Youtube;

namespace Music
{
    class Program
    {
        static void Main(string[] args)
        {
            Playlist playlist = new Playlist();
            Youtube.Youtube youtube_client = new Youtube.Youtube();
            SoundCloud.SoundCloud soundcloud_client = new SoundCloud.SoundCloud("bb9515b11ad6d190d296583917f534fd");


            Console.ReadLine();
        }
    }
}
