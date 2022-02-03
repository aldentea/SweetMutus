using System;
using System.Collections.Generic;
using System.Linq;

using System.Collections.ObjectModel;
using System.Collections.Specialized;

using GrandMutus.Net6.Data;
using System.Xml.Linq;
using System.IO;
using Aldentea.Wpf.Document;

namespace Aldentea.SweetMutus.Net6.Data
{
	// (0.4.0)ISongsCollectionインターフェイスを追加。
	public class SweetQuestionsCollection : ObservableCollection<SweetQuestion>, ISongsCollection
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

		// ☆これ使ってるの？
		public SweetMutusDocument Document { get { return _document; } }
		readonly SweetMutusDocument _document;

		#endregion


		#region コレクション変更関連

		// (*0.4.1)
		/// <summary>
		/// 問題が削除された時に発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IEnumerable<SweetQuestion>>> QuestionsRemoved = delegate { };


		// (*0.4.1) Remove時の処理を追加(ほとんどSongsCollectionのコピペ)．
		#region *コレクション変更時(QuestionsCollection_CollectionChanged)
		private void QuestionsCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (e.NewItems != null)
					{
						foreach (var item in e.NewItems)
						{
							var question = (SweetQuestion)item;

							// IDを付与する．
							// (0.1.2)IDが既に設定されているかどうかを確認．
							if (question.ID <= 0) // 無効な値．
							{
								question.ID = GenerateNewID();
							}
							// ※songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
							//question.PropertyChanging += Question_PropertyChanging;
							//question.PropertyChanged += Question_PropertyChanged;
							question.NoChanged += Question_NoChanged;
							question.OnAddedTo(this);

							// ※songのプロパティ変更をここで受け取る？MutusDocumentで行えばここでは不要？
							// ↑とりあえずこのクラスで使っています。
							question.PropertyChanging += Question_PropertyChanging;
							question.PropertyChanged += Question_PropertyChanged;
						}
					}
				break;

				// (0.2.1.1)削除時にいったんNoをnullにするように修正．
				case NotifyCollectionChangedAction.Remove:
					if (e.OldItems != null)
					{
						IList<SweetQuestion> questions = new List<SweetQuestion>();
						for (int i = 0; i < e.OldItems.Count; i++)
						{
							var item = e.OldItems[i];
							if (item != null)
							{
								var question = (SweetQuestion)item;

								question.No = null;
								// 削除にあたって、変更通知機能を抑止。
								question.PropertyChanging -= Question_PropertyChanging;
								question.PropertyChanged -= Question_PropertyChanged;

								question.NoChanged -= Question_NoChanged;

								questions.Add(question);
							}

							// (MutusDocumentを経由せずに)UIから削除される場合もあるので，
							// ここでOperationCacheの処理を行うことにした．
							if (questions.Count > 0)
							{
								this.QuestionsRemoved(this, new ItemEventArgs<IEnumerable<SweetQuestion>> { Item = questions });
							}
						}
					}
					break;
			}

		}
		#endregion

		#endregion

		// (*0.3.3)SongsCollectionからのコピペ。共通実装にしますか？
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

		#region RootDirectory関連

		// (*0.4.4)set時に操作履歴に追加。
		// (*0.4.3)実装を変更し、この変更を各Songに通知するように修正。
		#region *RootDirectoryプロパティ
		/// <summary>
		/// 曲ファイルを格納するディレクトリのフルパスを取得／設定します．
		/// </summary>
		public string RootDirectory
		{
			get
			{
				return this._rootDirectory;
			}
			set
			{
				if (this._rootDirectory != value)
				{
					var previous_value = this._rootDirectory;
					this._rootDirectory = value;
					UpdateRelativeFileNames();
					this.RootDirectoryChanged(this, new ValueChangedEventArgs<string>(previous_value, value));
				}
			}
		}
		string _rootDirectory = string.Empty;

		// (0.4.3)
		void UpdateRelativeFileNames()
		{
			foreach (SweetQuestion question in this.Items)
			{
				question.UpdateRelativeFileName();
			}
		}
		#endregion

		// (0.4.4)
		/// <summary>
		/// RootDirecotyプロパティの値が変更されたときに発生します。
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<string>> RootDirectoryChanged = delegate { };

		#endregion

		#region アイテム変更関連

		/// <summary>
		/// 格納されているアイテムのプロパティ値が変化したときに発生します．
		/// </summary>
		public event EventHandler<ItemEventArgs<IOperationCache>> ItemChanged = delegate { };


		string _titleCache = string.Empty;	// 手抜き．Songオブジェクト自体もキャッシュするべき．
		string _artistCache = string.Empty;
		TimeSpan _playPosCache = TimeSpan.Zero;
		TimeSpan _sabiPosCache = TimeSpan.Zero;
		TimeSpan _stopPosCache = TimeSpan.Zero; // (0.3.1)
		string _fileNameCache = string.Empty;
		string _categoryCache = String.Empty;
		string _memoCache = string.Empty;	// (0.4.3)

		// (0.4.3)Memoに関する処理を追加。
		// (0.3.1)StopPosに関する処理を追加．
		#region *Questionのプロパティ変更前(Question_PropertyChanging)
		void Question_PropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs e)
		{
			if (sender is SweetQuestion)
			{
				SweetQuestion song = (SweetQuestion)sender;
				switch (e.PropertyName)
				{
					case "Title":
						this._titleCache = song.Title;
						break;
					case "Artist":
						this._artistCache = song.Artist;
						break;
					case "PlayPos":
						this._playPosCache = song.PlayPos;
						break;
					case "SabiPos":
						this._sabiPosCache = song.SabiPos;
						break;
					case "StopPos":
						this._stopPosCache = song.StopPos;
						break;
					case "FileName":
						this._fileNameCache = song.FileName;
						break;
					case "Category":
						this._categoryCache = song.Category;
						// Noは？
						break;
					case "Memo":
						this._memoCache = song.Memo;
						break;
				}
			}
		}
		#endregion

		// (0.4.3)Memoに関する処理を追加。
		// (0.3.1)StopPosに関する処理を追加．
		#region *Questionのプロパティ変更後(Question_PropertyChanged)
		void Question_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender is SweetQuestion)
			{
				var song = (SweetQuestion)sender;

				switch (e.PropertyName)
				{
					case "Title":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionTitleChangedCache(song, _titleCache, song.Title)
						});
						_titleCache = string.Empty;
						break;
					case "Artist":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionArtistChangedCache(song, _artistCache, song.Artist)
						});
						_artistCache = string.Empty;
						break;
					case "PlayPos":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionPlayPosChangedCache(song, _playPosCache, song.PlayPos)
						});
						_playPosCache = TimeSpan.Zero;
						break;
					case "SabiPos":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionSabiPosChangedCache(song, _sabiPosCache, song.SabiPos)
						});
						_sabiPosCache = TimeSpan.Zero;
						break;
					case "StopPos":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionStopPosChangedCache(song, _stopPosCache, song.StopPos)
						});
						_stopPosCache = TimeSpan.Zero;
						break;
					case "FileName":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionFileNameChangedCache(song, _fileNameCache, song.FileName)
						});
						_fileNameCache = string.Empty;
						break;
					case "Category":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionCategoryChangedCache(song, _categoryCache, song.Category)
						});
						_categoryCache = String.Empty;
						break;
					case "Memo":
						this.ItemChanged(this, new ItemEventArgs<IOperationCache>
						{
							Item = new QuestionMemoChangedCache(song, _memoCache, song.Memo)
						});
						_memoCache = String.Empty;
						break;
					case "No":
						// 整番処理は複雑なのでここでは行わない(Question_NoChangedで行う)。
						break;
				}
				// ドキュメントにNotifyしたい！？
				//e.PropertyName
			}
		}
		#endregion

		// (*0.4.6.0)QuestionNoChangeCompletedイベントの発生を追加．
		// (*0.4.5.2)で、カテゴリを考慮。
		// (*0.4.5.1)まずはカテゴリを考慮しない形で整番処理を記述．
		#region 整番処理関連

		bool _noChangingFlag = false;

		void Question_NoChanged(object? sender, ValueChangedEventArgs<int?> e)
		{
			if (_noChangingFlag)
			{
				return;
			}
			_noChangingFlag = true;

			if (sender is SweetQuestion)
			{
				try
				{
					var self = (SweetQuestion)sender;
					int? old_no = e.PreviousValue;
					int? new_no = e.CurrentValue;
					if (new_no.HasValue)
					{
						int n = Items.Count(q => { return q.Category == self.Category && q.No.HasValue; });
						if (new_no.Value > n)
						{
							self.No = new_no = n;
						}
					}

					// このあたりで操作履歴に加えておく。
					// (ここまでの処理で変更になる可能性があるので、Questionから直接MutusDocumentに通知することはしない。)
					this.QuestionNoChanged(self, new ValueChangedEventArgs<int?>(old_no, new_no));

					if (old_no.HasValue)
					{
						if (new_no.HasValue)
						{
							// M -> N
							int m = old_no.Value;
							int n = new_no.Value;

							if (m < n)
							{
								// M < N
								// (M+1)からNを1つずつ減らす。
								foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No > m && q.No <= n && q != self; }))
								{
									question.No -= 1;
								}
							}
							else
							{
								// M > N
								// Nから(M-1)を1つずつ増やす。
								foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No < m && q.No >= n && q != self; }))
								{
									question.No += 1;
								}
							}
						}
						else
						{
							// N -> null
							// Nより大きい番号を1ずつ減らす．
							int n = old_no.Value;

							foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No > n; }))
							{
								question.No -= 1;
							}
						}
					}
					else
					{
						// new_noはnullではないはずだが、一応チェックする。
						if (new_no.HasValue)
						{
							int n = new_no.Value;

							// null -> N
							// N以上の番号を1つずつ増やす。
							foreach (var question in Items.Where(q => { return q.Category == self.Category && q.No.HasValue && q.No >= n && q != self; }))
							{
								question.No += 1;
							}
						}

					}

					this.QuestionNoChangeCompleted(self, new ValueChangedEventArgs<int?>(old_no, new_no));

				}
				finally
				{
					_noChangingFlag = false;
				}
			}
		}

		// (0.4.5.1)
		/// <summary>
		/// 問題の番号が変更になったときに発生します。(操作履歴管理用かな？)
		/// senderはQuestionsCollectionではなくQuestionであることに一応注意。
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<int?>> QuestionNoChanged = delegate { };

		// 新しいNoの決定→変更→QuestionNoChanged→他の問題のNoの処理→QuestionNoChangeCompletedの順．

		// (0.4.6.0)
		/// <summary>
		/// 問題番号の変更処理が完了したときに発生します（他の問題の番号スライド処理の完了後）。
		/// senderはQuestionsCollectionではなくQuestionであることに一応注意。
		/// </summary>
		public event EventHandler<ValueChangedEventArgs<int?>> QuestionNoChangeCompleted = delegate { };

		#endregion

		#endregion


		#region *初期化(Initialize)
		/// <summary>
		/// 初期化します．
		/// </summary>
		public void Initialize()
		{
			this.Clear();
			this.RootDirectory = string.Empty;
		}
		#endregion

		#endregion

		#region *Categoriesプロパティ
		/// <summary>
		/// 現在のドキュメントで使用しているカテゴリを取得します．
		/// </summary>
		public IEnumerable<string> Categories
		{
			get
			{
				return this.Select(q => q.Category).Distinct();
			}
		}
		#endregion

		// (0.3.1.1)
		/// <summary>
		/// 指定したIDがついた問題を返します。
		/// 該当する問題がない場合は、InvalidOperationExceptionをスローします。
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public SweetQuestion Get(int id)
		{
			return this.Items.Single(q => q.ID == id);
		}

		#region XML入出力関連

		public const string ELEMENT_NAME = "questions";
		const string PATH_ATTRIBUTE = "path";

		// (0.0.1)エクスポートの場合に対応したつもりです．
		#region *XMLを生成(GenerateXML)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination_directory">出力されるXMLファイルのディレクトリのフルパスを与えます．</param>
		/// <param name="exported_songs_root">エクスポートするときは，その曲を格納するディレクトリの名前を与えます．
		/// そうでなければnullを与えます．</param>
		/// <returns></returns>
		public XElement GenerateElement(string destination_directory, string export_songs_root = "")
		{

			// path属性について，以下の3つのパターンがある．
			// 1. 絶対パスを記述．
			// 2. songs_root(というかファイルを保存するディレクトリ)からの相対パスを記述．(songs_rootと等しいときは，path="."とする！)
			// 3. 記述なし．

			bool exporting = !string.IsNullOrEmpty(export_songs_root);
			var songs_root = exporting ? export_songs_root : this.RootDirectory;

			XElement element = AddRootDirectoryProperty(new XElement(ELEMENT_NAME), destination_directory, songs_root);

			foreach (var question in this.Items)
			{
				element.Add(question.GenerateElement(songs_root, exporting));
			}

			return element;
		}
		#endregion

		#region HyperMutus用ドキュメント出力関係

		// (0.1.1)
		public XElement GenerateSongsElement(string destination_directory, string export_songs_root = "")
		{
			bool exporting = !string.IsNullOrEmpty(export_songs_root);
			var songs_root = exporting ? export_songs_root : this.RootDirectory;

			XElement element = AddRootDirectoryProperty(new XElement("songs"), destination_directory, songs_root);
			foreach (var question in this.Items)
			{
				element.Add(question.GenerateSongElement(songs_root, exporting));
			}
			return element;
		}

		// (0.1.1)
		public XElement GenerateQuestionsElement()
		{
			var element = new XElement("questions");
			foreach (var question in this.Items)
			{
				element.Add(question.GenerateQuestionElement());
			}
			return element;
		}

		#endregion

		// (0.1.1)GenerateXMLから分離。
		#region *RootDirectoryPropertyを追加(AddRootDirectoryProperty)
		XElement AddRootDirectoryProperty(XElement element, string destination_directory, string songs_root)
		{
			if (songs_root.Contains(destination_directory))
			{
				// (0.1.3.2)TrimEndを追加．
				if (songs_root.TrimEnd('\\') == destination_directory)
				{
					// ↓記述することにした．
					// 記述なし．
					element.Add(new XAttribute(PATH_ATTRIBUTE, "."));
				}
				else
				{
					// 相対パスを記述．
					element.Add(new XAttribute(PATH_ATTRIBUTE, songs_root.Substring(destination_directory.Length).Trim('\\')));
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(songs_root))
				{
					// 絶対パスを記述．
					element.Add(new XAttribute(PATH_ATTRIBUTE, songs_root));
				}
			}
			return element;
		}
		#endregion

		#region *questionsElementを読み込む(LoadElement)
		public void LoadElement(XElement questionsElement, string source_directory)
		{
			var path = (string?)questionsElement.Attribute(PATH_ATTRIBUTE) ?? String.Empty;
			string songs_root;
			if (string.IsNullOrEmpty(path))
			{
				// RootDirectoryプロパティは設定されない．
				songs_root = source_directory;	// ロードするファイルのあるディレクトリが入っているはずである．
			}
			else
			{
				// 何らかの形でRootDirectoryプロパティが設定される．
				if (Path.IsPathRooted(path))
				{
					this.RootDirectory = path;
				}
				else if (path == ".")
				{
					this.RootDirectory = source_directory;
				}
				else
				{
					this.RootDirectory = Path.Combine(source_directory, path);
				}
				songs_root = this.RootDirectory;
			}

			foreach (var question_element in questionsElement.Elements())
			{
				switch (question_element.Name.LocalName)
				{
					case "question":
						this.Add(SweetQuestion.Generate(question_element, songs_root));
						break;
					default:
						// ※知らない要素が出てきたらどうしますか？
						break;
				}
			}
		}
		#endregion

		// (0.1.5)Noの処理をここで行うようにする．
		// (0.1.3)
		#region *mutus2のsongs要素を読み込む(LoadMutus2SongsElement)
		public void LoadMutus2SongsElement(XElement songsElement /*, string source_directory*/)
		{
			var root = (string?)songsElement.Attribute("current_directory") ?? String.Empty;
			if (!string.IsNullOrEmpty(root) && Path.IsPathRooted(root))
			{
				this.RootDirectory = root;
			}
			// これってどうなってるんだっけ？
			// 1. current_directoryに絶対パスが入っている．
			// 2. current_directoryがなくて，各曲のファイル名がフルパス．
			// 1と2のとちらかだったと思ったけど...

			foreach (var category in songsElement.Elements("category"))
			{
				var category_name = (string?)category.Attribute("name") ?? String.Empty;
				int n = 1;
				foreach (var song in category.Elements("song"))
				{
					// ちょっといびつかもしれないが，Noはここで管理してみる．

					// no属性は，番号がついている場合には存在せず，
					// 番号が付されていない場合は"practice"という値が与えられている，
					// 他の値はとらない，という仕様でいいんだっけ？

					var question = SweetQuestion.GenerateFromMutus2(song, root, category_name);
					if (song.Attribute("no") == null)
					{
						question.No = n++;
					}
					this.Add(question);
				}
			}
		}
		#endregion


		#endregion

	}
}
