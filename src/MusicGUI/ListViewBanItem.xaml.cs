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
    /// Logique d'interaction pour ListViewBanItem.xaml
    /// </summary>
    public partial class ListViewBanItem : UserControl
    {
        string name;
        Music.Playlist playlist;

        public ListViewBanItem(string name, Music.Playlist playlist)
        {
            this.name = name;
            this.playlist = playlist;
            InitializeComponent();
            this.user_name.Content = name;
        }

        private void unban_Click(object sender, RoutedEventArgs e)
        {
            this.playlist.unban(this.name);
        }
    }
}
