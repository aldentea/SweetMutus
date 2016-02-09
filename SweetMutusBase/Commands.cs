using System.Windows.Input; // for RoutedCommand.

namespace Aldentea.SweetMutus.Base
{
	// (0.1.3.1)SweetMutusBaseに移動．

	public static class Commands
	{
		public static RoutedCommand SaveAsMtqCommand = new RoutedCommand();

		/// <summary>
		/// 曲ファイルを選択して問題を追加します．
		/// </summary>
		//public static RoutedCommand AddQuestionsCommand = new RoutedCommand();

		/// <summary>
		/// 出題曲をエクスポートします．
		/// </summary>
		public static RoutedCommand ExportCommand = new RoutedCommand();

		public static RoutedCommand SetPlayPosCommand = new RoutedCommand();

		// (0.1.3.1)
		public static RoutedCommand SetStopPosCommand = new RoutedCommand();

		/// <summary>
		/// 新たなカテゴリを追加します。
		/// </summary>
		public static RoutedCommand AddCategoryCommand = new RoutedCommand();

		//public static RoutedCommand SetSabiPosCommand = new RoutedCommand();

		public static RoutedCommand SaveSongInformationCommand = new RoutedCommand();

		public static RoutedCommand ChangeFileNameCommand = new RoutedCommand();

		/// <summary>
		/// 問題のカテゴリを変更します。パラメータには変更先のカテゴリ名をとります。
		/// </summary>
		public static RoutedCommand ChangeCategoryCommand = new RoutedCommand();

		public static RoutedCommand IncrementNoCommand = new RoutedCommand();
		public static RoutedCommand DecrementNoCommand = new RoutedCommand();
		public static RoutedCommand OmitQuestionsCommand = new RoutedCommand();
		public static RoutedCommand EnterQuestionsCommand = new RoutedCommand();

		// (0.0.11)
		/// <summary>
		/// 曲や問題を他のドキュメントからインポートします．
		/// </summary>
		public static RoutedCommand ImportCommand = new RoutedCommand();

		// (0.1.3.1)
		public static RoutedCommand SetWindowModeCommand = new RoutedCommand();

		// ※とりあえずここに置いておくが，QuizCommandsに入れた方がいいかな？
		public static RoutedCommand NextQuestionCommand = new RoutedCommand();
		public static RoutedCommand JudgeCommand = new RoutedCommand();	// 判定内容はパラメータで与える．


	}

}
