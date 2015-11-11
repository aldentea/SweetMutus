using System.Windows;
using System.Windows.Input;

namespace Aldentea.SweetMutus
{

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

		#endregion

	}
	#endregion

}
