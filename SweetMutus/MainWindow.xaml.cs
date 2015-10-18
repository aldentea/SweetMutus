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
using System.Collections.ObjectModel;

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

		#region *Categoriesプロパティ
		public ObservableCollection<string> Categories
		{
			get
			{
				return _categories;
			}
		}
		ObservableCollection<string> _categories = new ObservableCollection<string>(new string[] {string.Empty});
		#endregion

		#endregion

		// (0.0.6)
		bool Confirm(string message)
		{
			return MessageBox.Show(message, "確認", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
		}

		public MainWindow()
		{
			InitializeComponent();
			this.FileHistoryShortcutParent = menuItemHistory;


			MyDocument.Confirmer = (message) => this.Confirm(message);
			MyDocument.Initialized += MyDocument_Initialized;
			MyDocument.Opened += MyDocument_Opened;
			MyDocument.QuestionCategoryChanged += MyDocument_QuestionCategoryChanged;
			MyDocument.QuestionNoChanged += MyDocument_QuestionCategoryChanged;

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
			// (0.0.7)
			CommandBindings.Add(
				new CommandBinding(ApplicationCommands.Redo,
					Redo_Executed, Redo_CanExecute)
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

		// (0.0.7)
		#region Redo

		private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (MyDocument.CanRedo)
			{
				MyDocument.Redo();
			}
		}

		private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = MyDocument.CanRedo;
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
				this.AddCategory((string)e.Parameter);
			}
		}

		private void AddCategory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is string) && !this.Categories.Contains((string)e.Parameter);
		}

		#endregion

		#region ChangeCategory

		private void ChangeCategory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string && (string)e.Parameter != this.CurrentCategory)
			{
				foreach (var question in this.dataGridQuestions.SelectedItems.Cast<SweetQuestion>().ToArray())
				{
					question.Category = (string)e.Parameter;
				}
			}
		}

		private void ChangeCategory_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is string) && (string)e.Parameter != this.CurrentCategory
				&& this.dataGridQuestions.SelectedItems.Count > 0;
		}


		#endregion


		#region SaveSongInformation

		// (0.0.8.4)気休めにTask.Delayを追加。
		private void SaveSongInformation_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SweetQuestion)
			{
				SweetQuestion question = (SweetQuestion)e.Parameter;
				if (this.SongPlayer.MediaSource == new Uri(question.FileName))
				{
					this.SongPlayer.Close();
					// 無意味にSleepする。
					Task.Delay(140);
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

		// (0.0.8)
		#region IncrementNo

		private void IncrementNo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = e.Parameter as System.Collections.IList;
			if (items != null)
			{
				var questions = items.Cast<SweetQuestion>();
				// 何かいい方法はないかなぁ？
				// (questionsに直接foreachすると，要素を変更してしまうので...)
				var ids = questions.Where(q => q.No.HasValue).OrderByDescending(q => q.No).Select(q => q.ID).ToArray();
				foreach(var id in ids)
				{
					var question = MyDocument.FindQuestion(id);
					question.No += 1;
				}
				// ソートし直す．
				UpdateFilter();
			}
		}

		private void IncrementNo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var items = e.Parameter as System.Collections.IList;
			e.CanExecute = items != null && items.Count > 0;
		}

		#endregion

		// (0.0.8)
		#region DecrementNo

		private void DecrementNo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = e.Parameter as System.Collections.IList;
			if (items != null)
			{
				var questions = items.Cast<SweetQuestion>();
				// 何かいい方法はないかなぁ？
				// (questionsに直接foreachすると，要素を変更してしまうので...)
				var ids = questions.Where(q => q.No.HasValue).OrderBy(q => q.No).Select(q => q.ID).ToArray();
				int i = 1;
				foreach (var id in ids)
				{
					var question = MyDocument.FindQuestion(id);
					if (question.No == i)
					{
						i++;
					}
					else
					{
						question.No -= 1;
					}
				}
				// ソートし直す．
				UpdateFilter();
			}
		}

		#endregion

		// (0.0.8)
		#region OmitQuestions

		private void OmitQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = e.Parameter as System.Collections.IList;
			if (items != null)
			{
				var questions = items.Cast<SweetQuestion>();
				// 何かいい方法はないかなぁ？
				// (questionsに直接foreachすると，要素を変更してしまうので...)
				var ids = questions.Where(q => q.No.HasValue).OrderByDescending(q => q.No).Select(q => q.ID).ToArray();
				foreach (var id in ids)
				{
					var question = MyDocument.FindQuestion(id);
					question.No = null;
				}
				// ソートし直す．
				UpdateFilter();
			}
		}

		#endregion

		// (0.0.8)
		#region EnterQuestions

		private void EnterQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = e.Parameter as System.Collections.IList;
			if (items != null)
			{
				var questions = items.Cast<SweetQuestion>();
				// 何かいい方法はないかなぁ？
				// (questionsに直接foreachすると，要素を変更してしまうので...)
				var ids = questions.Where(q => !q.No.HasValue).Select(q => q.ID).ToArray();
				foreach (var id in ids)
				{
					var question = MyDocument.FindQuestion(id);
					question.No = MyDocument.Questions.Count(q => q.Category == question.Category && q.No.HasValue) + 1;
				}
				// ソートし直す．
				UpdateFilter();
			}
		}

		#endregion

		#endregion


		// カレントカテゴリ関連

		void MyDocument_Initialized(object sender, EventArgs e)
		{
			Categories.Clear();
			AddCategory(string.Empty);
			//this.comboBoxCategories.Items.Clear();
			//this.comboBoxCategories.Items.Add(string.Empty);

			this.SongPlayer.Close();
			this.CurrentSong = null;
			this.expanderSongPlayer.IsExpanded = false;
		}

		void MyDocument_Opened(object sender, EventArgs e)
		{
			Categories.Clear();
			foreach (var category in MyDocument.Questions.Categories)
			{
				this.AddCategory(category);
			}

			// ☆将来的にはリボンにするのがいいのかな？

			// カテゴリグループボックスを表示。
			if (Categories.Count > 1)
			{
				this.menuItemCategoryVisible.IsChecked = true;
			}
			else
			{
				if (Categories.Count < 1)
				{
					this.AddCategory(string.Empty);
				}
				this.menuItemCategoryVisible.IsChecked = false;
			}
			this.comboBoxCategories.SelectedIndex = 0;
			this.comboBoxDestinationCategory.SelectedIndex = 0;

		}

		//private void comboBoxCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
		//{
		//	dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == (string)comboBoxCategories.SelectedItem;
			
		//}



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
			//window.dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == (string)e.NewValue;
			window.UpdateFilter((string)e.NewValue);
		}

		// (0.0.8)
		internal void UpdateFilter()
		{
			UpdateFilter(CurrentCategory);
		}

		internal void UpdateFilter(string category)
		{
			this.dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == category;
		}


		void MyDocument_QuestionCategoryChanged(object sender, EventArgs e)
		{
			UpdateFilter(CurrentCategory);
		}


		/// <summary>
		/// カテゴリを追加します。
		/// </summary>
		/// <param name="category"></param>
		bool AddCategory(string category)
		{
			if (!Categories.Contains(category))
			{
				Categories.Add(category);
				//this.NotifyPropertyChanged("Categories");
				return true;
			}
			else
			{
				return false;
			}
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

		// (0.0.6.2)GrandMutusからコピペ．
		// (0.3.4.1)既定の動作をオーバーライドする．
		private void DeleteQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = ((System.Collections.IList)((DataGrid)sender).SelectedItems).Cast<SweetQuestion>();
			if (items != null)
			{
				this.MyDocument.RemoveQuestions(items);
			}
		}

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

		#region カテゴリ関連

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

		#endregion

		// (0.0.6.1)曲ファイルのドロップ処理を実装．
		#region ドラッグドロップ関連

		private void dataGridQuestions_DragOver(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = ((DataObject)e.Data).GetFileDropList();

				for (int i=0; i<files.Count; i++)
				{
					if (files[i].EndsWith(".mp3"))
					{
						e.Effects = DragDropEffects.Copy;
						return;
					}
				}
			}
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void dataGridQuestions_Drop(object sender, DragEventArgs e)
		{
			List<string> songFileNames = new List<string>();

			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				var files = ((DataObject)e.Data).GetFileDropList();

				for (int i = 0; i < files.Count; i++)
				{
					if (files[i].EndsWith(".mp3"))
					{
						songFileNames.Add(files[i]);
					}
				}
			}
			MyDocument.AddQuestions(songFileNames);
	
		}

		#endregion


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

				this.expanderSongPlayer.IsExpanded = true;	// ←これはここに書くべきものなのか？
			}
		}

		void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			//e.CanExecute = true;
			e.CanExecute = e.Parameter is SweetQuestion;
		}
		#endregion


		#region SwitchPlayPauseコマンド

		// 動作を2つ用意する．
		// 曲をパラメータとしてとり，現在の曲と違ったら，Playの動作をするようにする？

		void SwitchPlayPause_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			SweetQuestion q = e.Parameter as SweetQuestion;
			if (q != null && q != CurrentSong)
			{
				MediaCommands.Play.Execute(q, this);
			}
			else if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_songPlayer.TogglePlayPause();
			}
		}

		void SwitchPlayPause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (e.Parameter is SweetQuestion) || _songPlayer.CurrentState != SongPlayer.State.Inactive;
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

		// (0.0.8.2)
		#region SetPlayPosコマンド
		void SetPlayPos_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_currentSong.PlayPos = _songPlayer.CurrentPosition;
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
				var new_position = CurrentSong.SabiPos.Add(TimeSpan.FromSeconds(-0.1));
				CurrentSong.SabiPos = new_position > TimeSpan.Zero ? new_position : TimeSpan.Zero;
			}
		}
		private void UpDownControlPlayPos_UpClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				CurrentSong.PlayPos += TimeSpan.FromSeconds(0.1);
			}
		}

		private void UpDownControlPlayPos_DownClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				var new_position = CurrentSong.PlayPos.Add(TimeSpan.FromSeconds(-0.1));
				CurrentSong.PlayPos = new_position > TimeSpan.Zero ? new_position : TimeSpan.Zero;
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

/*		private void MenuItemFilter_Click(object sender, RoutedEventArgs e)
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
*/
		#endregion


	}






}
