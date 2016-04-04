using System.Collections.Generic;

namespace Glyssen
{
	public enum SingleVoiceReason
	{
		NotSpecified,
		TooComplexToAssignAccurately,
		MostUsersWillNotDramatize
	}

	public static class BookMetadata
	{
		private static readonly Dictionary<string, Book> s_defaultToSingleVoice;

		static BookMetadata()
		{
			s_defaultToSingleVoice = new Dictionary<string, Book>
			{
				{"NEH", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"ISA", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"JER", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"HOS", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"JOL", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"AMO", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"MIC", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"NAM", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"HAB", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"ZEP", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"HAG", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"ZEC", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
				{"MAL", new Book(SingleVoiceReason.TooComplexToAssignAccurately)},
			};
		}

		public static bool DefaultToSingleVoice(string bookCode, out SingleVoiceReason singleVoiceReason)
		{
			Book book;
			if (s_defaultToSingleVoice.TryGetValue(bookCode, out book))
			{
				singleVoiceReason = book.SingleVoiceReason;
				return true;
			}
			singleVoiceReason = SingleVoiceReason.NotSpecified;
			return false;
		}

		private class Book
		{
			public Book() { }

			public Book(SingleVoiceReason singleVoiceReason)
			{
				SingleVoiceReason = singleVoiceReason;
			}

			public SingleVoiceReason SingleVoiceReason { get; set; }
		}
	}
}
