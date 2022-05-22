using System;
using UnityEditor.IMGUI.Controls;

namespace KVD.Profiler.Editor
{
	[Serializable]
	public class ProfilerTreeViewState : TreeViewState
	{
		public readonly ProfilerData data;
		
		public ProfilerTreeViewState(ProfilerData data)
		{
			this.data = data;
		}
	}
}
