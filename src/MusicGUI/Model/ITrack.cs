using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music
{

    /// <summary>
    /// Interface globale représentant un morceau
    /// </summary>
    public interface ITrack
    {
        bool load(bool play = false);
        void play();
        void dispose();
        void stop();
        string getUrl();
        string getRemote();
        string getTitle();
        bool isTerminated();
        void reset();
    }
}
