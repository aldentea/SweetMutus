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

namespace Aldentea.SweetMutus
{

	// HyperMutusよりコピペ。

	/// <summary>
	/// ChangeFileNameDialog.xaml の相互作用ロジック
	/// </summary>
	public partial class ChangeFileNameDialog : Window, INotifyPropertyChanged
	{

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

		public ChangeFileNameDialog()
		{
			InitializeComponent();
		}


		#region *INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropretyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		private void buttonOK_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = false;
			//this.Close();

		}

		// 12/09/2011 by aldentea
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (this.textBoxName.Focus())
			{
				this.textBoxName.SelectionStart = 0;
				this.textBoxName.SelectionLength = textBoxName.Text.Length;
			}
		}

	}
}
