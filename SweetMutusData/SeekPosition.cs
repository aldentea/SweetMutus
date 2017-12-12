using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Aldentea.SweetMutus.Data
{

	// (0.6.0)
	#region SeekPositionクラス
	public class SeekPosition : INotifyPropertyChanged, INotifyPropertyChanging
	{

		#region プロパティ

		// (0.6.0)
		#region *Positionプロパティ
		/// <summary>
		/// シーク先の(ファイル先頭からの絶対)位置を取得／設定します．
		/// 従来のものとは違い、TimeSpan型になっています。
		/// </summary>
		public TimeSpan Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (this.Position != value)
				{
					NotifyPropertyChanging("Position");
					this._position = value;
					NotifyPropertyChanged("Position");
				}
			}
		}
		TimeSpan _position = TimeSpan.Zero;
		#endregion
		
		// (0.6.0)
		#region *Commentプロパティ
		/// <summary>
		/// シーク位置に対するコメント取得／設定します．
		/// 取得時にはnullは返らないようになっています。
		/// </summary>
		public string Comment
		{
			get
			{
				return _comment ?? string.Empty;
			}
			set
			{
				string new_value = string.IsNullOrEmpty(value) ? string.Empty : value;
				if (Comment != new_value)
				{
					NotifyPropertyChanging("Comment");
					this._comment = new_value;
					NotifyPropertyChanged("Comment");
				}
			}
		}
		string _comment = string.Empty;
		#endregion
		
		#endregion


		#region XML入出力関連

		// (0.6.0) シーク位置設定を拡張。
		public const string ELEMENT_NAME = "seek";
		const string POSITION_ATTRIBUTE = "position";
		const string COMMENT_ATTRIBUTE = "comment";

		// (0.6.0)
		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement()
		{
			var element = new XElement(ELEMENT_NAME,
				new XAttribute(POSITION_ATTRIBUTE, this.Position),
				new XAttribute(COMMENT_ATTRIBUTE, this.Comment)
			);
			return element;
		}
		#endregion


		// (0.6.0)
		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static SeekPosition Generate(XElement seekElement)
		{
			var seek_pos = new SeekPosition();

			var position = (TimeSpan?)seekElement.Attribute(POSITION_ATTRIBUTE);
			if (position.HasValue)
			{
				seek_pos.Position = position.Value;
			}

			var comment = (string)seekElement.Attribute(COMMENT_ATTRIBUTE);
			if (!string.IsNullOrEmpty(comment))
			{
				seek_pos.Comment = comment;
			}

			return seek_pos;
		}
		#endregion


		#endregion


		#region INotifyPropertyChangedインターフェイス実装

		// (0.6.0)
		protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
		{
			this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		#endregion

		#region INotifyPropertyChangingインターフェイス実装

		// (0.6.0)
		protected void NotifyPropertyChanging([CallerMemberName]string propertyName = "")
		{
			this.PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}
		public event PropertyChangingEventHandler PropertyChanging = delegate { };

		#endregion

	}
	#endregion

}
