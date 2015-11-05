﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aldentea.Wpf.Document;
using System.Xml;
using System.Xml.Linq;
using GrandMutus.Data;
using System.IO;


namespace Aldentea.SweetMutus.Data
{

	#region SweetMutusDocumentクラス
	public class SweetMutusDocument : DocumentWithOperationHistory
	{
		#region *Questionsプロパティ
		/// <summary>
		/// 問題のコレクションを取得します．
		/// </summary>
		public SweetQuestionsCollection Questions { get { return _questions; } }
		readonly SweetQuestionsCollection _questions;
		#endregion

		// (0.1.9)
		#region *Songsプロパティ
		/// <summary>
		/// Questionsプロパティと同じものを返しますが，ISongの集合として見たいとき(曲のインポートとか)に使います．
		/// ※プロパティの変更通知はされないかもしれません．
		/// </summary>
		public SweetQuestionsCollection Songs { get { return _questions; } }
		#endregion

		#region *WriterSettingsプロパティ
		public XmlWriterSettings WriterSettings
		{
			get { return _xmlWriterSettings; }
		}
		XmlWriterSettings _xmlWriterSettings;
		#endregion

		// (0.3.3)Questions関連処理を追加。
		#region *コンストラクタ(SweetMutusDocument)
		public SweetMutusDocument()
		{
			// Questions関連処理
			_questions = new SweetQuestionsCollection(this);
			//_questions.QuestionsRemoved += Questions_QuestionsRemoved;
			_questions.ItemChanged += Songs_ItemChanged;
			_questions.RootDirectoryChanged += Questions_RootDirectoryChanged;
			_questions.QuestionNoChanged += Questions_QuestionNoChanged;

			// カレントカテゴリ関連
			//this.Opened += SweetMutusDocument_Opened;

			// XML出力関連処理
			_xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}
		#endregion

		#region *アイテム変更時(Songs_ItemChanged)
		// (0.1.4)QuestionNoChangedイベントを発生。
		// (0.1.2.1)QuestionCategoryChangedイベントを発生。
		// (0.2.0)Songs以外でも共通に使えるのではなかろうか？
		void Songs_ItemChanged(object sender, ItemEventArgs<IOperationCache> e) // Aldentea.Wpf.DocumentにもIOperationCacheがある．
		{
			var operationCache = (IOperationCache)e.Item;
			if (operationCache != null)
			{
				this.AddOperationHistory(operationCache);
				if (operationCache is QuestionCategoryChangedCache)
				{
					this.QuestionCategoryChanged(this, EventArgs.Empty);
				}
				// ★ここに書くと，Undoのときにイベントが発生しないのでは...
				if (operationCache is QuestionNoChangedCache)
				{
					this.QuestionNoChanged(this, EventArgs.Empty);
				}
			}
		}
		#endregion


		#region 曲関連

		// Songオブジェクトはここで(のみ)作るようにする？

		/// <summary>
		/// (0.3.4)現時点では未使用！
		/// </summary>
		List<string> _addedSongFiles = new List<string>();

		#region *曲を追加(AddQuestions)
		/// <summary>
		/// このメソッドが直接呼び出されることは想定していません．
		/// 呼び出し元でAddSongsActionに設定されるActionの中で呼び出して下さい(ややこしい...)．
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public SweetQuestion AddQuestion(string fileName)
		{
			SweetQuestion question = new SweetQuestion { FileName = fileName };
			LoadInformation(question);
			return this.AddQuestion(question);
		}

		private SweetQuestion AddQuestion(SweetQuestion question)
		{
			try
			{
				_questions.Add(question);	// この後の処理でSongDuplicateExceptionが発生する。
				_addedSongFiles.Add(question.FileName);
				//SongAdded(this, new ItemEventArgs<Song> { Item = song });
				return question;
			}
			catch (SongDuplicateException)
			{
				// この時点ではsongが_songsに追加された状態になっているので、ここで削除する。
				_questions.Remove(question);
				return null;
			}
		}


		public void AddQuestions(IEnumerable<string> fileNames)
		{
			IList<SweetQuestion> added_questions;

			if (AddQuestionsAction == null)
			{
				// 同期的に実行．
				added_questions = new List<SweetQuestion>();
				foreach (var fileName in fileNames)
				{
					var question = AddQuestion(fileName);
					if (question != null)
					{ added_questions.Add(question); }
				}
			}
			else
			{
				// 通常は呼び出し元に制御を渡して，UIを表示する．
				added_questions = AddQuestionsAction.Invoke(fileNames);
			}
			
			AddOperationHistory(new SweetQuestionsAddedCache(this, added_questions.ToArray()));
		}

		#endregion

		public Func<IEnumerable<string>, IList<SweetQuestion>> AddQuestionsAction = null;

		// (0.3.0)
		#region *問題を削除(RemoveQuestions)
		//public void RemoveSongs(IEnumerable<string> fileNames)
		//{
		//	RemoveSongs(fileNames.Select(fileName => _songs.FirstOrDefault(s => s.FileName == fileName)).Where(s => s != null));
		//}

		// (0.0.6.3)UIから削除する場合も，このメソッドを経由することにしたので，OperationCacheの追加はここで行う．
		// (0.3.1)OperationCacheの追加はQuestionsRemovedイベントハンドラで行うことにする
		// (曲の削除はUIから直接行われることが想定されるため)．
		// (0.3.0)
		public void RemoveQuestions(IEnumerable<SweetQuestion> questions)
		{
			IList<SweetQuestion> removed_questions = new List<SweetQuestion>();
			foreach (var question in questions)
			{
				if (_questions.Remove(question))
				{
					removed_questions.Add(question);
				}
			}
			AddOperationHistory(new SweetQuestionsRemovedCache(this, removed_questions));
		}
		#endregion

		// (0.3.1)
		//void Questions_QuestionsRemoved(object sender, ItemEventArgs<IEnumerable<SweetQuestion>> e)
		//{
		//	AddOperationHistory(new SweetQuestionsRemovedCache(this, e.Item));
		//}

		// (0.1.7)再生開始位置もロードするように変更．
		// HyperMutusからのパクリ．古いメソッドだけど，とりあえずそのまま使う．
		// 場所も未定．とりあえずstatic化してここに置いておく．
		#region *[static]ファイルからメタデータを読み込み(LoadInformation)
		/// <summary>
		/// songのFileNameプロパティで指定されたファイルからメタデータを読み込みます．
		/// </summary>
		static void LoadInformation(SweetQuestion question)
		{
			SPP.Aldente.IID3Tag tag;
			try
			{
				tag = SPP.Aldente.AldenteMP3TagAccessor.ReadFile(question.FileName);
			}
			catch (ApplicationException)
			{
				tag = null;
			}
			question.Title = tag == null ? string.Empty : tag.Title;
			question.Artist = tag == null ? string.Empty : tag.Artist;
			question.SabiPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.SabiPos));
			question.PlayPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.StartPos));
		}
		#endregion


		#endregion


		#region 問題関連

		// (0.1.4)
		#region *IDから問題を取得(FindQuestion)
		/// <summary>
		/// 与えられたIDに対応する問題を返します(内部でSingleメソッドを使っています)．
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public SweetQuestion FindQuestion(int id)
		{
			return _questions.Single(q => q.ID == id);
		}
		#endregion

		// (*0.4.1)
		#region *問題を追加(AddQuestions)
		/// <summary>
		/// 問題削除をアンドゥしたときに使うことを想定しています。
		/// (0.1.8からは，インポートの時にも使っています．)
		/// </summary>
		/// <param name="questions"></param>
		public void AddQuestions(IEnumerable<SweetQuestion> questions)
		{
			// ここは同期実行でいいでしょう。
			var added_questions = new List<SweetQuestion>();
			foreach (var question in questions)
			{
				var added_song = AddQuestion(question);
				if (added_song != null)
				{ added_questions.Add(added_song); }
			}
			AddOperationHistory(new SweetQuestionsAddedCache(this, added_questions.ToArray()));
		}
		#endregion

		// (0.1.8)
		#region *曲をインポート(ImportSongs)
		public void ImportSongs(IEnumerable<GrandMutus.Data.ISong> songs)
		{
			this.AddQuestions(songs.Select(song => new SweetQuestion(song)));
		}
		#endregion


		// (*0.4.5.1)
		#region *Questionの番号変更時(Questions_NoChanged)
		void Questions_QuestionNoChanged(object sender, ValueChangedEventArgs<int?> e)
		{
			var question = (SweetQuestion)sender;
			AddOperationHistory(new QuestionNoChangedCache(question, e.PreviousValue, e.CurrentValue));
		}
		#endregion

		// (*0.4.4)
		#region *曲のルートディレクトリ変更時(Questions_RootDirectoryChanged)
		void Questions_RootDirectoryChanged(object sender, ValueChangedEventArgs<string> e)
		{
			this.AddOperationHistory(new RootDirectoryChangedCache(this.Questions, e.PreviousValue, e.CurrentValue));
		}
		#endregion

		void Questions_QuestionCategoryChanged(object sender, EventArgs e)
		{

		}
		/// <summary>
		/// 問題のカテゴリ変更があったときに発生します。
		/// </summary>
		public event EventHandler QuestionCategoryChanged = delegate { };

		/// <summary>
		/// 問題のNo変更があったときに発生します。
		/// </summary>
		public event EventHandler QuestionNoChanged = delegate { };

		#endregion


		#region ファイル入出力関連

		// <mutus version="3.0">
		//   <sweet>
		//     <questions>
		//        <question ... >

		public const string ROOT_ELEMENT_NAME = "mutus";
		const string VERSION_ATTERIBUTE = "version";
		const string SWEET_ELEMENT_NAME = "sweet";

		// (0.0.1)エクスポートの場合に対応したつもりです．
		#region *XMLを生成(GenerateXML)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination_directory">出力されるXMLファイルのディレクトリのフルパスを与えます．</param>
		/// <param name="exported_songs_root">エクスポートするときは，その曲を格納するディレクトリの名前を与えます．
		/// そうでなければnullを与えます．</param>
		/// <returns></returns>
		public XDocument GenerateXml(string destination_directory, string exported_songs_root = null)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));
			XElement sweet = new XElement(SWEET_ELEMENT_NAME);
			sweet.Add(Questions.GenerateElement(destination_directory, exported_songs_root));
			xdoc.Root.Add(sweet);
			return xdoc;
		}
		#endregion

		// (0.1.1)
		#region *HyperMutusのXMLを生成(GenerateMtuXML)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="destination_directory">出力されるXMLファイルのディレクトリのフルパスを与えます．</param>
		/// <param name="exported_songs_root">エクスポートするときは，その曲を格納するディレクトリの名前を与えます．
		/// そうでなければnullを与えます．</param>
		/// <returns></returns>
		public XDocument GenerateMtuXml(string destination_directory, string exported_songs_root = null)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));

			xdoc.Root.Add(Questions.GenerateQuestionsElement());
			xdoc.Root.Add(Questions.GenerateSongsElement(destination_directory, exported_songs_root));

			return xdoc;
		}
		#endregion

		#endregion

		/// <summary>
		/// エクスポート時にファイルを保存します．
		/// 曲ファイルのコピーは予め済ませておいて下さい．
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="songs_root"></param>
		public void SaveExport(string destination, string songs_root)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(System.IO.Path.GetDirectoryName(destination), songs_root).WriteTo(writer);
			}

		}


		#region DocumentBase実装

		// (0.1.0)基底クラスのメソッド名の変更を反映．
		protected override void InitializeDocument()
		{
			base.InitializeDocument();
			Questions.Initialize();
		}

		// (0.1.3)mutus2のファイルに対応？
		// (*0.4.0.1)Songs.RootDirectoryの設定を追加。
		#region *[override]ファイルからロード(LoadDocument)
		protected override bool LoadDocument(string fileName)
		{
			using (XmlReader reader = XmlReader.Create(fileName))
			{
				var xdoc = XDocument.Load(reader);
				var root = xdoc.Root;

				// ☆ここから下は，継承先でオーバーライドできるようにしておきましょう．
				if (root.Name == ROOT_ELEMENT_NAME)
				{
					decimal? version = (decimal?)root.Attribute(VERSION_ATTERIBUTE);
					if (version.HasValue)
					{
						if (version >= 3.0M)
						{
							var sweet = root.Element(SWEET_ELEMENT_NAME);
							NowLoading = true;
							try
							{
								this.Questions.LoadElement(sweet.Element(SweetQuestionsCollection.ELEMENT_NAME), Path.GetDirectoryName(fileName));
							}
							finally
							{
								NowLoading = false;
							}
							return true;
						}
						else if (version >= 2.0M)
						{
							// mutus2のファイル？
							var songs = root.Element("songs");
							if (Confirm("mutus2のファイルを読み込みます．保存するときにSweetMutusの形式に変換することになります(情報の一部が失われることがあります)．\n"
								+ "処理を続行しますか？"))
							{
								NowLoading = true;
								try
								{
									this.Questions.LoadMutus2SongsElement(songs/*, Path.GetDirectoryName(fileName)*/);
									this.IsConverted = true;
								}
								finally
								{
									NowLoading = false;
								}
								return true;
							}
							else
							{
								return false;
							}
						}
					}
				}

			}
			return false;
		}
		#endregion

		#region *[override]ファイルに保存(SaveDocument)
		protected override bool SaveDocument(string destination)
		{
			
			// 拡張子に応じてフォーマットを決める。
			var ext = Path.GetExtension(destination);	// extには"."を含む。

			switch (ext)
			{
				case ".mtu":
				case ".mtq":
					return SaveMtqDocument(destination);
				default:
					return SaveSmtDocument(destination);
			}

		}
		#endregion

		#region 保存関連メソッド

		bool SaveSmtDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(System.IO.Path.GetDirectoryName(destination)).WriteTo(writer);
			}
			// 基本的にtrueを返せばよろしい．
			// falseを返すべきなのは，保存する前にキャンセルした時とかかな？
			return true;
		}

		bool SaveMtqDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateMtuXml(System.IO.Path.GetDirectoryName(destination)).WriteTo(writer);
			}
			return true;
		}

		#endregion

		#endregion

	}
	#endregion

}
