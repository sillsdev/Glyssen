using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using SIL.Scripture;

namespace Glyssen
{
	[XmlInclude(typeof(ScriptText))]
	[XmlInclude(typeof(Verse))]
	[XmlInclude(typeof(ScriptAnnotation))]
	public abstract class BlockElement
	{
		public virtual BlockElement Clone()
		{
			return (BlockElement)MemberwiseClone();
		}
	}

	public class BlockElementContentsComparer : IEqualityComparer<BlockElement>
	{
		public bool Equals(BlockElement x, BlockElement y)
		{
			var xAsVerse = x as Verse;
			if (xAsVerse != null)
			{
				var yAsVerse = y as Verse;
				return yAsVerse != null && xAsVerse.Number == yAsVerse.Number;
			}

			var xAsScriptText = x as ScriptText;
			if (xAsScriptText != null)
			{
				var yAsScriptText = y as ScriptText;
				return yAsScriptText != null && xAsScriptText.Content == yAsScriptText.Content;
			}

			var xAsScriptAnnotation = x as ScriptAnnotation;
			if (xAsScriptAnnotation != null)
			{
				var yAsScriptAnnotation = y as ScriptAnnotation;
				return yAsScriptAnnotation != null && xAsScriptAnnotation.Equals(yAsScriptAnnotation);
			}

			return false;
		}

		public int GetHashCode(BlockElement obj)
		{
			return obj.GetHashCode();
		}
	}

	public class ScriptText : BlockElement
	{
		public ScriptText()
		{
			// Needed for deserialization
		}

		public ScriptText(string content)
		{
			Content = content;
		}

		[XmlText]
		public string Content { get; set; }
	}

	public class Verse : BlockElement
	{
		public Verse()
		{
			// Needed for deserialization
		}

		public Verse(string number)
		{
			Number = number;
		}

		[XmlAttribute("num")]
		public string Number { get; set; }

		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// starting number in the bridge.
		/// </summary>
		public int StartVerse
		{
			get { return ScrReference.VerseToIntStart(Number); }
		}

		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// ending number in the bridge.
		/// </summary>
		public int EndVerse
		{
			get { return ScrReference.VerseToIntEnd(Number); }
		}
	}

	[XmlInclude(typeof(Pause))]
	[XmlInclude(typeof(Sound))]
	public abstract class ScriptAnnotation : BlockElement
	{
		public abstract string ToDisplay(string elementSeparator);
	}

	public class Pause : ScriptAnnotation
	{
		public const string kPauseSecondsFormat = "||| + {0} SECs |||";

		[XmlAttribute("timeUnits")]
		[DefaultValue(TimeUnits.Seconds)]
		public TimeUnits TimeUnits { get; set; }

		[XmlAttribute("time")]
		public double Time { get; set; }

		public override string ToDisplay(string elementSeparator)
		{
			if (TimeUnits == TimeUnits.Seconds)
				return string.Format(kPauseSecondsFormat, Time);
			if (Time == 1.0d)
				return "||| + 1 MINUTE |||";
			Debug.Fail("No code for displaying this annotation: " + ToString());
			return string.Empty;
		}

		public override string ToString()
		{
			return string.Format("TimeUnits: {0}, Time: {1}", TimeUnits, Time);
		}

		#region Equality members
		protected bool Equals(Pause other)
		{
			return TimeUnits == other.TimeUnits && Time.Equals(other.Time);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Pause)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)TimeUnits * 397) ^ Time.GetHashCode();
			}
		}
		#endregion
	}

	public class Sound : ScriptAnnotation
	{
		public const string kDoNotCombine = " ||| DO NOT COMBINE |||";
		public const int kNonSpecificStartOrStop = -999;

		[XmlAttribute("soundType")]
		[DefaultValue(SoundType.Music)]
		public SoundType SoundType { get; set; }

		[XmlAttribute("startVerse")]
		[DefaultValue(0)]
		public int StartVerse { get; set; }

		[XmlAttribute("endVerse")]
		[DefaultValue(0)]
		public int EndVerse { get; set; }

		[XmlAttribute("effectName")]
		[DefaultValue(null)]
		public string EffectName { get; set; }

		[XmlAttribute("userSpecifiesLocation")]
		[DefaultValue(false)]
		public bool UserSpecifiesLocation { get; set; }

		public override string ToDisplay(string elementSeparator)
		{
			if (UserSpecifiesLocation)
			{
				switch (SoundType)
				{
					case SoundType.Music:
						if (StartVerse == kNonSpecificStartOrStop)
							return "{F8 Music--Starts} ";
						return "{F8 Music--Ends} ";
					case SoundType.Sfx:
						if (!string.IsNullOrEmpty(EffectName))
							return string.Format("{{F8 SFX--{0}}} ", EffectName);
						return "{F8 SFX}";
					default:
						Debug.Fail("No code for displaying this annotation: " + ToString());
						return string.Empty;
				}
			}
			switch (SoundType)
			{
				case SoundType.Music:
					if (StartVerse > 0 && EndVerse == kNonSpecificStartOrStop)
						return kDoNotCombine + elementSeparator + string.Format("{{Music--Ends & New Music--Starts @ v{0}}}", StartVerse);
					if (StartVerse > 0)
						return kDoNotCombine + elementSeparator + string.Format("{{Music--Starts @ v{0}}}", StartVerse);
					if (EndVerse > 0)
						return kDoNotCombine + elementSeparator + string.Format("{{Music--Ends before v{0}}}", EndVerse);
					goto default;
				case SoundType.Sfx:
					if (StartVerse != 0)
					{
						if (EndVerse != 0)
							return kDoNotCombine + elementSeparator + string.Format("{{SFX--{0} @ v{1}-{2}}}", EffectName, StartVerse, EndVerse);
						return kDoNotCombine + elementSeparator + string.Format("{{SFX--{0}--Starts @ v{1}}}", EffectName, StartVerse);
					}
					if (EndVerse != 0)
					{
						if (!string.IsNullOrEmpty(EffectName))
							return kDoNotCombine + elementSeparator + string.Format("{{SFX--{0}--Ends before v{1}}}", EffectName, EndVerse);
						return kDoNotCombine + elementSeparator + string.Format("{{SFX--Ends before v{0}}}", EndVerse);
					}
					goto default;
				case SoundType.MusicSfx:
					return kDoNotCombine + elementSeparator + string.Format("{{Music + SFX--{0} Start @ v{1}}}", EffectName, StartVerse);
				default:
					Debug.Fail("No code for displaying this annotation: " + ToString());
					return string.Empty;
			}
		}

		public override string ToString()
		{
			return string.Format("SoundType: {0}, StartVerse: {1}, EndVerse: {2}, EffectName: {3}, UserSpecifiesLocation: {4}", SoundType, StartVerse, EndVerse, EffectName, UserSpecifiesLocation);
		}

		#region Equality members
		protected bool Equals(Sound other)
		{
			return SoundType == other.SoundType && StartVerse == other.StartVerse && EndVerse == other.EndVerse && string.Equals(EffectName, other.EffectName) && UserSpecifiesLocation == other.UserSpecifiesLocation;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != GetType())
				return false;
			return Equals((Sound)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)SoundType;
				hashCode = (hashCode * 397) ^ StartVerse;
				hashCode = (hashCode * 397) ^ EndVerse;
				hashCode = (hashCode * 397) ^ (EffectName != null ? EffectName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ UserSpecifiesLocation.GetHashCode();
				return hashCode;
			}
		}
		#endregion
	}

	public enum SoundType
	{
		Music,
		Sfx,
		MusicSfx
	}

	public enum TimeUnits
	{
		Seconds,
		Minutes
	}
}
