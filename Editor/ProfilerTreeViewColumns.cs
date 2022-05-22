using UnityEditor.IMGUI.Controls;

namespace KVD.Profiler.Editor
{
	public partial class ProfilerTreeView
	{
		private const int NameColumnIndex = 0;
		private const int CallsColumnIndex = 1;
		private const int OwnTimeColumnIndex = 2;
		private const int TotalTimeColumnIndex = 3;
		private const int OwnGcColumnIndex = 4;
		private const int TotalGcColumnIndex = 5;
		private const int AvgOwnTimeColumnIndex = 6;
		private const int MinOwnTimeColumnIndex = 7;
		private const int MaxOwnTimeColumnIndex = 8;
		private const int AvgOwnGcColumnIndex = 9;
		private const int MinOwnGcColumnIndex = 10;
		private const int MaxOwnGcColumnIndex = 11;
		
		public static MultiColumnHeader CreateColumnsState()
		{
			var headersState = new MultiColumnHeaderState(new[]
			{
				new MultiColumnHeaderState.Column
				{
					headerContent = new("Name"), canSort = false, autoResize = true, width = 500,
				},
				new MultiColumnHeaderState.Column { headerContent = new("Calls"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Own time"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Total time"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Own gc"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Total gc"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Avg own time"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Min own time"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Max own time"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Avg own gc"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Min own gc"), autoResize = true, width = 100, },
				new MultiColumnHeaderState.Column { headerContent = new("Max own gc"), autoResize = true, width = 100, },
			});
			return new(headersState);
		}
	}
}
