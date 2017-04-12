namespace Glyssen.Recording
{
	public class RecordingStats
	{
		public double TotalRecordingLengthInSeconds { get; private set; }
		public int TotalBlocks { get; private set; }
		public int RecordedBlocks { get; private set; }

		public void AddRecording(double seconds)
		{
			TotalRecordingLengthInSeconds += seconds;
			TotalBlocks++;
			RecordedBlocks++;
		}

		public void AddUnrecordedBlock()
		{
			TotalBlocks++;
		}
	}
}
