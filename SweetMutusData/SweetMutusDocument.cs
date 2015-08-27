using System;
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

	public class SweetMutusDocument : GrandMutus.Data.DocumentWithOperationHistory
	{

		#region MutusDocumentのコピペ

		// (0.3.3)とりあえずIntroQuestionだけに対応している。
		public SweetQuestionsCollection Questions { get { return _questions; } }
		readonly SweetQuestionsCollection _questions;

		#region *WriterSettingsプロパティ
		public XmlWriterSettings WriterSettings
		{
			get { return _xmlWriterSettings; }
		}
		XmlWriterSettings _xmlWriterSettings;
		#endregion

		// (0.3.3)Questions関連処理を追加。
		#region *コンストラクタ(MutusDocument)
		public SweetMutusDocument()
		{
			// Questions関連処理
			_questions = new SweetQuestionsCollection(this);
			_questions.QuestionsRemoved += Questions_QuestionsRemoved;

			// XML出力関連処理
			_xmlWriterSettings = new XmlWriterSettings
			{
				Indent = true,
				NewLineHandling = NewLineHandling.Entitize
			};
		}
		#endregion

		// (0.2.0)Songs以外でも共通に使えるのではなかろうか？
		void Songs_ItemChanged(object sender, ItemEventArgs<GrandMutus.Data.IOperationCache> e) // Aldentea.Wpf.DocumentにもIOperationCacheがある．
		{
			var operationCache = (GrandMutus.Data.IOperationCache)e.Item;
			if (operationCache != null)
			{
				this.AddOperationHistory(operationCache);
			}
		}


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

		// (0.3.1)OperationCacheの追加はQuestionsRemovedイベントハンドラで行うことにする
		// (曲の削除はUIから直接行われることが想定されるため)．
		// (0.3.0)
		public void RemoveQuestions(IEnumerable<SweetQuestion> questions)
		{
			IList<string> removed_song_files = new List<string>();
			foreach (var question in questions)
			{
				if (_questions.Remove(question))
				{
					removed_song_files.Add(question.FileName);
				}
			}
		}
		#endregion

		// (0.3.1)
		void Questions_QuestionsRemoved(object sender, ItemEventArgs<IEnumerable<SweetQuestion>> e)
		{
			AddOperationHistory(new SweetQuestionsRemovedCache(this, e.Item));
		}

		// HyperMutusからのパクリ．古いメソッドだけど，とりあえずそのまま使う．
		// 場所も未定．とりあえずstatic化してここに置いておく．
		#region *[static]ファイルからメタデータを読み込み(LoadInformation)
		/// <summary>
		/// songのFileNameプロパティで指定されたファイルからメタデータを読み込みます．
		/// </summary>
		static void LoadInformation(SweetQuestion question)
		{
			SPP.Aldente.IID3Tag tag = SPP.Aldente.AldenteMP3TagAccessor.ReadFile(question.FileName);
			question.Title = tag == null ? string.Empty : tag.Title;
			question.Artist = tag == null ? string.Empty : tag.Artist;
			question.SabiPos = tag == null ? TimeSpan.Zero : TimeSpan.FromSeconds(Convert.ToDouble(tag.SabiPos));
		}
		#endregion


		#endregion


		#region 問題関連

		// (0.3.4)とりあえず．
		/*public void AddSweetQuestions(IEnumerable<Song> songs)
		{
			var added_questions = new List<SweetQuestion>();
			foreach (var song in songs)
			{
				var question = new SweetQuestion(song.ID);
				_questions.Add(question);
				added_questions.Add(question);
			}
			// ここで操作履歴処理を行う．(削除の場合と異なりUIから直接，というのは考えられない．)
			AddOperationHistory(new SweetQuestionsAddedCache(this, added_questions.ToArray()));
		}
		*/
		// (0.4.1)
		#region *問題を追加(AddQuestions)
		//private SweetQuestion AddQuestion(SweetQuestion question)
		//{
		//	_questions.Add(question);	// ←失敗することは通常想定されないよね？
		//	return question;
		//}

		/// <summary>
		/// 問題削除をアンドゥしたときに使うことを想定しています。
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

		// (0.4.1)



		#endregion


		#region ファイル入出力関連

		// <mutus version="3.0">
		//   <sweet>
		//     <questions>
		//        <question ... >

		public const string ROOT_ELEMENT_NAME = "mutus";
		const string VERSION_ATTERIBUTE = "version";
		const string SWEET_ELEMENT_NAME = "sweet";

		// (0.3.3.1)Questionsの出力を追加．
		public XDocument GenerateXml(string destination)
		{
			XDocument xdoc = new XDocument(new XElement(ROOT_ELEMENT_NAME, new XAttribute(VERSION_ATTERIBUTE, "3.0")));
			XElement sweet = new XElement(SWEET_ELEMENT_NAME);
			sweet.Add(Questions.GenerateElement(System.IO.Path.GetDirectoryName(destination)));
			xdoc.Add(sweet);
			return xdoc;
		}

		#endregion


		#region DocumentBase実装

		protected override void Initialize()
		{
			base.Initialize();
			Questions.Initialize();
		}

		// (0.4.0.1)Songs.RootDirectoryの設定を追加。
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
							this.Questions.RootDirectory = Path.GetDirectoryName(fileName);	// RootDirectoryのデフォルトの値を設定する。
							this.Questions.LoadElement(sweet.Element(SweetQuestionsCollection.ELEMENT_NAME));
							return true;
						}
					}
				}

			}
			return false;
		}

		protected override bool SaveDocument(string destination)
		{
			using (XmlWriter writer = XmlWriter.Create(destination, this.WriterSettings))
			{
				GenerateXml(destination).WriteTo(writer);
			}
			// 基本的にtrueを返せばよろしい．
			// falseを返すべきなのは，保存する前にキャンセルした時とかかな？
			return true;
		}
		#endregion


		#endregion


	}

}
