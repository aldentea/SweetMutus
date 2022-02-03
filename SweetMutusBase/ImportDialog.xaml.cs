using System.Windows;
using System.Windows.Input;



namespace Aldentea.SweetMutus.Base
{
	using Data;

	#region ImportDialogクラス
	/// <summary>
	/// ImportDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ImportDialog : Window
	{
		#region *コンストラクタ(ImportDialog)
		public ImportDialog()
		{
			InitializeComponent();
		}
		#endregion

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
		static bool HitFilter(GrandMutus.Data.ISong song, string filter)
		{
			// とりあえず大文字小文字を判別する．
			return song.Title.Contains(filter) || song.Artist.Contains(filter);
		}

		#endregion

		// (0.1.0.2)
		private void textBoxFilter_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			dataGridSongs.Items.Filter = (song) => HitFilter((GrandMutus.Data.ISong)song, textBoxFilter.Text);
		}
	}
	#endregion

}
