using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aldentea.Wpf.Document;


namespace Aldentea.SweetMutus.Data
{
	// ほとんどQuestionsCacheのコピペ．

	#region [abstract]SweetQuestionsCacheクラス
	public abstract class SweetQuestionsCache : GrandMutus.Data.IOperationCache
	{

		public SweetMutusDocument Document { get; protected set; }
		
		public ISet<SweetQuestion> Questions
		{ get; protected set; }

		protected SweetQuestionsCache(SweetMutusDocument document, IEnumerable<SweetQuestion> questions)
		{
			this.Document = document;
			this.Questions = new HashSet<SweetQuestion>(questions);
		}

		public abstract void Reverse();
		public abstract bool CanCancelWith(GrandMutus.Data.IOperationCache other);

		/// <summary>
		/// Questionsプロパティの中身が同一であればtrueを返します．
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasSameQuestionsWith(SweetQuestionsCache other)
		{
			if (this.Questions.Count == other.Questions.Count)
			{
				return this.Questions.Except(other.Questions).Count() == 0;
			}
			else
			{
				return false;
			}
		}
	}
	#endregion

	// (0.3.4)
	#region SweetQuestionsAddedCacheクラス
	public class SweetQuestionsAddedCache : SweetQuestionsCache
	{
		public SweetQuestionsAddedCache(SweetMutusDocument document, IEnumerable<SweetQuestion> questions)
			: base(document, questions)
		{ }

		public override void Reverse()
		{
			Document.RemoveQuestions(this.Questions);
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			//return false;
			return other is SweetQuestionsRemovedCache
				&& ((SweetQuestionsCache)other).Document == this.Document
				&& this.HasSameQuestionsWith((SweetQuestionsCache)other);
		}
	}
	#endregion


	// (0.4.1)
	#region QuestionsRemovedCacheクラス
	public class SweetQuestionsRemovedCache : SweetQuestionsCache
	{
		public SweetQuestionsRemovedCache(SweetMutusDocument document, IEnumerable<SweetQuestion> questions)
			: base(document, questions)
		{ }


		public override void Reverse()
		{
			Document.AddQuestions(this.Questions);
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			return other is SweetQuestionsAddedCache
				&& ((SweetQuestionsCache)other).Document == this.Document
				&& this.HasSameQuestionsWith((SweetQuestionsCache)other);
		}
	}
	#endregion


	#region RootDirectoryChangedCacheクラス
	public class RootDirectoryChangedCache : GrandMutus.Data.PropertyChangedCache<string>
	{
		readonly SweetQuestionsCollection _questionsCollection;

		public RootDirectoryChangedCache(SweetQuestionsCollection songs, string from, string to)
			: base(from, to)
		{
			this._questionsCollection = songs;
		}

		public override void Reverse()
		{
			_questionsCollection.RootDirectory = this._previousValue;
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			var other_cache = other as RootDirectoryChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				// other_cache._songsCollectionにアクセスできるんだね！
				return other_cache._questionsCollection == this._questionsCollection &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}
	#endregion


	// 以下，個々のアイテムの変更用．

	// (0.2.0)
	#region QuestionTitleChangedCacheクラス
	public class QuestionTitleChangedCache : GrandMutus.Data.PropertyChangedCache<string>
	{
		SweetQuestion _question;

		public QuestionTitleChangedCache(SweetQuestion question, string from, string to)
			: base(from, to)
		{
			this._question = question;
		}

		// DoとかReverseで実行するときにはOperationCacheを新規作成したくないわけだが...

		// →考えられる方法は2つ．
		// 1つは，通常のプロパティのsetterで値をセットするんだけど，キャッシュの作成を抑止する．
		// もう1つは，キャッシュを作成せずに値をセットする別の機構(internalメソッドか？)を用意する．
		// 並列実行の対応も気になりますが…


		//public override void Do()
		//{
		//	_song.Title = _currentValue;
		//}

		public override void Reverse()
		{
			_question.Title = _previousValue;
		}

		// そもそもoperationCache.Reverse(); だけでアンドゥできる仕組みだったのに，
		// 実装側のコードが複雑になってしまっては意味がないのではないか？

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			var other_cache = other as QuestionTitleChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._question == this._question &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

		//public override IOperationCache GetInverse()
		//{
		//	return new SongTitleChangedCache(this._song, this._currentValue, this._previousValue);
		//}

	}
	#endregion

	// (0.2.0)
	#region QuestionArtistChangedCacheクラス
	public class QuestionArtistChangedCache : GrandMutus.Data.PropertyChangedCache<string>
	{
		SweetQuestion _question;

		public QuestionArtistChangedCache(SweetQuestion question, string from, string to)
			: base(from, to)
		{
			this._question = question;
		}

		public override void Reverse()
		{
			_question.Artist = _previousValue;
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			var other_cache = other as QuestionArtistChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._question == this._question &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}
	#endregion

	// (0.4.2)
	#region QuestionSabiPosChangedCacheクラス
	public class QuestionSabiPosChangedCache : GrandMutus.Data.PropertyChangedCache<TimeSpan>
	{
		SweetQuestion _question;

		public QuestionSabiPosChangedCache(SweetQuestion question, TimeSpan from, TimeSpan to)
			: base(from, to)
		{
			this._question = question;
		}

		public override void Reverse()
		{
			_question.SabiPos = _previousValue;
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			var other_cache = other as QuestionSabiPosChangedCache;
			if (other_cache == null)
			{ return false; }
			else
			{
				return other_cache._question == this._question &&
					other_cache._previousValue == this._currentValue &&
					other_cache._currentValue == this._previousValue;
			}
		}

	}
	#endregion

	// (0.4.5.1)
	#region QuestionNoChangedCacheクラス
	public class QuestionNoChangedCache : GrandMutus.Data.PropertyChangedCache<int?>
	{
		readonly SweetQuestion _question;

		public QuestionNoChangedCache(SweetQuestion question, int? from, int? to)
			: base(from, to)
		{
			this._question = question;
		}

		public override void Reverse()
		{
			_question.No = this._previousValue;
		}

		public override bool CanCancelWith(GrandMutus.Data.IOperationCache other)
		{
			if (other is QuestionNoChangedCache)
			{
				var other_cache = ((QuestionNoChangedCache)other);
				return (other_cache._question == this._question)
					&& (other_cache._previousValue == this._currentValue)
					&& (other_cache._currentValue == this._previousValue);
			}
			else
			{
				return false;
			}
		}
	}
	#endregion

}
