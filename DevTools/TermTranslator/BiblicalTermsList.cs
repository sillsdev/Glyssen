using System.Xml.Serialization;

namespace DevTools.TermTranslator
{
	[XmlRoot("BiblicalTermsLocalizations")]
	public sealed class BiblicalTermsLocalizations
	{
		[XmlElement("Terms")]
		public Terms Terms;
	}
}
