using System;
using UnityEngine;

namespace KVD.Profiler.Editor
{
	public partial class ProfilerTreeView
	{
		protected override void RowGUI(RowGUIArgs args)
		{
			if (multiColumnHeader == null)
			{
				base.RowGUI(args);
				return;
			}
			
			var item = (ProfilerTreeItem)args.item;

			for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
			{
				CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
			}
		}
		
		private void CellGUI(Rect rect, ProfilerTreeItem item, int column, ref RowGUIArgs args)
		{
			if (column == NameColumnIndex)
			{
				base.RowGUI(args);
			}
			else if (column == CallsColumnIndex)
			{
				GUI.Label(rect, item.calls.ToString());
			}
			else if (column == OwnTimeColumnIndex)
			{
				GUI.Label(rect, $"{item.ownTime:f5}ms");
			}
			else if (column == TotalTimeColumnIndex)
			{
				GUI.Label(rect, $"{item.totalTime:f5}ms");
			}
			else if (column == OwnGcColumnIndex)
			{
				GUI.Label(rect, BytesToString(item.ownGc));
			}
			else if (column == TotalGcColumnIndex)
			{
				GUI.Label(rect, BytesToString(item.totalGc));
			}
			else if (column == AvgOwnTimeColumnIndex)
			{
				GUI.Label(rect, $"{item.avgOwnTime:f5}ms");
			}
			else if (column == MinOwnTimeColumnIndex)
			{
				GUI.Label(rect, $"{item.minOwnTime:f5}ms");
			}
			else if (column == MaxOwnTimeColumnIndex)
			{
				GUI.Label(rect, $"{item.maxOwnTime:f5}ms");
			}
			else if (column == AvgOwnGcColumnIndex)
			{
				GUI.Label(rect, BytesToString(item.avgOwnGc));
			}
			else if (column == MinOwnGcColumnIndex)
			{
				GUI.Label(rect, BytesToString(item.minOwnGc));
			}
			else if (column == MaxOwnGcColumnIndex)
			{
				GUI.Label(rect, BytesToString(item.maxOwnGc));
			}
		}

		private static string BytesToString(double bytes)
		{
			const string zeroString = "0B"; 
			string[]     suf        = { "B", "KB", "MB", "GB", "TB", "PB", "EB", };
			if (bytes < 1)
			{
				return zeroString;
			}
			var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
			var num   = Math.Round(bytes / Math.Pow(1024, place), 2);
			return num + suf[place];
		}
	}
}
