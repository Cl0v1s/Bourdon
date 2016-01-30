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

namespace MusicGUI
{
    /// <summary>
    /// Logique d'interaction pour ListViewPlaylistItem.xaml
    /// </summary>
    public partial class ListViewPlaylistItem : UserControl
    {

        Music.PlayListEntry entry;
        Music.Playlist playlist;

        public ListViewPlaylistItem(Music.PlayListEntry entry, Music.Playlist playlist)
        {
            InitializeComponent();
            this.entry = entry;
            this.title.Content = entry.getTitle();
            this.user.Content = entry.user;
            this.playlist = playlist;
        }

        private void remove_Click(object sender, RoutedEventArgs e)
        {
            this.playlist.remove(this.entry);
        }
    }
}
