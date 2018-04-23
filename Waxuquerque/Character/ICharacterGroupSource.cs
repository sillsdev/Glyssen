namespace Waxuquerque.Character
{
	public interface ICharacterGroupSource
	{
		CharacterGroupTemplate GetTemplate(int numberOfActors);
	}
}
