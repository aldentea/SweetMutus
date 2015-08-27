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

}
