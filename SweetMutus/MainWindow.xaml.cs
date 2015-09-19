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

using Aldentea.Wpf.Application;
using System.ComponentModel;
using HyperMutus;

namespace Aldentea.SweetMutus
{
	using Data;

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : BasicWindow, INotifyPropertyChanged
	{

		#region プロパティ

		protected SweetMutusDocument MyDocument
		{
			get { return (SweetMutusDocument)App.Current.Document; }
		}

		#endregion

		public MainWindow()
		{
			InitializeComponent();
			this.FileHistoryShortcutParent = menuItemHistory;


			MyDocument.Initialized += MyDocument_Initialized;
			MyDocument.Opened += MyDocument_Opened;

			//複数曲追加
			//this.MyDocument.AddSongsAction = this.AddSongsParallel;

			dataGridQuestions.Items.SortDescriptions.Add(
				new SortDescription { Direction = ListSortDirection.Ascending, PropertyName = "No" }
			);

			// 曲再生関連
			//_songPlayer.Volume = App.Current.MySettings.SongPlayerVolume;
			_songPlayer.MediaOpened += SongPlayer_MediaOpened;

			CommandBindings.Add(
				new CommandBinding(ApplicationCommands.Close,
					Close_Executed, Always_CanExecute)
			);
			CommandBindings.Add(
				new CommandBinding(ApplicationCommands.Undo,
					Undo_Executed, Undo_CanExecute)
			);
		}

		#region コマンドハンドラ

		// (0.2.0)
		#region Close
		private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}
		#endregion

		// (0.0.2)
		#region SaveAsMtq
		private void SaveAsMtq_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.SaveAsMtq();
		}
		#endregion

		#region Undo

		private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MyDocument.CanUndo)
			{
				MyDocument.Undo();
			}
		}

		private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = MyDocument.CanUndo;
		}

		#endregion

		#region AddQuestions

		private void AddQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is bool && (bool)e.Parameter)
			{
				AddQuestionsFromDirectory();
			}
			else
			{
				AddQuestions();
			}
		}

		//private void AddQuestions_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		//{
		//	e.CanExecute = MyDocument.CanUndo;
		//}

		#endregion

		#region Export

		private void Export_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.Export();
		}


		#endregion

		#region SetRootDirectoryCommand

		private void SetRootDirectory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.SetRootDirectory();
		}


		#endregion

		#region AddCategory

		private void AddCategory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string)
			{
				string category = (string)e.Parameter;
				if (!comboBoxCategories.Items.Contains(category))
				{
					comboBoxCategories.Items.Add(category);
				}
			}
		}

		private void AddCategory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is string) && !comboBoxCategories.Items.Contains((string)e.Parameter);
		}
		
		#endregion


		#region SaveSongInformation

		private void SaveSongInformation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SweetQuestion)
			{
				SweetQuestion question = (SweetQuestion)e.Parameter;
				if (this.SongPlayer.MediaSource == new Uri(question.FileName))
				{
					this.SongPlayer.Close();
				}
				question.SaveInformation();
			}
		}

		private void SaveSongInformation_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is SweetQuestion);
		}
	
		#endregion

		#region ChangeFileName

		private void ChangeFileName_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SweetQuestion)
			{
				SweetQuestion question = (SweetQuestion)e.Parameter;
				// 手抜きだがここに処理を書く。
				var dialog = new ChangeFileNameDialog { FileName = question.FileName };
				if (dialog.ShowDialog() == true)
				{
					try
					{
						System.IO.File.Move(question.FileName, dialog.FileName);
						question.FileName = dialog.FileName;
					}
					catch (System.IO.IOException ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
			}

		}

		private void ChangeFileName_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is SweetQuestion);
		}

		#endregion


		#endregion


		// カレントカテゴリ関連

		void MyDocument_Initialized(object sender, EventArgs e)
		{
			this.comboBoxCategories.Items.Clear();
			this.comboBoxCategories.Items.Add(string.Empty);

			this.SongPlayer.Close();
			this.CurrentSong = null;
			this.expanderSongPlayer.IsExpanded = false;
		}

		void MyDocument_Opened(object sender, EventArgs e)
		{
			this.comboBoxCategories.Items.Clear();
			foreach (var category in MyDocument.Questions.Categories)
			{
				this.comboBoxCategories.Items.Add(category);
			}

			// ☆将来的にはリボンにするのがいいのかな？
			// カテゴリグループボックスを表示。
			if (this.comboBoxCategories.Items.Count > 1)
			{
				this.menuItemCategoryVisible.IsChecked = true;
			}
		}

		private void comboBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == (string)comboBoxCategories.SelectedItem;
			
		}



		public string CurrentCategory
		{
			get { return (string)GetValue(CurrentCategoryProperty); }
			set { SetValue(CurrentCategoryProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentCategory.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentCategoryProperty =
				DependencyProperty.Register("CurrentCategory", typeof(string), typeof(MainWindow),
				new PropertyMetadata(string.Empty, CurrentCategoryChanged));

		static void CurrentCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = (MainWindow)d;
			window.dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == (string)e.NewValue;
		}

		#region 問題リスト関連

		void AddQuestions()
		{
			// ファイルダイアログを表示．
			var fileNames = HyperMutus.Helpers.SelectSongFiles();	// これのためにMutusBaseを参照している！(いや，もう1か所あった．)
			if (fileNames != null)
			{
				AddQuestions(fileNames);
			//SayInfo("曲追加完了！");
			}
		}

		void AddQuestionsFromDirectory()
		{
			var dialog = new Wpf.Controls.FolderBrowserDialog
			{
				Description = "指定したフォルダ以下にある全ての音楽ファイル(既定では'*.mp3')を問題リストに追加します．",
				Title = "フォルダから曲を追加",
				DisplaySpecialFolders = Wpf.Controls.SpecialFoldersFlag.Personal | Wpf.Controls.SpecialFoldersFlag.MyMusic
			};
			if (dialog.ShowDialog() == true)
			{
				var directory = dialog.SelectedPath;
				AddQuestions(System.IO.Directory.GetFiles(directory, "*.mp3", System.IO.SearchOption.AllDirectories));
				if (string.IsNullOrEmpty(MyDocument.Questions.RootDirectory))
				{
					MyDocument.Questions.RootDirectory = directory;
				}
			}
		}

		#region 問題を追加(AddQuestions)
		public void AddQuestions(IEnumerable<string> fileNames)
		{
			this.MyDocument.AddQuestions(fileNames);
		}

		IList<SweetQuestion> AddQuestionsParallel(IEnumerable<string> fileNames)
		{
			List<SweetQuestion> added_questions = new List<SweetQuestion>();

			Action<string> action = (fileName) =>
			{
				// ObservableCollectionに対する操作は，それが作られたスレッドと同じスレッドで行う必要がある．

				var question = this.Dispatcher.Invoke(
					new Func<string, SweetQuestion>(delegate(string f) { return MyDocument.AddQuestion(f); }), fileName);
				if (question is SweetQuestion)
				{
					added_questions.Add((SweetQuestion)question);
				}
					
			};
			HyperMutus.Helpers.WorkBackgroundParallel<string>(fileNames, action);
			return added_questions;
		}
		#endregion

		#endregion


		void SaveAsMtq()
		{
			var dialog = new Microsoft.Win32.SaveFileDialog { Filter = "mtqファイル(*.mtq)|*.mtq", DefaultExt = ".mtq" };
			if (dialog.ShowDialog() == true)
			{
				this.Document.SaveCopyAs(dialog.FileName);

			}
		}


		void Export()
		{
			// ファイル名選択
			var dialog = new HyperMutus.ExportDialog { FileFilter = this.SaveFileDialogFilter };
			if (dialog.ShowDialog() == true)
			{
				string fileName = dialog.Destination;
				//var document = IntroMutusDocument.Clone<IntroMutusDocument>();
				
				// 曲ファイルコピー
				var songDirectory = dialog.SongDirectory;
				var destinationDirectory = System.IO.Path.GetDirectoryName(fileName);
				string songsDestination = songDirectory == null ? destinationDirectory : System.IO.Path.Combine(destinationDirectory, songDirectory);

				//document.SongsRoot = songsDestination;
				Helpers.ExportFiles(MyDocument.Questions.Select(q => q.FileName), songsDestination);

				// ドキュメント保存
				MyDocument.SaveExport(fileName, songDirectory);
			}
			
		}


		private void SetRootDirectory()
		{
			var directory = HyperMutus.Helpers.SelectSongsRoot(this.MyDocument.Questions.RootDirectory);
			if (directory != null)
			{
				MyDocument.Questions.RootDirectory = directory;
			}
		}


		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			var expander = (Expander)sender;
			if (expander.IsExpanded)
			{
				comboBoxCategories.Visibility = Visibility.Collapsed;
				expander.ToolTip = "クリックするとカテゴリ選択に戻ります．";
			}
			else
			{
				comboBoxCategories.Visibility = Visibility.Visible;
				// ☆デフォルトでこれを表示させる必要がある．
				expander.ToolTip = "カテゴリを追加するにはクリックして下さい．";
			}
		}


		#region 曲再生関連

		#region *SongPlayerプロパティ
		public HyperMutus.SongPlayer SongPlayer
		{
			get
			{
				return _songPlayer;
			}
		}
		HyperMutus.SongPlayer _songPlayer = new HyperMutus.SongPlayer();
		#endregion


		// (0.3.2)プロパティ化。
		#region *CurrentSongプロパティ
		public SweetQuestion CurrentSong
		{
			get { return _currentSong; }
			set
			{
				if (_currentSong != value)
				{
					_currentSong = value;
					NotifyPropertyChanged("CurrentSong");
				}
			}
		}
		SweetQuestion _currentSong = null;
		#endregion

		//DispatcherTimer _songPlayerTimer = null;

		// (0.3.2)

		void SongPlayer_MediaOpened(object sender, EventArgs e)
		{
			if (_songPlayer.Duration.HasValue)
			{
				this.labelDuration.Content = _songPlayer.Duration.Value;
				this.sliderSeekSong.Maximum = _songPlayer.Duration.Value.TotalSeconds;
			}
		}

		#region Playコマンド
		void Play_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SweetQuestion)
			{
				var song = (SweetQuestion)e.Parameter;
				_songPlayer.Open(song.FileName);
				//_songPlayerTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.25) }; // 可変にする？
				//_songPlayerTimer.Tick += SongPlayerTimer_Tick;
				//_songPlayerTimer.IsEnabled = true;
				this.CurrentSong = song;
				_songPlayer.Play();
			}
		}

		void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = e.Parameter is SweetQuestion;
		}
		#endregion


		#region SwitchPlayPauseコマンド
		void SwitchPlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_songPlayer.TogglePlayPause();
			}
		}
		#endregion

		void SeekRelative_executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				double sec;
				if (Double.TryParse(e.Parameter.ToString(), out sec))
				{
				_songPlayer.CurrentPosition = _songPlayer.CurrentPosition.Add(TimeSpan.FromSeconds(sec));
				}
			}
		}

		void SeekSabi_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_songPlayer.CurrentPosition = _currentSong.SabiPos;
			}
		}

		void SongPlayer_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _songPlayer.CurrentState != SongPlayer.State.Inactive;
		}

		#region SetSabiPosコマンド
		void SetSabiPos_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_currentSong.SabiPos = _songPlayer.CurrentPosition;
			}
		}
		#endregion

		private void UpDownControl_UpClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				CurrentSong.SabiPos += TimeSpan.FromSeconds(0.1);
			}
		}

		private void UpDownControl_DownClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				CurrentSong.SabiPos += TimeSpan.FromSeconds(-0.1);
			}
		}

		#endregion

		//	#endregion

		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion

		#region お試し

		private void MenuItemFilter_Click(object sender, RoutedEventArgs e)
		{
			//dataGridQuestions.Items.Filter = (q) => { return ((SweetQuestion)q).Category == "tanuki"; };
			//dataGridQuestions.Items.SortDescriptions.Add(
			//	new SortDescription { Direction = ListSortDirection.Ascending, PropertyName = "No" }
			//);
			var song = dataGridQuestions.SelectedItem as SweetQuestion;
			if (song != null)
			{
				MediaCommands.Play.Execute(song, this);
				this.expanderSongPlayer.IsExpanded = true;
			}
		}

		#endregion


	}






}
