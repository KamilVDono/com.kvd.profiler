using System.Collections.Generic;

namespace KVD.Profiler.Editor
{
	public record ProfilerData
	{
		public int[] MarkerIds{ get; set; }
		public int[] ParentIds{ get; set; }
		public int[][] ChildrenIds{ get; set; }
		public float[] OwnTimes{ get; set; }
		public float[] TotalTimes{ get; set; }
		public float[] OwnGcAllocs{ get; set; }
		public float[] TotalGcAllocs{ get; set; }
		public string[] NameById{ get; set; }
		public List<int>[] SamplesByMarkers{ get; set; }
	}
}
