using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using L10NSharp;
using SIL.Scripture;

namespace Glyssen.Utilities
{
	internal class L10N
	{
		public static void LocalizeComboList(Component comboBox, string localizationId)
		{
			if (!(comboBox is ComboBox) && !(comboBox is ToolStripComboBox))
			{
				throw new ArgumentException(@"The control must be type ComboBox or ToolStripComboBox", "comboBox");
			}

			var items = GetItems(comboBox);
			if (items == null) return;

			for (var i = 0; i < items.Count; i++)
			{
				var itemLocId = localizationId + "[" + i + "]";
				var item = items[i] as LocalizedListItem;
				if (item == null)
				{
					// this is for the first time, when the item text is the English string
					items[i] = new LocalizedListItem(items[i].ToString(), itemLocId);
				}
				else
				{
					// this is for subsequent times, when the item text may not be English
					item.Localize();
				}
			}
		}

		private static IList GetItems(Component ctrl)
		{
			var type = ctrl.GetType();
			var info = type.GetProperty("Items");

			if (info == null) return null;

			return (IList)info.GetValue(ctrl);
		}

		public static Func<string, string> GetLocalizedBookNameFunc(Func<string, string> getEnglishBookName)
		{
			return (id) =>
			{
				var englishName = getEnglishBookName(id);
				// Prevent attempting to look up dynamic strings for non-canonical books (which should never actually appear in the Glyssen UI)
				if (!String.IsNullOrEmpty(englishName) && BCVRef.BookToNumber(id) > 0)
					return LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, "Common.BookName." + id, englishName);
				return englishName;
			};
		}
	}

	internal class LocalizedListItem
	{
		private readonly string m_englishText;
		private string m_localizedText;
		private readonly string m_localizationId;

		public LocalizedListItem(string englishText, string localizationId)
		{
			m_englishText = englishText;
			m_localizationId = localizationId;
			Localize();
		}

		public void Localize()
		{
			m_localizedText = LocalizationManager.GetDynamicString(GlyssenInfo.kApplicationId, m_localizationId, m_englishText);
		}

		public override string ToString()
		{
			return m_localizedText;
		}
	}
}
