using UnityEditor.IMGUI.Controls;

namespace KVD.Profiler.Editor
{
	public sealed class ProfilerTreeItem : TreeViewItem
	{
		public float ownTime;
		public float totalTime;
		public float ownGc;
		public float totalGc;
		public ushort calls;
		public float avgOwnTime;
		public float minOwnTime;
		public float maxOwnTime;
		
		public ProfilerTreeItem(int id, string name)
		{
			this.id = id;

			var colonIndex = name.LastIndexOf(':')+1;
			displayName = colonIndex > 0 ? name[colonIndex..] : name;
		}
	}
}
