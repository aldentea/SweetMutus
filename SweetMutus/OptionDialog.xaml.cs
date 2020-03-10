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

	// (0.2.6)
	#region OptionDialogクラス
	/// <summary>
	/// OptionDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class OptionDialog : Window
	{
		
		#region *AutoPlayOnNextプロパティ
		public bool AutoPlayOnNext
		{
			get
			{
				return (bool)GetValue(AutoPlayOnNextProperty);
			}
			set
			{
				SetValue(AutoPlayOnNextProperty, value);
			}
		}

		// よくわからないが、複数回呼び出すと例外が発生する。
		// ↑staticにするのを忘れていたｗ
		public static DependencyProperty AutoPlayOnNextProperty
			= DependencyProperty.Register(
				"AutoPlayOnNext", typeof(bool), typeof(OptionDialog), new PropertyMetadata(true));
		#endregion

		#region *UseMutus2RandomRantroプロパティ
		public bool UseMutus2RandomRantro
		{
			get => (bool)GetValue(UseMutus2RandomRantroProperty);
			set
			{
				SetValue(UseMutus2RandomRantroProperty, value);
			}
		}
		#endregion

		public static DependencyProperty UseMutus2RandomRantroProperty
			= DependencyProperty.Register(
				"UseMutus2RandomRantro", typeof(bool), typeof(OptionDialog), new PropertyMetadata(false));

		public double Mutus2RandomRantroFactor
		{
			get => (double)GetValue(RandomRantroFactorProperty);
			set => SetValue(RandomRantroFactorProperty, value);
		}

		public static DependencyProperty RandomRantroFactorProperty
			= DependencyProperty.Register(
				"RandomRantroFactor", typeof(double), typeof(OptionDialog), new PropertyMetadata(0.97));



		public OptionDialog()
		{
			InitializeComponent();
		}

		private void ButtonOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			this.Close();
		}
	}
	#endregion

}
