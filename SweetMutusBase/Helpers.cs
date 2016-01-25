using System.Collections.Generic;

namespace Aldentea.SweetMutus.Base
{

	// (0.1.3.1)SweetMutusBaseに移動．
	public static class Helpers
	{

		#region *[static]ファイルをエクスポート(ExportFiles)
		/// <summary>
		/// ファイルを指定した場所にコピーします．
		/// </summary>
		/// <param name="destination">コピー先のディレクトリ．</param>
		public static void ExportFiles(IEnumerable<string> files, string destination)
		{
			HyperMutus.Helpers.WorkBackground<string>(
				files,
				(fileName) =>
				{
					HyperMutus.Helpers.CopyFileTo(fileName, destination);
				}
			);
		}
		#endregion

	}
}
