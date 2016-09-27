﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
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

	public interface IVerse
	{
		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// starting number in the bridge.
		/// </summary>
		int StartVerse { get; }
		
		/// <summary>
		/// Gets the verse number as an integer. If the Verse number represents a verse bridge, this will be the
		/// ending number in the bridge.
		/// </summary>
		int EndVerse { get; }

		/// <summary>
		/// If the Verse number represents a verse bridge, this will be the ending number in the bridge; otherwise 0.
		/// </summary>
		int LastVerseOfBridge { get; }
	}

	public class Verse : BlockElement, IVerse
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

		/// <summary>
		/// If the Verse number represents a verse bridge, this will be the ending number in the bridge; otherwise 0.
		/// </summary>
		public int LastVerseOfBridge
		{
			get
			{
				var endVerse = EndVerse;
				var startVerse = StartVerse;
				return endVerse == startVerse ? 0 : endVerse;
			}
		}
	}

	[XmlInclude(typeof(Pause))]
	[XmlInclude(typeof(Sound))]
	public abstract class ScriptAnnotation : BlockElement
	{
		public abstract string ToDisplay(string elementSeparator = " ");
	}

	public class Pause : ScriptAnnotation
	{
		public const string kPauseSecondsFormat = "||| + {0} SECs |||";

		[XmlAttribute("timeUnits")]
		[DefaultValue(TimeUnits.Seconds)]
		public TimeUnits TimeUnits { get; set; }

		[XmlAttribute("time")]
		public double Time { get; set; }

		public override string ToDisplay(string elementSeparator = " ")
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
		private const string kDetailSeparatorDisplayString = "--";
		private const string kStartDisplayString = "Starts";
		private const string kMusic = "Music";
		private const string kSfx = "SFX";
		public const int kNonSpecificStartOrStop = -999;
		public const string kRegexForUserLocatedSounds = @"((\u00A0| )*\{F8 (?<musicOrSfx>(" + kSfx + ")|(" + kMusic + "))" + kDetailSeparatorDisplayString + @"(?<effectDetails>.*)\})";

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

		public static Sound CreateFromMatchedRegex(Match match)
		{
			if (!match.Groups["musicOrSfx"].Success)
			{
				throw new ArgumentException(
					"This method is designed to work only with a Regex Match from a regular expression built using kRegexForUserLocatedSounds",
					"match");
			}
			var type = match.Result("${musicOrSfx}");
			var details = match.Result("${effectDetails}");

			if (type == kSfx)
				return new Sound {EffectName = details, SoundType = SoundType.Sfx, UserSpecifiesLocation = true};
			if (details == kStartDisplayString)
				return new Sound { SoundType = SoundType.Music, StartVerse = kNonSpecificStartOrStop, UserSpecifiesLocation = true};
			return new Sound { SoundType = SoundType.Music, UserSpecifiesLocation = true };
		}

		private string ToDisplay(StringBuilder stringBuilderToAppendTo, string details)
		{
			stringBuilderToAppendTo.Append("{");
			if (UserSpecifiesLocation)
				stringBuilderToAppendTo.Append("F8 ");

			switch (SoundType)
			{
				case SoundType.Music: stringBuilderToAppendTo.Append(kMusic); break;
				case SoundType.MusicSfx:
					stringBuilderToAppendTo.Append(kMusic);
					stringBuilderToAppendTo.Append(" + ");
					stringBuilderToAppendTo.Append(kSfx);
					break;
				case SoundType.Sfx: stringBuilderToAppendTo.Append(kSfx); break;
				default: throw new InvalidEnumArgumentException("type", (int)SoundType, typeof(SoundType));
			}

			if (details != null)
			{
				stringBuilderToAppendTo.Append(kDetailSeparatorDisplayString);
				stringBuilderToAppendTo.Append(details);
			}
			stringBuilderToAppendTo.Append("}");
			if (UserSpecifiesLocation)
				stringBuilderToAppendTo.Append(" ");

			return stringBuilderToAppendTo.ToString();
		}

		public override string ToDisplay(string elementSeparator = " ")
		{
			const string kEndDisplayString = "Ends";
		
			string details = null;
			var sb = new StringBuilder();

			if (UserSpecifiesLocation)
			{
				switch (SoundType)
				{
					case SoundType.Music:
						details = StartVerse == kNonSpecificStartOrStop ? kStartDisplayString : kEndDisplayString;
						break;
					case SoundType.Sfx:
						if (!string.IsNullOrEmpty(EffectName))
							details = EffectName;
						break;
					default:
						Debug.Fail("No code for displaying this annotation: " + ToString());
						return string.Empty;
				}
			}
			else
			{
				sb.Append(kDoNotCombine);
				sb.Append(elementSeparator);

				switch (SoundType)
				{
					case SoundType.Music:
						if (StartVerse > 0)
						{
							if (EndVerse == kNonSpecificStartOrStop)
							{
								details = String.Format("{0} & New Music{1}{2} @ v{3}",
									kEndDisplayString, kDetailSeparatorDisplayString, kStartDisplayString, StartVerse);
							}
							else
								details = String.Format("{0} @ v{1}", kStartDisplayString, StartVerse);
						}
						else if (EndVerse > 0)
							details = String.Format("{0} before v{1}", kEndDisplayString, EndVerse);
						else
							goto default;
						break;
					case SoundType.Sfx:
						if (StartVerse != 0)
						{
							details = EndVerse != 0 ?
								String.Format("{0} @ v{1}-{2}", EffectName, StartVerse, EndVerse) :
								String.Format("{0}{1}{2} @ v{3}", EffectName, kDetailSeparatorDisplayString, kStartDisplayString, StartVerse);
						}
						else if (EndVerse != 0)
						{
							details = !string.IsNullOrEmpty(EffectName) ?
								String.Format("{0}{1}{2} before v{3}", EffectName, kDetailSeparatorDisplayString, kEndDisplayString, EndVerse) :
								String.Format("{0} before v{1}", kEndDisplayString, EndVerse);
						}
						else
							goto default;
						break;
					case SoundType.MusicSfx:
						details = String.Format("{0} Start @ v{1}", EffectName, StartVerse);
						break;
					default:
						Debug.Fail("No code for displaying this annotation: " + ToString());
						return string.Empty;
				}
			}
			return ToDisplay(sb, details);
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
