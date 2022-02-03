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

namespace Aldentea.SweetMutus.Base
{
	/// <summary>
	/// ExportTextWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class ExportTextWindow : Window
	{

		public ExportTextSettings Settings
		{
			get
			{
				return _settings;
			}
		}
		ExportTextSettings _settings = new ExportTextSettings();

		public ExportTextWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
	}
}
