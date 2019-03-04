using System.Collections.Generic;
using System.Drawing;
using Glyssen.Character;
using Glyssen.Dialogs;

namespace Glyssen.Utilities
{
	public interface IGlyssenColorScheme
	{
		Color BackColor { get; }
		Color MouseDownBackColor { get; }
		Color MouseOverBackColor { get; }
		Color ForeColor { get; }
		Color LinkColor { get; }
		Color ActiveLinkColor { get; }
		Color DisabledLinkColor { get; }
		Color VisitedLinkColor { get; }
		Color Warning { get; }
		Color Highlight1 { get; }
		Color Highlight2 { get; }
		Color Highlight3 { get; }
		Color SpeechJesus { get; }
		Color SpeechCharacter { get; }
		Color SpeechNonCharacter { get; }

		Color GetMatchColor(int i);
		Color GetForeColorByCharacter(AssignCharacterViewModel.Character character);
		Color GetForeColorByCharacterId(string characterId);
	}

	public class DefaultColorScheme : IGlyssenColorScheme
	{
		public virtual Color BackColor { get { return SystemColors.Control; } }
		public virtual Color MouseDownBackColor { get { return MouseOverBackColor; } }
		public virtual Color MouseOverBackColor { get { return SystemColors.ControlDark; } }
		public virtual Color ForeColor { get { return SystemColors.WindowText; } }
		public virtual Color LinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color ActiveLinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color DisabledLinkColor { get { return Color.FromArgb(133, 133, 133); } }
		public virtual Color VisitedLinkColor { get { return SystemColors.HotTrack; } }
		public virtual Color Warning { get { return Color.Red; } }
		public virtual Color Highlight1 { get { return Color.DarkBlue; } }
		public virtual Color Highlight2 { get { return Color.MediumBlue; } }
		public virtual Color Highlight3 { get { return Color.Blue; } }
		public virtual Color SpeechJesus => Color.DarkRed;
		public virtual Color SpeechCharacter => Color.Blue;
		public virtual Color SpeechNonCharacter => Color.Black;

		public static readonly List<Color> MaxContrastColorList = new List<Color>
		{
			Color.FromArgb(226, 217, 177), //Tan
			Color.FromArgb(190, 177, 226), //Dusty Purple
			Color.FromArgb(226, 186, 177), //Muted Peachy Orange
			Color.FromArgb(199, 226, 177), //Pale green
			Color.FromArgb(177, 226, 226), //Washed-out blue green
			Color.FromArgb(239, 239, 167), //Calming yellow
			Color.FromArgb(219, 167, 239), //Tainted Pepto Bismol
			Color.FromArgb(177, 196, 143), //Drab Faded Olive
			Color.FromArgb(193, 168, 168), //Grayish mauve?
			Color.FromArgb(224, 205, 123), //Bold sandstone
			Color.FromArgb(131, 228, 239), //Baby blue sky
			Color.FromArgb(211, 209, 209), //Basically gray
			Color.FromArgb(156, 255, 147)  //Intoxicated Leprechaun
		};

		public virtual Color GetMatchColor(int i)
		{
			return MaxContrastColorList[i % MaxContrastColorList.Count];
		}

		public virtual Color GetForeColorByCharacter(AssignCharacterViewModel.Character character)
		{
			if (character == null || string.IsNullOrWhiteSpace(character.CharacterId))
				return SpeechNonCharacter;

			if (character.CharacterId== "Jesus")
				return SpeechJesus;

			if (!character.IsStandard)
				return SpeechCharacter;

			return SpeechNonCharacter;
		}

		public virtual Color GetForeColorByCharacterId(string characterId)
		{
			if (string.IsNullOrWhiteSpace(characterId))
				return SpeechNonCharacter;

			if (characterId == "Jesus")
				return SpeechJesus;

			if (!CharacterVerseData.IsCharacterStandard(characterId))
				return SpeechCharacter;

			return SpeechNonCharacter;
		}
	}

	public class TraditionalBlueColorScheme : DefaultColorScheme
	{
		public override Color BackColor { get { return Color.FromArgb(0, 73, 108); } }
		public override Color MouseOverBackColor { get { return Color.FromArgb(0, 93, 128); } }
		public override Color ForeColor { get { return Color.White; } }
		public override Color LinkColor { get { return Color.FromArgb(51, 153, 255); } }
		public override Color ActiveLinkColor { get { return LinkColor; } }
		public override Color VisitedLinkColor { get { return LinkColor; } }
		public override Color Highlight1 { get { return Color.Yellow; } }
		public override Color Highlight2 { get { return Color.LawnGreen; } }
		public override Color Highlight3 { get { return Color.Orange; } }
	}

	public enum GlyssenColors
	{
		Default,
		ForeColor,
		BackColor,
		LinkColor,
		ActiveLinkColor,
		DisabledLinkColor,
		VisitedLinkColor,
		MouseDownBackColor,
		MouseOverBackColor,
		Warning,
	}
}
