using System;
using System.Xml.Linq;
using System.IO;

namespace Aldentea.SweetMutus.Data
{
	// (0.1.8)ISongインターフェイスを実装．
	#region SweetQuestionクラス
	public class SweetQuestion : GrandMutus.Data.QuestionBase<SweetQuestionsCollection>, GrandMutus.Data.ISong /*, IEditableObject*/
	{
		// (0.1.8)ISongを引数にとるコンストラクタを追加．
		#region *コンストラクタ(SweetQuestion)
		public SweetQuestion()
		{ }

		public SweetQuestion(GrandMutus.Data.ISong song)
		{
			this.Title = song.Title;
			this.Artist = song.Artist;
			this.FileName = song.FileName;
			this.SabiPos = song.SabiPos;
		}
		#endregion

		#region Songのコピペ

		#region *Titleプロパティ
		/// <summary>
		/// 曲のタイトルを取得／設定します．
		/// </summary>
		public string Title
		{
			get
			{
				return _title;
			}
			set
			{
				if (Title != value)
				{
					NotifyPropertyChanging("Title");
					this._title = value;
					NotifyPropertyChanged("Title");
				}
			}
		}
		string _title = string.Empty;
		#endregion

		// (0.1.2)RelativeFileNameプロパティの変更を通知。
		#region *FileNameプロパティ
		/// <summary>
		/// 曲のファイル名を(とりあえずフルパスで)取得／設定します．
		/// </summary>
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				if (FileName != value)
				{
					NotifyPropertyChanging("FileName");
					this._fileName = value;
					NotifyPropertyChanged("FileName");
					NotifyPropertyChanged("RelativeFileName");
				}
			}
		}
		string _fileName = string.Empty;
		#endregion

		#region *Artistプロパティ
		/// <summary>
		/// 曲のアーティストを取得／設定します．
		/// </summary>
		public string Artist
		{
			get
			{
				return _artist;
			}
			set
			{
				if (Artist != value)
				{
					NotifyPropertyChanging("Artist");
					this._artist = value;
					NotifyPropertyChanged("Artist");
				}
			}
		}
		string _artist = string.Empty;
		#endregion

		// (*0.3.2)TimeSpan型にしてみました．
		#region *SabiPosプロパティ
		/// <summary>
		/// 曲のサビの位置を取得／設定します．
		/// </summary>
		public TimeSpan SabiPos
		{
			get
			{
				return _sabiPos;
			}
			set
			{
				if (this.SabiPos != value)
				{
					NotifyPropertyChanging("SabiPos");
					this._sabiPos = value;
					NotifyPropertyChanged("SabiPos");
				}
			}
		}
		TimeSpan _sabiPos = TimeSpan.Zero;
		#endregion

		// (*0.3.0)Parentがnullの場合の対策をしておく(OnAddTo(null)が呼ばれるときにまずいことになったので)．
		// (*0.2.1)←もっと前？
		#region *RelativeFileNameプロパティ
		public string RelativeFileName
		{
			get
			{
				string root = Parent == null ? string.Empty : Parent.RootDirectory;
				if (!string.IsNullOrEmpty(root) && this.FileName.StartsWith(root))
				{
					return this.FileName.Substring(root.Length).TrimStart('\\');
				}
				else
				{
					return this.FileName;
				}
			}
		}

		// (*0.4.3)
		/// <summary>
		/// RelativeFileNameプロパティが変更になりうる場合を通知します．
		/// </summary>
		internal void UpdateRelativeFileName()
		{
			this.NotifyPropertyChanged("RelativeFileName");
		}

		#endregion

		// (0.1.2)
		#region *SongTagプロパティ
		/// <summary>
		/// 予め指定した形式で，曲ファイルのメタデータのオブジェクトを取得します．
		/// ※いまのところは常にID3v23を返すようになっています．
		/// </summary>
		public SPP.Aldente.IID3Tag SongTag
		{
			get
			{
				// とりあえずID3v23を生成する．
				return new SPP.Aldente.ID3v23Tag {
					Title = this.Title,
					Artist = this.Artist,
					SabiPos = Convert.ToDecimal(this.SabiPos.TotalSeconds),
					StartPos = Convert.ToDecimal(this.PlayPos.TotalSeconds)
				};
			}
		}
		#endregion

		// (0.1.2)
		// 08/19/2013 by aldentea : ★★RealFileNameプロパティの廃止に対応．
		// 09/29/2011 by aldentea
		#region *曲情報を曲ファイルに保存(SaveInfomation)
		/// <summary>
		/// 曲情報を曲ファイルのメタデータとして保存します．再生開始位置と再生停止位置は0.0で置き換えるので注意して下さい．
		/// </summary>
		public int SaveInformation()
		{
			// startPosとStopPosは0.0Mで書き換える！
			if (File.Exists(this.FileName))
			{ 
//			int ret = 0;
//			while (true)
//			{
//				try
//				{
					// ひどいAPIだね．
					//aldente.AldenteMP3TagAccessor.UpdateInfo(this.Title, this.Artist, this.SabiPos, 0.0M, 0.0M, this.RealFileName, 88);
					//AldenteMP3TagAccessor.UpdateInfo(this.Title, this.Artist, this.SabiPos, 0.0M, 0.0M, this.FileName, 88);

					// ※それでは，どうすればいいか？
					// IID3Tagの基礎的な実装をしたクラスを用意する(ただしI/Oは未実装)．
					// で，それをUpdateInfoに渡す．
					SPP.Aldente.AldenteMP3TagAccessor.UpdateInfo(this.FileName, this.SongTag);


//					break;
//				}
//				catch (IOException ex)
//				{
//					ret += 1;
//					if (ret > 100) { throw ex; }
//				}
			}
//			return ret;
					return 0;
		}
		#endregion

		#endregion

		#region IntroQuestionのコピペ

		#region *PlayPosプロパティ
		/// <summary>
		/// 出題時の再生開始位置を取得／設定します．
		/// </summary>
		public TimeSpan PlayPos
		{
			get
			{
				return _playPos;
			}
			set
			{
				if (this.PlayPos != value)
				{
					// 負の値でないことをここでチェックした方がいいかな？

					NotifyPropertyChanging("PlayPos");
					this._playPos = value;
					NotifyPropertyChanged("PlayPos");
				}
			}
		}
		TimeSpan _playPos = TimeSpan.Zero;
		#endregion

		// 機能はいつの間にか実装されていた．
		// (0.1.3.1)器だけ作っておく．設定のUIや機能は未実装．
		#region *StopPosプロパティ
		/// <summary>
		/// 出題時の再生停止位置を取得／設定します．
		/// TimeSpan.Zeroであれば，停止しないことを意味します．
		/// </summary>
		public TimeSpan StopPos
		{
			get
			{
				return _stopPos;
			}
			set
			{
				if (this.StopPos != value)
				{
					// 負の値でないことをここでチェックした方がいいかな？

					NotifyPropertyChanging("StopPos");
					this._stopPos = value;
					NotifyPropertyChanged("StopPos");
				}
			}
		}
		TimeSpan _stopPos = TimeSpan.Zero;
		#endregion

		#endregion


		#region XML入出力関連

		// どこに置くかは未定．
		// ここに置くか，XML生成専用のクラスを作成するか...

		// このあたり本来はXNameなんだけど，手抜きをする．
		public const string ELEMENT_NAME = "question";
		const string ID_ATTRIBUTE = "id";
		const string NO_ATTRIBUTE = "no";
		const string CATEGORY_ATTRIBUTE = "category";
		const string PLAY_POS_ATTRIBUTE = "play_pos";
		const string STOP_POS_ATTRIBUTE = "stop_pos";

		const string TITLE_ELEMENT = "title";
		const string ARTIST_ELEMENT = "artist";
		const string FILE_NAME_ELEMENT = "file_name";
		const string SABI_POS_ATTRIBUTE = "sabi_pos";

		// (0.3.2)sabi_pos要素を出力．
		#region *XML要素を生成(GenerateElement)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="songs_root">曲ファイルを格納しているディレクトリのフルパスです．</param>
		/// <returns></returns>
		public XElement GenerateElement(string songs_root, bool exporting = false)
		{
			var element = new XElement(ELEMENT_NAME,
				new XAttribute(ID_ATTRIBUTE, this.ID)
			);
			return AddSongProperty(AddIntroQuestionProperty(element), songs_root, exporting);
		}
		#endregion

		// (0.1.1)
		#region Mtuファイル出力用メソッド

		public XElement GenerateQuestionElement()
		{
			var element = new XElement(GrandMutus.Data.IntroQuestion.ELEMENT_NAME
				, new XAttribute("id", this.ID), new XAttribute("song_id", this.ID)
			);

			return AddIntroQuestionProperty(element);
		}

		public XElement GenerateSongElement(string songs_root, bool exporting = false)
		{
			var element = new XElement(GrandMutus.Data.Song.ELEMENT_NAME,
				new XAttribute("id", this.ID)
			);
			return AddSongProperty(element, songs_root, exporting);
		}

		#endregion

		#region プロパティをXElementに出力

		XElement AddIntroQuestionProperty(XElement element)
		{
			// ※IDやSongIDはここでは追加しない！

			if (this.No.HasValue)
			{
				element.Add(new XAttribute(NO_ATTRIBUTE, this.No));
			}
			if (!string.IsNullOrEmpty(this.Category))
			{
				element.Add(new XAttribute(CATEGORY_ATTRIBUTE, this.Category));
			}
			if (this.PlayPos > TimeSpan.Zero)
			{
				element.Add(new XAttribute(PLAY_POS_ATTRIBUTE, this.PlayPos.TotalSeconds));
			}
			if (this.StopPos > TimeSpan.Zero)
			{
				element.Add(new XAttribute(STOP_POS_ATTRIBUTE, this.StopPos.TotalSeconds));
			}

			// ☆answer要素はどうする？
			return element;
		}

		XElement AddSongProperty(XElement element, string songs_root, bool exporting = false)
		{
			// ※IDはここでは追加しない！

			if (this.SabiPos > TimeSpan.Zero)
			{
				element.Add(new XAttribute(SABI_POS_ATTRIBUTE, this.SabiPos.TotalSeconds));
			}
			element.Add(new XElement(TITLE_ELEMENT, this.Title));
			element.Add(new XElement(ARTIST_ELEMENT, this.Artist));

			// XMLに出力する曲のファイル名．
			string file_full_name = exporting ? Path.Combine(songs_root, Path.GetFileName(this.FileName)) : this.FileName;
			string file_name;
			if (!string.IsNullOrEmpty(songs_root) && file_full_name.Contains(songs_root))
			{
				// songs_rootからの相対パスを記録．
				file_name = file_full_name.Substring(songs_root.Length).TrimStart('\\');
			}
			else
			{
				// フルパスを記録．
				file_name = file_full_name;
			}
			element.Add(new XElement(FILE_NAME_ELEMENT, file_name));

			return element;
		}

		#endregion

		// (0.3.3)
		#region *[static]XML要素からオブジェクトを生成(Generate)
		public static SweetQuestion Generate(XElement questionElement, string songsRoot = null)
		{
			var question = new SweetQuestion();

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = questionElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				question.ID = (int)id_attribute;
			}
			question.Category = (string)questionElement.Attribute(CATEGORY_ATTRIBUTE);
			
			question.No = (int?)questionElement.Attribute(NO_ATTRIBUTE);

			question.Title = (string)questionElement.Element(TITLE_ELEMENT);
			question.Artist = (string)questionElement.Element(ARTIST_ELEMENT);
			var file_name = (string)questionElement.Element(FILE_NAME_ELEMENT);	// 相対パスをフルパスに直す作業が必要！
			if (!Path.IsPathRooted(file_name))
			{
				file_name = Path.Combine(songsRoot, file_name);
				if (!Path.IsPathRooted(file_name))
				{
					throw new ArgumentException("ファイル名が相対パスで記録されています．songsRootには，絶対パスを指定して下さい．", "songsRoot");
				}
			}
			question.FileName = file_name;
			var sabi_pos = (double?)questionElement.Attribute(SABI_POS_ATTRIBUTE);
			if (sabi_pos.HasValue)
			{
				question.SabiPos = TimeSpan.FromSeconds(sabi_pos.Value);
			}
			var play_pos = (double?)questionElement.Attribute(PLAY_POS_ATTRIBUTE);
			if (play_pos.HasValue)
			{
				question.PlayPos = TimeSpan.FromSeconds(play_pos.Value);
			}
			var stop_pos = (double?)questionElement.Attribute(STOP_POS_ATTRIBUTE);
			if (stop_pos.HasValue)
			{
				question.StopPos = TimeSpan.FromSeconds(stop_pos.Value);
			}

			return question;
		}
		#endregion

		// (0.1.5)Noの処理は，1つ上(SweetQuestionsCollectionクラス)で行うようにする．
		// (0.1.3)
		#region *[static]mutus2のsong要素からオブジェクトを生成する(GenerateFromMutus2)
		public static SweetQuestion GenerateFromMutus2(XElement songElement, string songsRoot, string category)
		{
			//	<song id="6" sabipos="21.7">
			//		<title>誰もいない海</title>
			//		<artist>トワエモア</artist>
			//		<filename>intro\JOY02554.mp3</filename>
			//	</song>

			var question = new SweetQuestion();

			// XMLからインスタンスを生成するならばIDは常にあるのでは？
			// →songをインポートする時とかはそうではないかもしれない？ので一応有無をチェックする．
			// →でも，インポートする時はXML経由ではなくオブジェクト経由で行った方がいいのでは？(ファイル名のパスの扱いとか...)
			var id_attribute = songElement.Attribute(ID_ATTRIBUTE);
			if (id_attribute != null)
			{
				question.ID = (int)id_attribute;
			}
			question.Category = category;

			// mutus2のNoは，それ以降のものとかなり仕様が違うのであった！
			//question.No = (int?)songElement.Attribute(NO_ATTRIBUTE);

			question.Title = (string)songElement.Element(TITLE_ELEMENT);
			question.Artist = (string)songElement.Element(ARTIST_ELEMENT);
			var file_name = (string)songElement.Element("filename");	// 相対パスをフルパスに直す作業が必要！
			if (!Path.IsPathRooted(file_name))
			{
				file_name = Path.Combine(songsRoot, file_name);
				if (!Path.IsPathRooted(file_name))
				{
					throw new ArgumentException("ファイル名が相対パスで記録されています．songsRootには，絶対パスを指定して下さい．", "songsRoot");
				}
			}
			question.FileName = file_name;
			var sabi_pos = (double?)songElement.Attribute("sabipos");
			if (sabi_pos.HasValue)
			{
				question.SabiPos = TimeSpan.FromSeconds(sabi_pos.Value);
			}
			var play_pos = (double?)songElement.Attribute("playpos");
			if (play_pos.HasValue)
			{
				question.PlayPos = TimeSpan.FromSeconds(play_pos.Value);
			}
			var stop_pos = (double?)songElement.Attribute("stoppos");
			if (stop_pos.HasValue)
			{
				question.StopPos = TimeSpan.FromSeconds(stop_pos.Value);
			}

			return question;

		}
		#endregion

		#endregion

	}
	#endregion

}
