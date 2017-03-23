using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldentea.SweetMutus.Base
{
	// (0.1.4)MemoColumnを追加。
	// (0.1.3.1)
	#region QuestionColumnsVisibility列挙体

	/// <summary>
	/// DataGridQuestionsでどの列を表示するかを指定するためのフラグ列挙体です．
	/// </summary>
	[Flags]
	public enum QuestionColumnsVisibility
	{
		/// <summary>
		/// 最低限の列のみを表示します．
		/// </summary>
		None = 0x0,
		/// <summary>
		/// 問題ID列を表示します．
		/// </summary>
		IdColumn = 0x1,
		/// <summary>
		/// ファイル名列を表示します．
		/// </summary>
		FileNameColumn = 0x2,
		/// <summary>
		/// 再生開始位置列を表示します．
		/// </summary>
		PlayPosColumn = 0x4,
		/// <summary>
		/// サビ位置列を表示します．
		/// </summary>
		SabiPosColumn = 0x8,
		/// <summary>
		/// 再生停止位置列を表示します．
		/// </summary>
		StopPosColumn = 0x10,
		/// <summary>
		/// メモ列を表示します．
		/// </summary>
		MemoColumn = 0x20,
		/// <summary>
		/// デフォルトの設定です(再生開始位置列とサビ開始位置列を表示)．
		/// </summary>
		Default = PlayPosColumn | SabiPosColumn
	}
	#endregion

}
