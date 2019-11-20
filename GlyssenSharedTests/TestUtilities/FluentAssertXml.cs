// Glyssen has been updated to use NUnit 3.12 and is no longer compatible with the older
// version of NUnit that is used by libpalaso. In order to pass unit testing, this file
// was created. It is an exact copy of the file libpalaso\SIL.TestUtilities\FluentAssertXml.cs
// with the exception of the static constructor. This is a temporary solution until 
// libpalaso is updated to use NUnit 3.12. At that time the entire
// GlyssenSharedTests.TestUtilities folder should be deleted.  To ensure that this 
// temporary fix doesn't become permanent, the constructor was added and an exception
// will be thrown starting in 2020.  Andrew Christensen

using System;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.PlatformUtilities;
using SIL.TestUtilities;
using SIL.Xml;

namespace GlyssenSharedTests.TestUtilities
{
	//NB: if c# ever allows us to add static exension methods,
	//then all this could be an extension on nunit's Assert class.

	public class AssertThatXmlIn
	{
		public static AssertDom Dom(XmlDocument dom)
		{
			return new AssertDom(dom);
		}
		public static AssertFile File(string path)
		{
			return new AssertFile(path);
		}
		public static AssertXmlString String(string xmlString)
		{
			return new AssertXmlString(xmlString);
		}
	}

	public class AssertXmlString : AssertXmlCommands
	{
		private readonly string _xmlString;

        static AssertXmlString()
        {
            // Please see the comment at the top of this file. This code exists to ensure that libpalaso is updated to use NUnit 3.12 by January, 2020.
            // Once that is done, this entire file should be deleted.  Andrew Christensen
            if (DateTime.Now.Year == 2020)
                throw new NotSupportedException("The deadline has passed for libpalaso to be upgraded to use NUnit 3.12.");
        }

        public AssertXmlString(string xmlString)
		{
			_xmlString = xmlString;
		}

		protected override XmlNode NodeOrDom
		{
			get
			{
				var dom = new XmlDocument();
				dom.LoadXml(_xmlString);
				return dom;
			}
		}

		/// <summary>
		/// Assert functional equivalence of two XML strings while ignoring whitespace
		///
		/// May fail if XML header is different.  Also, see warning in CanonicalXml.
		/// </summary>
		/// <param name="xml"></param>
		public void EqualsIgnoreWhitespace(string xml)
		{
            Assert.AreEqual(CanonicalXml.ToCanonicalString(_xmlString), CanonicalXml.ToCanonicalString(xml));
		}

		/// <summary>
		/// Assert functional inequivalence of two XML strings while ignoring whitespace
		/// </summary>
		/// <param name="xml"></param>
		public void NotEqualsIgnoreWhitespace(string xml)
		{
			Assert.AreNotEqual(CanonicalXml.ToCanonicalString(_xmlString), CanonicalXml.ToCanonicalString(xml));
		}

		/// <summary>
		/// Assert node-by-node equivalence of two XML strings
		/// This will ignore irrelevant whitespace.
		///
		/// The only known difference between this method and EqualsIgnoreWhitespace
		/// is this completely ignores the encoding set in the header.
		/// However, given the difference of implementation, other differences could exist.
		///
		/// Additionally, the output of a failed test is much less useful here than
		/// the character-by-character comparison achieved by EqualsIgnoreWhitespace.
		/// </summary>
		/// <param name="xml"></param>
		public void IsNodewiseEqualTo(string xml)
		{
			Assert.IsTrue(NodeWiseEquals(xml),
				String.Format("{0}\n\nis not nodewise equivalent to\n\n{1}", _xmlString, xml));
		}

		/// <summary>
		/// Assert node-by-node inequivalence of two XML strings
		/// </summary>
		/// <param name="xml"></param>
		public void IsNotNodewiseEqualTo(string xml)
		{
			Assert.IsFalse(NodeWiseEquals(xml),
				String.Format("{0}\n\nis nodewise equivalent to\n\n{1}", _xmlString, xml));
		}

		private bool NodeWiseEquals(string xml)
		{
			return XNode.DeepEquals(XElement.Parse(_xmlString), XElement.Parse(xml));
		}
	}

	public class AssertFile : AssertXmlCommands
	{
		private readonly string _path;

		public AssertFile(string path)
		{
			_path = path;
		}

		protected override XmlNode NodeOrDom
		{
			get
			{
				var dom = new XmlDocument();
				dom.Load(_path);
				return dom;
			}
		}
	}

	public class AssertDom : AssertXmlCommands
	{
		private readonly XmlDocument _dom;

		public AssertDom(XmlDocument dom)
		{
			_dom = dom;
		}

		protected override XmlNode NodeOrDom
		{
			get
			{
				return _dom;
			}
		}
	}

	public abstract class AssertXmlCommands
	{
		protected abstract XmlNode NodeOrDom { get; }

		public void HasAtLeastOneMatchForXpath(string xpath, XmlNamespaceManager nameSpaceManager)
		{
			XmlNode node = GetNode(xpath, nameSpaceManager);
			if (node == null)
			{
				Console.WriteLine("Could not match " + xpath);
				PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNotNull(node, "Not matched: " + xpath);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public  void HasAtLeastOneMatchForXpath(string xpath)
		{
			XmlNode node = GetNode(xpath);
			if (node == null)
			{
				Console.WriteLine("Could not match " + xpath);
				PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNotNull(node, "Not matched: " + xpath);
		}

		/// <summary>
		/// Will honor default namespace
		/// </summary>
		public void HasSpecifiedNumberOfMatchesForXpath(string xpath, int count, XmlNamespaceManager nameSpaceManager = null)
		{
			HasSpecifiedNumberOfMatchesForXpath(xpath, count, true, nameSpaceManager);
		}

		/// <summary>
        /// Will honor default namespace
        /// </summary>
        public void HasSpecifiedNumberOfMatchesForXpath(string xpath, int count, bool verbose, XmlNamespaceManager nameSpaceManager = null)
		{
			var nodes = nameSpaceManager == null ? NodeOrDom.SafeSelectNodes(xpath) : NodeOrDom.SafeSelectNodes(xpath, nameSpaceManager);
			if (nodes==null)
			{
				if (count > 0)
				{
					Console.WriteLine("Expected {0} but got 0 matches for {1}", count,  xpath);
					if (verbose)
						PrintNodeToConsole(NodeOrDom);
				}
				Assert.AreEqual(count, 0);
			}
			else if (nodes.Count != count)
			{
				Console.WriteLine("Expected {0} but got {1} matches for {2}", count, nodes.Count, xpath);
				if (verbose)
					PrintNodeToConsole(NodeOrDom);
				Assert.AreEqual(count, nodes.Count, "matches for " + xpath);
			}
		}

		public static void PrintNodeToConsole(XmlNode node)
		{
			var settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;
			using (XmlWriter writer = XmlWriter.Create(Console.Out, settings))
			{
				node.WriteContentTo(writer);
				writer.Flush();
				Console.WriteLine();
			}
		}

		public void HasNoMatchForXpath(string xpath, XmlNamespaceManager nameSpaceManager = null, string message = null, bool print = true)
		{
			if (nameSpaceManager == null)
			{
				nameSpaceManager = new XmlNamespaceManager(new NameTable());
			}
			var node = GetNode(xpath, nameSpaceManager);
			if (node != null)
			{
				if (message != null)
					Console.WriteLine(message);
				Console.WriteLine(@"Was not supposed to match " + xpath);
				if (print)
					PrintNodeToConsole(NodeOrDom);
			}
			Assert.IsNull(node, "Should not have matched: {0}{1}{2}", xpath, Environment.NewLine, message);
		}

		private XmlNode GetNode(string xpath)
		{
			// Mono: Currently the method XmlNodeExtensions.GetPrefixedPath doesn't allow for / in a literal string
			return Platform.IsWindows
				? NodeOrDom.SelectSingleNodeHonoringDefaultNS(xpath)
				: NodeOrDom.SelectSingleNode(xpath);
		}

		private XmlNode GetNode(string xpath, XmlNamespaceManager nameSpaceManager)
		{
			return NodeOrDom.SelectSingleNode(xpath, nameSpaceManager);
		}
	}
}