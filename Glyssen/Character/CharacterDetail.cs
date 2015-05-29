namespace Glyssen.Character
{
	public class CharacterDetail
	{
		public string Character { get; set; }
		public bool MultipleSpeakers { get; set; }
		public string Gender { get; set; } //Review: should gender and age be enums?
		public string Age { get; set; }
		public string Comment { get; set; }
	}
}
