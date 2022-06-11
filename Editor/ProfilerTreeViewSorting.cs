using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

// ReSharper disable PossibleNullReferenceException

namespace KVD.Profiler.Editor
{
	public partial class ProfilerTreeView
	{
		private TreeViewItem _sortedRoot;
		private void OnSortingChanged(MultiColumnHeader _)
		{
			FullSort(_sortedRoot = rootItem);
			Reload();
			_sortedRoot = null;
		}

		public void FullSort(TreeViewItem item)
		{
			SortChildren(item);
			if ((item.children?.Count ?? 0) > 0)
			{
				foreach (var itemChild in item.children)
				{
					FullSort(itemChild);
				}
			}
		}
		
		private void SortChildren(TreeViewItem item)
		{
			if (multiColumnHeader.sortedColumnIndex < 1)
			{
				return;
			}

			var itemChildren = item.children;
			if (itemChildren == null || itemChildren.Count < 2)
			{
				return;
			}

			if (multiColumnHeader.sortedColumnIndex == CallsColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(CallsComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(CallsComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == OwnTimeColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(OwnTimeComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(OwnTimeComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == OwnGcColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(OwnGcComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(OwnGcComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == TotalGcColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(TotalGcComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(TotalGcComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == TotalTimeColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(TotalTimeComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(TotalTimeComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == AvgOwnTimeColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(AvgOwnTimeComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(AvgOwnTimeComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == MinOwnTimeColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(MinOwnTimeComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(MinOwnTimeComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == MaxOwnTimeColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(MaxOwnTimeComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(MaxOwnTimeComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == AvgOwnGcColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(AvgOwnGcComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(AvgOwnGcComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == MinOwnGcColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(MinOwnGcComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(MinOwnGcComparisonDesc.Comparer);
				}
			}
			else if (multiColumnHeader.sortedColumnIndex == MaxOwnGcColumnIndex)
			{
				if (multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex))
				{
					itemChildren.Sort(MaxOwnGcComparisonAsc.Comparer);
				}
				else
				{
					itemChildren.Sort(MaxOwnGcComparisonDesc.Comparer);
				}
			}
		}

		private class OwnTimeComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly OwnTimeComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).ownTime.CompareTo(((ProfilerTreeItem)y).ownTime);
			}
		}
		
		private class OwnTimeComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly OwnTimeComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).ownTime.CompareTo(((ProfilerTreeItem)x).ownTime);
			}
		}
		
		private class TotalGcComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly TotalGcComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).totalGc.CompareTo(((ProfilerTreeItem)y).totalGc);
			}
		}
		
		private class TotalGcComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly TotalGcComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).totalGc.CompareTo(((ProfilerTreeItem)x).totalGc);
			}
		}
		
		private class CallsComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly CallsComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).calls.CompareTo(((ProfilerTreeItem)y).calls);
			}
		}
		
		private class CallsComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly CallsComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).calls.CompareTo(((ProfilerTreeItem)x).calls);
			}
		}
		
		private class TotalTimeComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly TotalTimeComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).totalTime.CompareTo(((ProfilerTreeItem)y).totalTime);
			}
		}
		
		private class TotalTimeComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly TotalTimeComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).totalTime.CompareTo(((ProfilerTreeItem)x).totalTime);
			}
		}
		
		private class OwnGcComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly OwnGcComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).ownGc.CompareTo(((ProfilerTreeItem)y).ownGc);
			}
		}
		
		private class OwnGcComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly OwnGcComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).ownGc.CompareTo(((ProfilerTreeItem)x).ownGc);
			}
		}
		
		private class AvgOwnTimeComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly AvgOwnTimeComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).avgOwnTime.CompareTo(((ProfilerTreeItem)y).avgOwnTime);
			}
		}
		
		private class AvgOwnTimeComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly AvgOwnTimeComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).avgOwnTime.CompareTo(((ProfilerTreeItem)x).avgOwnTime);
			}
		}
		
		private class MinOwnTimeComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly MinOwnTimeComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).minOwnTime.CompareTo(((ProfilerTreeItem)y).minOwnTime);
			}
		}
		
		private class MinOwnTimeComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly MinOwnTimeComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).minOwnTime.CompareTo(((ProfilerTreeItem)x).minOwnTime);
			}
		}
		
		private class MaxOwnTimeComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly MaxOwnTimeComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).maxOwnTime.CompareTo(((ProfilerTreeItem)y).maxOwnTime);
			}
		}
		
		private class MaxOwnTimeComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly MaxOwnTimeComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).maxOwnTime.CompareTo(((ProfilerTreeItem)x).maxOwnTime);
			}
		}

		private class AvgOwnGcComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly AvgOwnGcComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).avgOwnGc.CompareTo(((ProfilerTreeItem)y).avgOwnGc);
			}
		}
		
		private class AvgOwnGcComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly AvgOwnGcComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).avgOwnGc.CompareTo(((ProfilerTreeItem)x).avgOwnGc);
			}
		}
		
		private class MinOwnGcComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly MinOwnGcComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).minOwnGc.CompareTo(((ProfilerTreeItem)y).minOwnGc);
			}
		}
		
		private class MinOwnGcComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly MinOwnGcComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).minOwnGc.CompareTo(((ProfilerTreeItem)x).minOwnGc);
			}
		}
		
		private class MaxOwnGcComparisonAsc : IComparer<TreeViewItem>
		{
			public static readonly MaxOwnGcComparisonAsc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)x).maxOwnGc.CompareTo(((ProfilerTreeItem)y).maxOwnGc);
			}
		}
		
		private class MaxOwnGcComparisonDesc : IComparer<TreeViewItem>
		{
			public static readonly MaxOwnGcComparisonDesc Comparer = new();
			
			public int Compare(TreeViewItem x, TreeViewItem y)
			{
				return ((ProfilerTreeItem)y).maxOwnGc.CompareTo(((ProfilerTreeItem)x).maxOwnGc);
			}
		}
	}
}
