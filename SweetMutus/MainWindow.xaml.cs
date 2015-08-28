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

namespace Aldentea.SweetMutus
{
	using Data;

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : BasicWindow
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

			dataGridQuestions.Items.SortDescriptions.Add(
				new SortDescription { Direction = ListSortDirection.Ascending, PropertyName = "No" }
			);

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

		#endregion


		// カレントカテゴリ関連

		void MyDocument_Initialized(object sender, EventArgs e)
		{
			this.comboBoxCategories.Items.Clear();
			this.comboBoxCategories.Items.Add(string.Empty);
		}

		void MyDocument_Opened(object sender, EventArgs e)
		{
			this.comboBoxCategories.Items.Clear();
			foreach (var category in MyDocument.Questions.Categories)
			{
				this.comboBoxCategories.Items.Add(category);
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


		#region お試し

		private void MenuItemFilter_Click(object sender, RoutedEventArgs e)
		{
			dataGridQuestions.Items.Filter = (q) => { return ((SweetQuestion)q).Category == "tanuki"; };
			dataGridQuestions.Items.SortDescriptions.Add(
				new SortDescription { Direction = ListSortDirection.Ascending, PropertyName = "No" }
			);
		}

		#endregion
	}





	// とりあえずここに置いておく．
	public static class Commands
	{
		/// <summary>
		/// 曲ファイルを選択して問題を追加します．
		/// </summary>
		public static RoutedCommand AddQuestionsCommand = new RoutedCommand();

		/// <summary>
		/// 出題曲をエクスポートします．
		/// </summary>
		public static RoutedCommand ExportCommand = new RoutedCommand();

		//public static RoutedCommand SetSabiPosCommand = new RoutedCommand();
	}

}
