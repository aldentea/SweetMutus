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
using System.Windows.Shapes;

using GrandMutus.Net6.Data;

namespace Aldentea.SweetMutus.Net6
{
	/// <summary>
	/// ImportDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ImportDialog : Window
	{
		public ImportDialog()
		{
			InitializeComponent();
		}

		#region コマンドハンドラ

		#region Close

		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void Always_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		#endregion

		// コマンドを使おうと思ったけど，TextBoxのTextが変更になったときに使えるコマンドのトリガがなかったので，
		// イベントで頑張ることにする．

		// (0.1.0.2)
		static bool HitFilter(ISong song, string filter)
		{
			// とりあえず大文字小文字を判別する．
			return song.Title.Contains(filter) || song.Artist.Contains(filter);
		}

		#endregion

		// (0.1.0.2)
		private void textBoxFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			dataGridSongs.Items.Filter = (song) => HitFilter((ISong)song, textBoxFilter.Text);
		}


	}
}
