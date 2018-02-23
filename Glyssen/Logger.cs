using System;
using System.Xml;
using SIL.Scripture;

namespace Glyssen
{
	public static class Logger
	{
		public static void WriteEvent(string exMessage)
		{
			throw new System.NotImplementedException();
		}

		public static void WriteError(InvalidVersificationLineException invalidVersificationLineException)
		{
			throw new System.NotImplementedException();
		}

		internal static void WriteError(Exception exRestoreBackup)
		{
			throw new NotImplementedException();
		}

		internal static void WriteError(string msg, XmlException e)
		{
			throw new NotImplementedException();
		}

		public static void WriteMinorEvent(string s)
		{
			throw new NotImplementedException();
		}

		internal static void WriteError(string v, Exception exMakeBackup)
		{
			throw new NotImplementedException();
		}

		internal static void WriteEvent(string v, BookBlockIndices currentIndices, BookBlockIndices indicesOfNewOrModifiedBlock)
		{
			throw new NotImplementedException();
		}

		internal static void WriteEvent(string v, string bookId)
		{
			throw new NotImplementedException();
		}

		internal static void WriteEvent(string v, string bookId1, string bookId2)
		{
			throw new NotImplementedException();
		}
	}
}