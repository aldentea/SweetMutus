using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GrandMutus.Data;

namespace Aldentea.SweetMutus.Data
{
	// とりあえずSweetMutusDocumentとは分けて実装してみる．

	// (0.3.0)
	#region SweetMutusGameDocumentクラス
	public class SweetMutusGameDocument : SweetMutusDocument
	{

		// (0.3.0)
		#region *Logsプロパティ
		LogsCollection Logs
		{
			get
			{
				return _logs;
			}
		}

		LogsCollection _logs = new LogsCollection();
		#endregion


		// (0.3.0)
		protected override void InitializeDocument()
		{
			base.InitializeDocument();
			Logs.Initialize();
		}

		#region ログ関連

		// (0.3.0)
		public void AddOrder(QuestionBase question)
		{
			Logs.AddOrder(question.ID);
		}

		// (0.3.0)
		public bool AddFirstOrder()
		{
			return Logs.AddFirstOrder();
		}

		// (0.3.0)
		public void AddLog(string code, decimal value)
		{
			Logs.AddLog(code, value);
		}

		#endregion


		#region XML入出力関連

		// (0.3.0)
		protected override XElement GenerateSweetMutusElement(string destination_directory, string exported_songs_root = null)
		{
			var sweet = base.GenerateSweetMutusElement(destination_directory, exported_songs_root);

			// ログを追加．
			sweet.Add(Logs.GenerateElement());

			return sweet;
		}

		// (0.3.0)
		protected override void LoadElements(XElement sweet, string fileDirectory)
		{
			base.LoadElements(sweet, fileDirectory);
			Logs.LoadElement(sweet.Element(LogsCollection.ELEMENT_NAME));
		}

		#endregion
	}
	#endregion

}
