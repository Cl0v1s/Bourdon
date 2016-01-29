using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoundCloud;
using System.Runtime.Serialization;

namespace Music
{
    class Program
    {
        static void Main(string[] args)
        {
            SoundCloud.SoundCloud soundcloud_client = new SoundCloud.SoundCloud("bb9515b11ad6d190d296583917f534fd");
            soundcloud_client.resolveTrack("https://soundcloud.com/klefki/homestuck-the-carnival").play();
            Console.ReadLine();
        }
    }
}
