using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CAT.TM
{
	public class Score
	{
		public Score(int id, int wordcount, float score, bool isRepetition)
		{
			this.id = id;
			this.wordcount = wordcount;
			this.score = score;
			this.isRepetition = isRepetition;
		}

		public int id;
		public int wordcount;
		public float score;
		public bool isRepetition;

		public Score CloneAsRepetition()
		{
			return new Score(id, wordcount, score, true);
		}
	}
}