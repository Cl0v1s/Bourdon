using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Music;
using System.Threading;
using System.Windows.Forms;

namespace MusicGUI
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Playlist playlist;
        private SoundCloud.SoundCloud soundcloud_client;
        private Youtube.Youtube youtube_client;
        private Thread updater;

        public MainWindow()
        {
            InitializeComponent();
            playlist = new Playlist();
            soundcloud_client = new SoundCloud.SoundCloud("bb9515b11ad6d190d296583917f534fd");
            youtube_client = new Youtube.Youtube();
            updater = new Thread(new ThreadStart(this.update));
            updater.Start();

            //TODO: a supprimer après les tests
            playlist.add(new PlayListEntry(soundcloud_client.resolveTrack("https://soundcloud.com/chiptune/unreal-superhero-3"), "cloclo", false));
            playlist.add(new PlayListEntry(youtube_client.resolveTrack("https://www.youtube.com/watch?v=9GkVhgIeGJQ"),"clacla", false));

            this.playlist.next();
        }

        public void update()
        {
            while (1 == 1)
            {
                this.playlist.update();
                MethodInvoker inv = delegate
                {
                    if (this.playlist.playing != null)
                        playing_link.Content = this.playlist.playing.getTitle();
                    else
                        playing_link.Content = "Inconnu";
                };
                this.Dispatcher.Invoke(inv);
                MethodInvoker inv2 = delegate
                {
                    if (this.playlist.playing != null)
                        playing_user.Content = this.playlist.playing.user;
                    else
                        playing_user.Content = "Personne";
                };
                this.Dispatcher.Invoke(inv2);

                Thread.Sleep(200);
            }
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            this.playlist.next();
        }

        private void playing_ban_Click(object sender, RoutedEventArgs e)
        {
            this.playlist.banCurrent();
        }

        private void ban_button_Click(object sender, RoutedEventArgs e)
        {
            string user = (string)this.ban_name.Content;
            this.playlist.ban(user);
        }
    }
}
