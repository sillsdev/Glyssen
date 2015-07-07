using System.IO;
using System.Text;
using Glyssen.VoiceActor;

namespace Glyssen
{
	public class ProjectExport
	{
		private const string Separator = "\t";

		private readonly Project m_project;
		private readonly bool m_includeVoiceActors;

		public ProjectExport(Project project, bool includeVoiceActors = true)
		{
			m_project = project;
			m_includeVoiceActors = includeVoiceActors;
		}

		public void GenerateFile(string path)
		{
			int blockNumber = 1;
			using (var stream = new StreamWriter(path, false, Encoding.UTF8))
			{
				foreach (var book in m_project.IncludedBooks)
				{
					foreach (var block in book.GetScriptBlocks(true))
					{
						if (m_includeVoiceActors)
						{
							VoiceActor.VoiceActor voiceActor = m_project.GetVoiceActorForCharacter(block.CharacterId);
							string voiceActorName = voiceActor != null ? voiceActor.Name : null;
							stream.WriteLine(GetExportLineForBlock(block, blockNumber++, book.BookId, voiceActorName ?? ""));
						}
						else
							stream.WriteLine(GetExportLineForBlock(block, blockNumber++, book.BookId));
					}
				}
			}
		}

		public static string GetExportLineForBlock(Block block, int blockNumber, string bookId, string voiceActor = null)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(blockNumber);
			builder.Append(Separator);
			if (voiceActor != null)
			{
				builder.Append(voiceActor);
				builder.Append(Separator);
			}
			builder.Append(block.StyleTag);
			builder.Append(Separator);
			builder.Append(bookId);
			builder.Append(Separator);
			builder.Append(block.ChapterNumber);
			builder.Append(Separator);
			builder.Append(block.InitialStartVerseNumber);
			builder.Append(Separator);
			builder.Append(block.CharacterId);
			builder.Append(Separator);
			builder.Append(block.Delivery);
			builder.Append(Separator);
			builder.Append(block.GetText(true));
			builder.Append(Separator);
			builder.Append(block.GetText(false).Length);
			return builder.ToString();
		}
	}
}
