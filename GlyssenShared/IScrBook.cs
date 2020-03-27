
namespace Glyssen.Shared
{
	public interface IScrBook
	{
		string BookId { get; }
		string GetVerseText(int chapter, int verse);
	}
}
