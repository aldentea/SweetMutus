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

namespace Aldentea.SweetMutus
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

		#endregion

	}
}
