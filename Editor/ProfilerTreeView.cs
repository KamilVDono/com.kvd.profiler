using System.Collections.Generic;
using KVD.Utils.DataStructures;
using UnityEditor.IMGUI.Controls;

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

			// Replace Dictionary
			var samplesByMarker = new DistinctMultiDictionary<int, int>();
			for (var index = 1; index < data.MarkerIds.Length; index++)
			{
				var markerId = data.MarkerIds[index];
				if (_blacklistMarkerIds.Contains(markerId))
				{
					continue;
				}
				samplesByMarker.Add(markerId, index);
			}

			root.children = new(samplesByMarker.Count);
			foreach (var samples in samplesByMarker)
			{
				AddItem(root, samples.Key, samples.Value, rootDepth, data);
			}
			
			SortChildren(root);

			return root;
		}

		private void AddItem(
			TreeViewItem parent, int markerId, HashSet<int> markerOccurrences, int parentDepth, ProfilerData data)
		{
			var myDepth = parentDepth+1;
			var item    = CreateTreeItem(markerId, markerOccurrences, myDepth, data);
			parent.AddChild(item);

			// Replace Dictionary
			var parents = new DistinctMultiDictionary<int, int>(4);
			foreach (var occurrenceId in markerOccurrences)
			{
				var parentId = data.ParentIds[occurrenceId];
				if (parentId < 1)
				{
					continue;
				}
				var parentMarkerId = data.MarkerIds[parentId];
				if (_blacklistMarkerIds.Contains(parentMarkerId))
				{
					continue;
				}
				parents.Add(parentMarkerId, parentId);
			}
			item.children = new(parents.Count);
			foreach (var (parentMarkerId, parentIndices) in parents)
			{
				AddItem(item, parentMarkerId, parentIndices, myDepth, data);
			}
			SortChildren(item);
		}

		private ProfilerTreeItem CreateTreeItem(int marker, HashSet<int> indices, int depth, ProfilerData data)
		{
			var    itemName  = data.NameById[marker];
			var    ownTime   = 0f;
			var    totalTime = 0f;
			var    ownGc     = 0f;
			var    totalGc   = 0f;
			ushort calls     = 0;

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
				calls     += data.Calls[index];

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
				ownTime   = ownTime,
				totalTime = totalTime,
				ownGc     = ownGc,
				totalGc   = totalGc,
				calls     = calls,
				depth     = depth,
				avgOwnTime = ownTime / indices.Count,
				minOwnTime = minOwnTime,
				maxOwnTime = maxOwnTime,
				avgOwnGc = ownGc / indices.Count,
				minOwnGc = minOwnGc,
				maxOwnGc = maxOwnGc,
			};
			return item;
		}
	}
}
