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

using System.ComponentModel;
using IO = System.IO;

namespace Aldentea.SweetMutus.Net6.Base
{

	// HyperMutusよりコピペ。

	/// <summary>
	/// ChangeFileNameDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ChangeFileNameDialog : Window, INotifyPropertyChanged
	{

		#region プロパティ

		#region *[dependency]FileNameプロパティ
		public string FileName
		{
			get
			{
				return (string)GetValue(FileNameProperty);
			}
			set
			{
				SetValue(FileNameProperty, value);
			}
		}

		public static readonly DependencyProperty FileNameProperty
			= DependencyProperty.Register("FileName", typeof(string), typeof(ChangeFileNameDialog),
						new PropertyMetadata(null, (sender, e) =>
						{
							var dialog = (ChangeFileNameDialog)sender;
							dialog.NotifyPropretyChanged("FilePath");
							dialog.NotifyPropretyChanged("FileMainName");
							dialog.NotifyPropretyChanged("FileExtension");
						})
				);
		#endregion

		// 以下3つはprivateでもいいのか？

		#region *FilePathプロパティ
		public string FilePath
		{
			get
			{
				return IO.Path.GetDirectoryName(this.FileName);
			}
			set
			{
				FileName = IO.Path.Combine(value, string.Join(".", FileMainName, FileExtension));
			}
		}
		#endregion

		#region *FileMainNameプロパティ
		public string FileMainName
		{
			get
			{
				return IO.Path.GetFileNameWithoutExtension(this.FileName);
			}
			set
			{
				FileName = IO.Path.Combine(FilePath, string.Join(".", value, FileExtension));
			}
		}
		#endregion

		#region *FileExtensionプロパティ
		public string FileExtension
		{
			get
			{
				return IO.Path.GetExtension(this.FileName).TrimStart(new char[] { '.' });
			}
			set
			{
				FileName = IO.Path.Combine(FilePath, string.Join(".", FileMainName, value));
			}
		}
		#endregion

		#endregion


		#region *コンストラクタ(ChangeFileNameDialog)
		public ChangeFileNameDialog()
		{
			InitializeComponent();
		}
		#endregion

		// 12/09/2011 by aldentea
		#region *ロード時(Window_Loaded)
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.textBoxName.Focus())
			{
				this.textBoxName.SelectionStart = 0;
				this.textBoxName.SelectionLength = textBoxName.Text.Length;
			}
		}
		#endregion


		#region ボタンイベントハンドラ

		#region *[OK]クリック時(buttonOK_Click)
		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}
		#endregion

		#region *[キャンセル]クリック時(buttonCancel_Click)
		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			//this.Close();

		}
		#endregion

		#endregion


		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropretyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
}
