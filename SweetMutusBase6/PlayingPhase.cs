using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldentea.SweetMutus.Net6.Base
{
	public enum PlayingPhase
	{
		/// <summary>
		/// 出題準備ができて，出題を待っている状態．
		/// </summary>
		Ready,
		/// <summary>
		/// 出題中．
		/// </summary>
		Playing,
		/// <summary>
		/// 解答中．
		/// </summary>
		Thinking,
		/// <summary>
		/// 判定後(得点処理前)．
		/// </summary>
		Judged,
		/// <summary>
		/// 出題後．
		/// </summary>
		Talking
	}

}
