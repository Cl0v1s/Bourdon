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
            Youtube.Youtube youtube_client = new Youtube.Youtube();
            youtube_client.resolveTrack("https://www.youtube.com/watch?v=3gxNW2Ulpwk").play();
            Console.ReadLine();
        }
    }
}
