using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

using GrandMutus.Data;
using System.Xml.Linq;

namespace Aldentea.SweetMutus.Data
{
	public class SweetQuestionsCollection : ObservableCollection<SweetQuestion>
	{

		#region QuestionsCollectionのコピペ

		#region *コンストラクタ(SweetQuestionsCollection)
		SweetQuestionsCollection()
		{
			this.CollectionChanged += QuestionsCollection_CollectionChanged;
			Initialize();
		}

		public SweetQuestionsCollection(SweetMutusDocument document) : this()
		{
			this._document = document;
		}
		#endregion



		#region Documentとの関係

		public SweetMutusDocument Document { get { return _document; } }
		readonly SweetMutusDocument _document;

		#endregion


		#region コレクション変更関連

		// (0.4.1)
		/// <summary>
		/// 問題が削除された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IEnumerable<SweetQuestion>>> QuestionsRemoved = delegate { };


		// (0.4.1) Remove時の処理を追加(ほとんどSongsCollectionのコピペ)．
		private void QuestionsCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					foreach (var item in e.NewItems)
					{
						var question = (SweetQuestion)item;

						// IDを付与する．
						// (0.1.2)IDが既に設定されているかどうかを確認．
						if (question.ID <= 0)	// 無効な値．
						{
							question.ID = GenerateNewID();
						}
						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						//question.PropertyChanging += Question_PropertyChanging;
						//question.PropertyChanged += Question_PropertyChanged;
						question.OnAddedTo(this);

						// ☆songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
						// ↑とりあえずこのクラスで使っています。
						question.PropertyChanging += Question_PropertyChanging;
						question.PropertyChanged += Question_PropertyChanged;
					}
				break;

				case NotifyCollectionChangedAction.Remove:
					IList<SweetQuestion> questions = new List<SweetQuestion>();
					foreach (var question in e.OldItems.Cast<SweetQuestion>())
					{
						// Questionでは、変更通知機能がまだ(ちょっと↑で)実装されていない．
						// 削除にあたって、変更通知機能を抑止。
						question.PropertyChanging -= Question_PropertyChanging;
						question.PropertyChanged -= Question_PropertyChanged;

						questions.Add(question);
					}
					// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
					// ここでOperationCacheの処理を行うことにした．
					if (questions.Count > 0)
					{
						this.QuestionsRemoved(this, new ItemEventArgs<IEnumerable<SweetQuestion>> { Item = questions });
					}
					break;
			}
		}
		#endregion

		// (0.3.3)SongsCollectionからのコピペ。共通実装にしますか？
		#region ID管理関連

		int GenerateNewID()
		{
			int new_id = this.UsedIDList.Any(n => n > 0) ? this.UsedIDList.Max() + 1 : 1;
			// ↑Max()は，要素が空ならInvalidOperationExceptionをスローする．

			//UsedIDList.Add(new_id);
			return new_id;
		}

		IEnumerable<int> UsedIDList
		{
			get
			{
				return Items.Select(question => question.ID);
			}
		}

		#endregion

		#endregion

		#region SongsCollectionのコピペ

		/// <summary>
		/// 曲ファイルを格納するディレクトリのフルパスを取得／設定します．
		/// </summary>
		public string RootDirectory { get; set; }

		#region アイテム変更関連

		/// <summary>
		/// 格納されているアイテムのプロパティ値が変化したときに発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IOperationCache>> ItemChanged = delegate { };


		string _titleCache = string.Empty;	// 手抜き．Songオブジェクト自体もキャッシュするべき．
		string _artistCache = string.Empty;
		TimeSpan _sabiPosCache = TimeSpan.Zero;

		void Question_PropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			Song song = (Song)sender;
			switch (e.PropertyName)
			{
				case "Title":
					this._titleCache = song.Title;
					break;
				case "Artist":
					this._artistCache = song.Artist;
					break;
				case "SabiPos":
					this._sabiPosCache = song.SabiPos;
					break;
			}
		}

		void Question_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var song = (Song)sender;

			switch (e.PropertyName)
			{
				case "Title":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongTitleChangedCache(song, _titleCache, song.Title)
					});
					_titleCache = string.Empty;
					break;
				case "Artist":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongArtistChangedCache(song, _artistCache, song.Artist)
					});
					_artistCache = string.Empty;
					break;
				case "SabiPos":
					this.ItemChanged(this, new ItemEventArgs<IOperationCache>
					{
						Item = new SongSabiPosChangedCache(song, _sabiPosCache, song.SabiPos)
					});
					_sabiPosCache = TimeSpan.Zero;
					break;
			}
			// ドキュメントにNotifyしたい！？
			//e.PropertyName
		}

		#endregion


		/// <summary>
		/// 初期化します．
		/// </summary>
		public void Initialize()
		{
			this.Clear();
			this.RootDirectory = string.Empty;

		}

		#endregion


		#region XML入出力関連

		public const string ELEMENT_NAME = "questions";
		const string PATH_ATTRIBUTE = "path";

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public XElement GenerateElement(string songs_root = null)
		{
			XElement element = new XElement(ELEMENT_NAME);

			foreach (var question in this.Items)
			{
				element.Add(question.GenerateElement(songs_root));
			}

			return element;
		}


		// (0.3.3)とりあえずIntroQuestionのみ。
		public void LoadElement(XElement questionsElement)
		{
			foreach (var question_element in questionsElement.Elements())
			{
				// ☆ここの処理は動的に分岐を生成するようにしたい！
				switch (question_element.Name.LocalName)
				{
					case "question":
						this.Add(SweetQuestion.Generate(question_element));
						break;
					default:
						// ※知らない要素が出てきたらどうしますか？
						break;
				}
			}
		}

		#endregion

	}
}
