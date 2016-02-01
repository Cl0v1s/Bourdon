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
        private Twitter.Twitter twitter_client;
        private Google.Google google_client;
        private Thread updater;
        private Thread retriever;

        public MainWindow()
        {
            InitializeComponent();

            twitter_client = new Twitter.Twitter();
            google_client = new Google.Google();

            playlist = new Playlist();
            soundcloud_client = new SoundCloud.SoundCloud(APIKeyProvider.Soundcloud_ClientId);
            youtube_client = new Youtube.Youtube();
            playlist.load(youtube_client,soundcloud_client);

            retriever = new Thread(new ThreadStart(this.retrieveEntries));
            retriever.Start();

            updater = new Thread(new ThreadStart(this.update));
            updater.Start();

            //TODO: a supprimer après les tests
           /* playlist.add(new PlayListEntry(soundcloud_client.resolveTrack("https://soundcloud.com/chiptune/unreal-superhero-3"), "cloclo", false));
            playlist.add(new PlayListEntry(soundcloud_client.resolveTrack("https://soundcloud.com/chiptune/positive-waves"), "clocla", false));
            playlist.add(new PlayListEntry(soundcloud_client.resolveTrack("https://soundcloud.com/prep-school-recordings/eion-hyper-active-original-mix"), "cloclu", false));
            */

            //this.playlist.next();
        }

        public void retrieveEntries()
        {
            while(1==1)
            {
                playlist.add(twitter_client.getPlaylistEntriesFromTweets(youtube_client, soundcloud_client));
                playlist.add(google_client.getPlaylistEntriesFromMail(youtube_client, soundcloud_client));
                Thread.Sleep(60 * 1000);
            }
        }

        public void update()
        {
            while (1 == 1)
            {
                this.playlist.update();
                //Mise à jour des informations sur le titre en cours
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
                //Mise à jour de la liste des bannis
                MethodInvoker inv3 = delegate
                {
                    if (ban_list.Items.Count == this.playlist.banned.Count())
                        return;
                    ban_list.Items.Clear();
                    foreach(string ban in this.playlist.banned)
                    {
                        Console.WriteLine("Adding " + ban + " to list of banned");
                        ListViewBanItem item = new ListViewBanItem(ban,this.playlist);
                        this.ban_list.Items.Add(item);
                    }
                    
                };
                this.Dispatcher.Invoke(inv3);
                //Mise à jour de la liste des morceaux
                MethodInvoker inv4 = delegate
                {
                    if (this.playlist_list.Items.Count == this.playlist.to_play.Count())
                        return;
                    this.playlist_list.Items.Clear();
                    foreach (PlayListEntry entry in this.playlist.to_play)
                    {
                        Console.WriteLine("Adding " + entry.getTitle() + " to list of playlist");
                        ListViewPlaylistItem item = new ListViewPlaylistItem(entry, this.playlist);
                        this.playlist_list.Items.Add(item);
                    }

                };
                this.Dispatcher.Invoke(inv4);
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
            string user = (string)this.ban_name.Text;
            this.playlist.ban(user);
            this.ban_name.Text = "";
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.playlist.save();
            this.updater.Abort();
            this.retriever.Abort();
        }
    }
}
