using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aldentea.SweetMutus.Data
{

	// (0.5.0)
	#region IExportQuestionsListParameterインターフェイス
	public interface IExportQuestionsListParameter
	{
		bool IDOutput { get; set; }
		bool CategoryOutput { get; set; }
		bool NoOutput { get; set; }
		bool TitleOutput { get; set; }
		bool ArtistOutput
		{
			get; set;
		}
		bool FileNameOutput
		{
			get; set;
		}
		bool SabiPosOutput
		{
			get; set;
		}
		bool PlayPosOutput
		{
			get; set;
		}
		bool StopPosOutput
		{
			get; set;
		}
		bool MemoOutput
		{
			get; set;
		}

	}
	#endregion

}
