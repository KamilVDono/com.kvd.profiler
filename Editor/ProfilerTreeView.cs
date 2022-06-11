using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using KVD.Utils.DataStructures;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace KVD.Profiler.Editor
{
	public partial class ProfilerTreeView : TreeView
	{
		private readonly HashSet<int> _blacklistMarkerIds;

		public ProfilerTreeView(
			ProfilerTreeViewState state, MultiColumnHeader multiColumnHeader, HashSet<int> blacklistMarkerIds)
			: base(state, multiColumnHeader)
		{
			_blacklistMarkerIds              =  blacklistMarkerIds;
			showAlternatingRowBackgrounds    =  true;
			showBorder                       =  true;
			multiColumnHeader.sortingChanged += OnSortingChanged;
			Reload();
		}

		protected override TreeViewItem BuildRoot()
		{
			if (_sortedRoot != null)
			{
				return _sortedRoot;
			}
			
			const int rootIndex = 0;
			const int rootDepth = -1;

			var profilerState = (ProfilerTreeViewState)state;
			var data          = profilerState.data;
			var root          = new TreeViewItem(rootIndex, rootDepth);
			
			Parallel.For(0, data.SamplesByMarkers.Length, i =>
			{
				var samples = data.SamplesByMarkers[i];
				if ((samples?.Count ?? 0) >= 1)
				{
					ConcurrentAddItem(root, i, samples, rootDepth, data);
				}
			});

			SortChildren(root);

			return root;
		}

		private void ConcurrentAddItem(TreeViewItem parent, int markerId,
			List<int> markerOccurrences, int parentDepth, ProfilerData data, SimplePool<List<int>> pool = null)
		{
			var myDepth = parentDepth+1;
			var item    = CreateTreeItem(markerId, markerOccurrences, myDepth, data);
			parent.AddChild(item);
			
			pool ??= new(10, () => new(24));

			// Replace Dictionary
			using var markersBorrow = pool.Borrow();
			var       markers       = markersBorrow.Element;
			markers.Clear();

			foreach (var occurrenceId in markerOccurrences)
			{
				var parentId       = data.ParentIds[occurrenceId];
				var parentMarkerId = data.MarkerIds[parentId];
				if (_blacklistMarkerIds.Contains(parentMarkerId))
				{
					continue;
				}

				if (markers.Contains(parentMarkerId))
				{
					continue;
				}
				markers.Add(parentMarkerId);
			}

			using var parentsBorrow = pool.Borrow();
			var       parents       = parentsBorrow.Element;
			for (var i = 0; i < markers.Count; i++)
			{
				var marker = markers[i];
				parents.Clear();
				
				foreach (var occurrenceId in markerOccurrences)
				{
					var parentId       = data.ParentIds[occurrenceId];
					var parentMarkerId = data.MarkerIds[parentId];
					if (marker == parentMarkerId && !parents.Contains(parentId))
					{
						parents.Add(parentId);
					}
				}
				
				ConcurrentAddItem(item, marker, parents, myDepth, data, pool);
			}

			SortChildren(item);
		}

		private ProfilerTreeItem CreateTreeItem(int marker, List<int> indices, int depth, ProfilerData data)
		{
			var    itemName  = data.NameById[marker];
			var    ownTime   = 0f;
			var    totalTime = 0f;
			var    ownGc     = 0f;
			var    totalGc   = 0f;

			var minOwnTime = float.MaxValue;
			var maxOwnTime = 0f;
			
			var minOwnGc = float.MaxValue;
			var maxOwnGc = 0f;

			foreach (var index in indices)
			{
				var currentOwnTime = data.OwnTimes[index];
				var currentOwnGc = data.OwnGcAllocs[index];
				
				ownTime   += currentOwnTime;
				totalTime += data.TotalTimes[index];
				ownGc     += currentOwnGc;
				totalGc   += data.TotalGcAllocs[index];

				if (currentOwnTime < minOwnTime)
				{
					minOwnTime = currentOwnTime;
				}
				if (currentOwnTime > maxOwnTime)
				{
					maxOwnTime = currentOwnTime;
				}
				
				if (currentOwnGc < minOwnGc)
				{
					minOwnGc = currentOwnGc;
				}
				if (currentOwnGc > maxOwnGc)
				{
					maxOwnGc = currentOwnGc;
				}
			}
			var item = new ProfilerTreeItem(marker, itemName)
			{
				ownTime    = ownTime,
				totalTime  = totalTime,
				ownGc      = ownGc,
				totalGc    = totalGc,
				calls      = (ushort)indices.Count,
				depth      = depth,
				avgOwnTime = ownTime / indices.Count,
				minOwnTime = minOwnTime,
				maxOwnTime = maxOwnTime,
				avgOwnGc   = ownGc / indices.Count,
				minOwnGc   = minOwnGc,
				maxOwnGc   = maxOwnGc,
			};
			return item;
		}
	}
}
