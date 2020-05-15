namespace Glyssen.Shared
{
	/// <summary>
	/// The reference text type.
	/// Basically, is it a known, shipped language? Or something else?
	/// </summary>
	public enum ReferenceTextType
	{
		Unknown,
		English,
		//Azeri,
		//French,
		//Indonesian,
		//Portuguese,
		Russian,
		//Spanish,
		//TokPisin,
		Custom
	}

	public static class ReferenceTextTypeExtensions
	{
		public static bool IsStandard(this ReferenceTextType type)
		{
			return (type != ReferenceTextType.Custom && type != ReferenceTextType.Unknown);
		}
	}
}
