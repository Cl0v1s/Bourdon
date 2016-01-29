using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{
    interface ITrack
    {
        bool load(bool play = false);
        void play();
        void dispose();
        void stop();
        string getRemote();
    }
}
