using SIL.Scripture;
using SIL.Windows.Forms.Scripture;

namespace GlyssenApp.Utilities
{
	public static class VerseRefExtensions
	{
		/// <summary>
		/// Sends a "Santa Fe" focus message which can be used by other applications (such as Paratext)
		/// to navigate to the same Scripture reference
		/// </summary>
		/// <param name="currRef"></param>
		public static void SendScrReference(this VerseRef currRef)
		{
			if (currRef != null && currRef.Valid)
			{
				currRef.ChangeVersification(ScrVers.English);
				SantaFeFocusMessageHandler.SendFocusMessage(currRef.ToString());
			}
		}
	}
}
