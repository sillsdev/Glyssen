namespace GlyssenCharacters
{
	public interface ICharacter
	{
		bool IsNarrator { get; }
		string LocalizedDisplay { get; }
		string CharacterId { get; }
	}
}
