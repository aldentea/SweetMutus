using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Aldentea.Wpf.Application;
using System.ComponentModel;
using HyperMutus;
using System.Collections.ObjectModel;

namespace Aldentea.SweetMutus
{
	using Data;
	using Base;

	#region MainWindowクラス
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : BasicWindow, INotifyPropertyChanged
	{

		#region プロパティ

		#region *MyDocumentプロパティ
		protected SweetMutusDocument MyDocument
		{
			get { return (SweetMutusDocument)App.Current.Document; }
		}
		#endregion

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

		// (0.1.3)
		#region モード関連

		#region *CurrentModeプロパティ
		/// <summary>
		/// 現在のモードを取得／設定します．
		/// </summary>
		public Mode CurrentMode
		{
			get
			{
				return _currentMode;
			}
			set
			{
				if (_currentMode != value)
				{
					this._currentMode = value;
					UpdateUI();
					NotifyPropertyChanged("CurrentMode");
				}
			}
		}
		Mode _currentMode = Mode.Edit;
		#endregion

		void UpdateUI()
		{
			dataGridQuestions.IsReadOnly = CurrentMode == Mode.Play;
		}

		public enum Mode
		{
			Edit,
			Play
		}

		#endregion



		#region *コンストラクタ(MainWindow)
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

			// レイアウト関係のコマンドハンドラだけど，なぜかここに記載．
			//dataGridQuestionsFileNameColumn.Visibility
		}
		#endregion

		// (0.1.3.1)[表示]系メニューの設定を追加．
		// (0.0.13)音声ボリュームを復元．
		// (0.0.8.9)
		#region *ウインドウ初期化時(MainWindow_Initialized)
		private void MainWindow_Initialized(object sender, EventArgs e)
		{
			// 窓の位置やサイズを復元。
			if (MySettings.WindowMaximized)
			{
				this.WindowState = System.Windows.WindowState.Maximized;
			}
			if (MySettings.WindowSize != new Size(0, 0))
			{
				this.Left = MySettings.WindowPosition.X;
				this.Top = MySettings.WindowPosition.Y;
				this.Width = MySettings.WindowSize.Width;
				this.Height = MySettings.WindowSize.Height;
			}
			this.SongPlayer.Volume = MySettings.SongPlayerVolume;

			// [表示]系メニューの設定．
			questionsIDColumn.Visibility
				= MySettings.DataGridColumnsVisibility.HasFlag(QuestionColumnsVisibility.IdColumn) ? Visibility.Visible : Visibility.Collapsed;
			questionsFileNameColumn.Visibility
				= MySettings.DataGridColumnsVisibility.HasFlag(QuestionColumnsVisibility.FileNameColumn) ? Visibility.Visible : Visibility.Collapsed;
			questionsPlayPosColumn.Visibility
				= MySettings.DataGridColumnsVisibility.HasFlag(QuestionColumnsVisibility.PlayPosColumn) ? Visibility.Visible : Visibility.Collapsed;
			questionsSabiPosColumn.Visibility
				= MySettings.DataGridColumnsVisibility.HasFlag(QuestionColumnsVisibility.SabiPosColumn) ? Visibility.Visible : Visibility.Collapsed;
			questionsStopPosColumn.Visibility
				= MySettings.DataGridColumnsVisibility.HasFlag(QuestionColumnsVisibility.StopPosColumn) ? Visibility.Visible : Visibility.Collapsed;
		}
		#endregion

		// (0.1.3.1)[表示]系メニューの保存を追加．
		// (0.0.13)音声ボリュームを保存．
		// (0.0.8.9)
		#region *ウインドウクローズ時(MainWindow_Closed)
		private void MainWindow_Closed(object sender, EventArgs e)
		{
			// 窓の位置やサイズを保存。
			MySettings.WindowMaximized = this.WindowState == System.Windows.WindowState.Maximized;
			MySettings.WindowPosition = new Point(this.Left, this.Top);
			MySettings.WindowSize = new System.Windows.Size(this.Width, this.Height);

			MySettings.SongPlayerVolume = this.SongPlayer.Volume;

			// (0.1.3.1)[表示]系メニューを保存．
			var flags = QuestionColumnsVisibility.None;
			flags |= menuItemIDColumnVisible.IsChecked ? QuestionColumnsVisibility.IdColumn : 0;
			flags |= menuItemFileNameColumnVisible.IsChecked ? QuestionColumnsVisibility.FileNameColumn : 0;
			flags |= menuItemPlayPosColumnVisible.IsChecked ? QuestionColumnsVisibility.PlayPosColumn : 0;
			flags |= menuItemSabiPosColumnVisible.IsChecked ? QuestionColumnsVisibility.SabiPosColumn : 0;
			flags |= menuItemStopPosColumnVisible.IsChecked ? QuestionColumnsVisibility.StopPosColumn : 0;
			MySettings.DataGridColumnsVisibility = flags;
		}
		#endregion


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

		// (0.1.0.1)カテゴリ追加後の後片付けをここに実装．
		#region AddCategory

		private void AddCategory_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string)
			{
				this.AddCategory((string)e.Parameter);
			}

			// 後片付け...はここに書くべきなのか？
			if (e.Source == buttonAddCategory)
			{
				textBoxNewCategory.Clear();
				expanderNewCategory.IsExpanded = false;
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

		// (0.0.11)
		#region Import

		private void Import_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ImportFrom(string.Empty);
		}

		#endregion


		#endregion

		// (0.0.8.9)
		#region *MySettingsプロパティ
		Properties.Settings MySettings
		{
			get
			{
				return App.Current.MySettings;
			}
		}
		#endregion

		// カレントカテゴリ関連

		#region *ドキュメント初期化時(MyDocument_Initialized)
		void MyDocument_Initialized(object sender, EventArgs e)
		{

			Categories.Clear();
			AddCategory(string.Empty);
			this.CurrentCategory = string.Empty;

			this.SongPlayer.Close();
			this.CurrentSong = null;
			this.expanderSongPlayer.IsExpanded = false;
		}
		#endregion

		#region *ドキュメントオープン時(MyDocument_Opened)
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
		#endregion

		#region *[dependency]CurrentCategoryプロパティ

		/// <summary>
		/// 現在のカテゴリを取得／設定します。
		/// </summary>
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

		#endregion

		// (0.0.8)
		internal void UpdateFilter()
		{
			UpdateFilter(CurrentCategory);
		}

		internal void UpdateFilter(string category)
		{
			this.dataGridQuestions.Items.Filter = q => ((SweetQuestion)q).Category == category;
		}

		#region *問題のカテゴリ変更時(MyDocument_QuestionCategoryChanged)
		void MyDocument_QuestionCategoryChanged(object sender, EventArgs e)
		{
			UpdateFilter(CurrentCategory);
		}
		#endregion

		#region *カテゴリを追加(AddCategory)
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
		#endregion

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

		// (0.1.0)選択しないままOKするとアプリケーションが落ちるバグに対応。
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
				if (!string.IsNullOrEmpty(directory))
				{
					AddQuestions(System.IO.Directory.GetFiles(directory, "*.mp3", System.IO.SearchOption.AllDirectories));
					if (string.IsNullOrEmpty(MyDocument.Questions.RootDirectory))
					{
						MyDocument.Questions.RootDirectory = directory;
					}
				}
			}
		}

		// (0.1.0.3)CurrentCategoryを設定するように改良。
		#region 問題を追加(AddQuestions)
		public void AddQuestions(IEnumerable<string> fileNames)
		{
			this.MyDocument.AddQuestions(fileNames, CurrentCategory);
		}

		IList<SweetQuestion> AddQuestionsParallel(IEnumerable<string> fileNames)
		{
			List<SweetQuestion> added_questions = new List<SweetQuestion>();

			Action<string> action = (fileName) =>
			{
				// ObservableCollectionに対する操作は，それが作られたスレッドと同じスレッドで行う必要がある．

				var question = this.Dispatcher.Invoke(
					new Func<string, SweetQuestion>(delegate(string f) { return MyDocument.AddQuestion(f, CurrentCategory); }), fileName);
				if (question is SweetQuestion)
				{
					added_questions.Add((SweetQuestion)question);
				}
					
			};
			HyperMutus.Helpers.WorkBackgroundParallel<string>(fileNames, action);
			return added_questions;
		}
		#endregion


		#region DeleteQuestions

		// (0.0.6.2)GrandMutusからコピペ．
		// (*0.3.4.1)DataGridの既定の動作をオーバーライドする．
		private void DeleteQuestions_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			var items = ((System.Collections.IList)((DataGrid)sender).SelectedItems).Cast<SweetQuestion>();
			if (items != null)
			{
				this.MyDocument.RemoveQuestions(items);
			}
		}

		#endregion

		#endregion

		// (0.0.12)smtファイルにも対応．
		// (0.0.11)
		#region インポート

		public void ImportFrom(string sourceFileName)
		{
			if (string.IsNullOrEmpty(sourceFileName))
			{
				sourceFileName = SelectImportSource();
				if (string.IsNullOrEmpty(sourceFileName))
				{
					return;
				}
			}

			var dialog = new ImportDialog();
			dialog.CommandBindings.Add(new CommandBinding(Commands.ImportCommand, DialogImport_Executed, DialogImport_CanExecute));
			switch (System.IO.Path.GetExtension(sourceFileName))
			{
				case ".mtu":
				case ".mtq":
					GrandMutus.Data.MutusDocument doc = new GrandMutus.Data.MutusDocument();
					doc.Open(sourceFileName, true);
					dialog.DataContext = doc;
					break;
				case ".smt":
					SweetMutusDocument s_doc = new SweetMutusDocument();
					s_doc.Open(sourceFileName, true);
					dialog.DataContext = s_doc;
					break;
				default:
					MessageBox.Show("未対応のファイルですorz");
					return;
			}
			dialog.ShowDialog();
		}

		#region *インポート元のファイルを選択(SelectImportSource)
		string SelectImportSource()
		{
			var file_dialog = new Microsoft.Win32.OpenFileDialog
			{
				Filter = "SweetMutusファイル(*.smt)|*.smt|HyperMutusファイル(*.mtu,*.mtq)|*.mtu;*.mtq",
				ReadOnlyChecked = true,
				Title = "インポート元ファイルを選択して下さい"
			};
			if (file_dialog.ShowDialog() == true)
			{
				return file_dialog.FileName;
			}
			else
			{
				return string.Empty;
			}
		}
		#endregion

		// (0.1.1？)CurrentCategoryを反映。
		void DialogImport_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is System.Collections.IList)
			{
				var songs = ((System.Collections.IList)e.Parameter).Cast<GrandMutus.Data.ISong>();
				MyDocument.ImportSongs(songs, CurrentCategory);
			}
		}

		void DialogImport_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Parameter is System.Collections.IList)
			{
				e.CanExecute = ((System.Collections.IList)e.Parameter).Count > 0;
			}
			else
			{
				e.CanExecute = false;
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

		// (0.0.13)SweetMutus形式に加えて，HyperMutus形式でエクスポートできるように改良．
		#region *エクスポート(Export)
		void Export()
		{
			// ファイル名選択
			var dialog = new HyperMutus.ExportDialog { FileFilter = "SweetMutusファイル(*.smt)|*.smt|HyperMutusファイル(*.mtq)|*.mtq" };
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
		#endregion

		#region *ルートディレクトリを設定(SetRootDirectory)
		private void SetRootDirectory()
		{
			var directory = HyperMutus.Helpers.SelectSongsRoot(this.MyDocument.Questions.RootDirectory);
			if (directory != null)
			{
				MyDocument.Questions.RootDirectory = directory;
			}
		}
		#endregion

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

		// (0.1.0.3)CurrentCategoryに追加するように改良。
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
			MyDocument.AddQuestions(songFileNames, CurrentCategory);
	
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


		// (*0.3.2)プロパティ化。
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

		// (*0.3.2)
		#region *曲ファイルオープン時(SongPlayer_MediaOpened)
		void SongPlayer_MediaOpened(object sender, EventArgs e)
		{
			if (_songPlayer.Duration.HasValue)
			{
				this.labelDuration.Content = _songPlayer.Duration.Value;
				this.sliderSeekSong.Maximum = _songPlayer.Duration.Value.TotalSeconds;
			}
		}
		#endregion

		//AutoResetEvent _mediaOpenedEvent = new AutoResetEvent(false);

		// (0.1.0)再生開始位置から再生するように修正。
		#region Playコマンド
		void Play_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is SweetQuestion)
			{
				var song = (SweetQuestion)e.Parameter;
				_songPlayer.Open(song.FileName);

				// このまま次の処理に進むと、まだファイルが開いていないうちにCurrentPositionを設定しまうことがある！
				// ＝ここでの設定が反映されない！
				// 同期をとればよい？が、そのやり方がよくわからないので適当にDelayを挟む。
				Task.Delay(10);

				this.CurrentSong = song;
				_songPlayer.CurrentPosition = song.PlayPos;
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

		// (0.1.0)現在の場所が再生開始位置より前であれば、(現在の曲の)再生開始位置にSeekするように修正。
		// (0.0.8.7)
		#region NextTrack
		void NextTrack_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentPosition < this.CurrentSong.PlayPos)
			{
				// 再生開始位置に移動。
				_songPlayer.CurrentPosition = this.CurrentSong.PlayPos;
			}
			else
			{
				// 次の曲を再生(しようとする)。
				var index = dataGridQuestions.Items.IndexOf(this.CurrentSong);
				if (index >= 0 && index < dataGridQuestions.Items.Count - 1)
				{
					var song = (SweetQuestion)dataGridQuestions.Items.GetItemAt(index + 1);
					System.Windows.Input.MediaCommands.Play.Execute(song, this);
					dataGridQuestions.SelectedItem = song;
				}
			}
		}
		#endregion

		// (0.0.8.7)
		#region PreviousTrack
		void PreviousTrack_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentPosition > CurrentSong.PlayPos + TimeSpan.FromSeconds(1.5))
			{
				// 再生開始位置へ．
				_songPlayer.CurrentPosition = CurrentSong.PlayPos;
			}
			else if (_songPlayer.CurrentPosition > TimeSpan.FromSeconds(1))
			{
				// 曲冒頭へ．
				_songPlayer.CurrentPosition = TimeSpan.Zero;
			}
			else
			{
				var index = dataGridQuestions.Items.IndexOf(this.CurrentSong);
				if (index >= 1 && index < dataGridQuestions.Items.Count)
				{
					var song = (SweetQuestion)dataGridQuestions.Items.GetItemAt(index - 1);
					System.Windows.Input.MediaCommands.Play.Execute(song, this);
					dataGridQuestions.SelectedItem = song;
				}
			}
		}

		//void PreviousTrack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		//{

		//}
		#endregion

		#region SwitchPlayPauseコマンド

		// 動作を2つ用意する．
		// 曲をパラメータとしてとり，現在の曲と違ったら，Playの動作をするようにする？
		// ↑してみました．

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

		#region SeekRelative
		void SeekRelative_Executed(object sender, ExecutedRoutedEventArgs e)
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

		#endregion

		#region SeekSabi

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

		#endregion

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

		// (0.1.3.1)
		#region SetStopPosコマンド
		void SetStopPos_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (_songPlayer.CurrentState != SongPlayer.State.Inactive)
			{
				_currentSong.StopPos = _songPlayer.CurrentPosition;
			}
		}
		#endregion

		#region UpDownControlコマンドハンドラ

		// このあたりはなんとかならないのかしら？

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

		// (0.1.3.1)
		private void UpDownControlStopPos_UpClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				CurrentSong.StopPos += TimeSpan.FromSeconds(0.1);
			}
		}

		// (0.1.3.1)
		private void UpDownControlStopPos_DownClick(object sender, RoutedEventArgs e)
		{
			if (CurrentSong != null)
			{
				var new_position = CurrentSong.StopPos.Add(TimeSpan.FromSeconds(-0.1));
				CurrentSong.StopPos = new_position > TimeSpan.Zero ? new_position : TimeSpan.Zero;
			}
		}

		#endregion

		#endregion

		//	#endregion

		#region INotifyPropertyChanged実装

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void NotifyPropertyChanged(string propertyName)
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
	#endregion

}
