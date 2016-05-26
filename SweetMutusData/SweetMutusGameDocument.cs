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

	// (0.3.1.2)IMutusGameDocumentを追加。
	// (0.3.0)
	#region SweetMutusGameDocumentクラス
	public class SweetMutusGameDocument : SweetMutusDocument, IMutusGameDocument
	{

		// (0.3.0)
		#region *Logsプロパティ
		public LogsCollection Logs
		{
			get
			{
				return _logs;
			}
		}

		LogsCollection _logs = new LogsCollection();
		#endregion


		public event EventHandler<OrderEventArgs> OrderAdded = delegate { };
		public event EventHandler<OrderEventArgs> OrderRemoved = delegate { };

		// (0.3.2)
		#region *IsRehearsalプロパティ
		/// <summary>
		/// リハーサルモードであるかどうかの値を取得／設定します。
		/// リハーサルモードの場合は、出題関連の操作についてダーティフラグが立ちません。
		/// </summary>
		public bool IsRehearsal
		{
			get
			{
				return this._isRehearsal;
			}
			set
			{
				if (this.IsRehearsal != value)
				{
					this._isRehearsal = value;
					NotifyPropertyChanged("IsRehearsal");
				}
			}
		}
		bool _isRehearsal = true;
		#endregion

		// (0.3.0)
		protected override void InitializeDocument()
		{
			base.InitializeDocument();
			Logs.Initialize();
		}


		#region ログ関連

		// (0.3.2)リハーサルモードを実装。
		// (0.3.0)
		public void AddOrder(int? questionID)
		{
			if (questionID.HasValue)
			{
				Logs.AddOrder(questionID.Value);
				if (!IsRehearsal)
				{
					AddOperationHistory(new AddOrderCache(this, questionID));
				}
			}
			else
			{
				Logs.AddFirstOrder();
			}
			this.OrderAdded(this, new OrderEventArgs(questionID));
		}

		// (0.3.0)
		//public bool AddFirstOrder()
		//{
		//	return Logs.AddFirstOrder();
		//}

		// (0.3.2)リハーサルモードを実装。
		// (0.3.0)
		public void RemoveOrder()
		{
			int? questionID = Logs.RemoveOrder();
			if (!IsRehearsal)
			{
				AddOperationHistory(new RemoveOrderCache(this, questionID));
			}
			this.OrderRemoved(this, new OrderEventArgs(null));
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
			if (Logs.Count > 0)
			{
				sweet.Add(Logs.GenerateElement());
			}
			return sweet;
		}

		// (0.3.0)
		protected override void LoadElements(XElement sweet, string fileDirectory)
		{
			base.LoadElements(sweet, fileDirectory);
			var logsElement = sweet.Element(LogsCollection.ELEMENT_NAME);
			if (logsElement != null)
			{
				Logs.LoadElement(logsElement);
			}
		}

		#endregion
	}
	#endregion

}
