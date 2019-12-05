using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Glyssen.Shared;
using SIL.Scripture;
using SIL.Xml;

namespace GlyssenEngine
{
	class ControlAnnotations
	{
		private static ControlAnnotations s_singleton;
		private static string s_tabDelimitedData;
		private ILookup<BCVRef, VerseAnnotation> m_annotations;

		internal static string TabDelimitedData
		{
			get { return s_tabDelimitedData; }
			set
			{
				s_tabDelimitedData = value;
				s_singleton = null;
			}
		}

		public static ControlAnnotations Singleton
		{
			get { return s_singleton ?? (s_singleton = new ControlAnnotations()); }
		}

		private ControlAnnotations()
		{
			// Tests can set this before accessing the Singleton.
			if (TabDelimitedData == null)
				TabDelimitedData = Resources.Annotations;
			LoadData();
		}

		public IEnumerable<VerseAnnotation> GetAnnotationsForVerse(BCVRef verseRef)
		{
			return m_annotations[verseRef];
		}

		private void LoadData()
		{
			List<VerseAnnotation> verseAnnotations = new List<VerseAnnotation>();
			foreach (var line in TabDelimitedData.Split(new[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (line.Length == 0 || line[0] == '#')
					continue;
				verseAnnotations.Add(CreateVerseAnnotationFromLine(line));
			}

			m_annotations = verseAnnotations.ToLookup(c => c.Verse);
		}

		private VerseAnnotation CreateVerseAnnotationFromLine(string line)
		{
			string[] items = line.Split(new[] { "\t" }, StringSplitOptions.None);
			ScriptAnnotation annotation;
			string annotationXml = items[4];
			//Enhance: find a way to get serialization to work properly on the base class directly
			if (annotationXml.StartsWith("<Sound"))
				annotation = XmlSerializationHelper.DeserializeFromString<Sound>(annotationXml);
			else
				annotation = XmlSerializationHelper.DeserializeFromString<Pause>(annotationXml);
			if (annotation == null)
				throw new InvalidDataException(string.Format("The annotation {0} could not be deserialized", annotationXml));
			return new VerseAnnotation(new BCVRef(BCVRef.BookToNumber(items[0]), int.Parse(items[1]), int.Parse(items[2])), annotation, int.Parse(items[3]));
		}
	}

	class VerseAnnotation
	{
		private readonly int m_offset;
		private readonly ScriptAnnotation m_annotation;
		private readonly BCVRef m_verse;

		public VerseAnnotation(BCVRef verse, ScriptAnnotation annotation, int offset)
		{
			m_verse = verse;
			m_annotation = annotation;
			m_offset = offset;
		}

		public BCVRef Verse { get { return m_verse; } }
		public ScriptAnnotation Annotation { get { return m_annotation; } }
		public int Offset { get { return m_offset; } }
	}
}
