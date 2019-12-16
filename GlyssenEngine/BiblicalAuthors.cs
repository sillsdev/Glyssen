﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using SIL.Xml;

namespace GlyssenEngine
{
	[XmlRoot("BiblicalAuthors")]
	public class BiblicalAuthors : List<BiblicalAuthors.Author>
	{
		private static BiblicalAuthors s_all;

		public static BiblicalAuthors All()
		{
			if (s_all == null)
				Load();
			return s_all;
		}

		public static Author GetAuthorOfBook(string bookId)
		{
			return All().Find(a => a.Wrote(bookId));
		}

		public static int GetAuthorCount(IEnumerable<string> bookIds)
		{
			HashSet<string> uniqueAuthors = new HashSet<string>();
			foreach (var bookId in bookIds)
				uniqueAuthors.Add(GetAuthorOfBook(bookId).Name);
			return uniqueAuthors.Count;
		}

		private static void Load()
		{
			s_all = XmlSerializationHelper.DeserializeFromString<BiblicalAuthors>(Resources.BiblicalAuthors);
		}

		public string GetAsXml()
		{
			return XmlSerializationHelper.SerializeToString(this);
		}

		public class Author
		{
			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlAttribute("combineAuthorAndNarrator")]
			[DefaultValue(false)]
			public bool CombineAuthorAndNarrator { get; set; }

			[XmlAttribute("doNotCombineByDefault")]
			[DefaultValue(false)]
			public bool DoNotCombineByDefault { get; set; }

			[XmlArrayItem("Book")]
			public List<string> Books { get; set; }

			public bool Wrote(string bookId)
			{
				return Books.Contains(bookId);
			}
		}
	}
}
