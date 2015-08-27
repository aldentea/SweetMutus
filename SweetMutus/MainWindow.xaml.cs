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
			AddQuestions();
		}

		//private void AddQuestions_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		//{
		//	e.CanExecute = MyDocument.CanUndo;
		//}

		#endregion


		#endregion

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


	}





	// とりあえずここに置いておく．
	public static class Commands
	{
		public static RoutedCommand AddQuestionsCommand = new RoutedCommand();

		//public static RoutedCommand SetSabiPosCommand = new RoutedCommand();
	}

}
