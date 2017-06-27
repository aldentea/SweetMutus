using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aldentea.SweetMutus
{

	// (0.2.6)
	#region ExportTextSettingsクラス
	public class ExportTextSettings : Data.IExportQuestionsListParameter, INotifyPropertyChanged
	{

		#region Destinationプロパティ
		public string Destination
		{
			get
			{
				return _destination;
			}
			set
			{
				if (Destination != value)
				{
					_destination = value;
					NotifyPropertyChanged();
				}
			}
		}
		string _destination = string.Empty;
		#endregion



		#region IDOutputプロパティ
		public bool IDOutput
		{
			get
			{
				return _idOutput;
			}
			set
			{
				if (IDOutput != value)
				{
					_idOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _idOutput = false;
		#endregion

		#region CategoryOutputプロパティ
		public bool CategoryOutput
		{
			get
			{
				return _categoryOutput;
			}
			set
			{
				if (CategoryOutput != value)
				{
					_categoryOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _categoryOutput = false;
		#endregion

		#region NoOutputプロパティ
		public bool NoOutput
		{
			get
			{
				return _noOutput;
			}
			set
			{
				if (NoOutput != value)
				{
					_noOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _noOutput = false;
		#endregion

		#region TitleOutputプロパティ
		public bool TitleOutput
		{
			get
			{
				return _titleOutput;
			}
			set
			{
				if (TitleOutput != value)
				{
					_titleOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _titleOutput = true;
		#endregion

		#region ArtistOutputプロパティ
		public bool ArtistOutput
		{
			get
			{
				return _artistOutput;
			}
			set
			{
				if (ArtistOutput != value)
				{
					_artistOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _artistOutput = true;
		#endregion

		#region FileNameOutputプロパティ
		public bool FileNameOutput
		{
			get
			{
				return _fileNameOutput;
			}
			set
			{
				if (FileNameOutput != value)
				{
					_fileNameOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _fileNameOutput = false;
		#endregion

		#region SabiPosOutputプロパティ
		public bool SabiPosOutput
		{
			get
			{
				return _sabiPosOutput;
			}
			set
			{
				if (SabiPosOutput != value)
				{
					_sabiPosOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _sabiPosOutput = false;
		#endregion

		#region PlayPosOutputプロパティ
		public bool PlayPosOutput
		{
			get
			{
				return _playPosOutput;
			}
			set
			{
				if (PlayPosOutput != value)
				{
					_playPosOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _playPosOutput = false;
		#endregion

		#region StopPosOutputプロパティ
		public bool StopPosOutput
		{
			get
			{
				return _stopPosOutput;
			}
			set
			{
				if (StopPosOutput != value)
				{
					_stopPosOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _stopPosOutput = false;
		#endregion

		#region MemoOutputプロパティ
		public bool MemoOutput
		{
			get
			{
				return _memoOutput;
			}
			set
			{
				if (MemoOutput != value)
				{
					_memoOutput = value;
					NotifyPropertyChanged();
				}
			}
		}
		bool _memoOutput = false;
		#endregion


		#region INotifyPropertyChangedお決まりの実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

	}
	#endregion

}
