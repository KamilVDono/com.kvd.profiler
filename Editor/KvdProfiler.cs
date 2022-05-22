using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

namespace KVD.Profiler.Editor
{
	public class KvdProfiler : EditorWindow
	{
		private static readonly HashSet<string> BlacklistMarkerNames = new()
		{
			"PlayerLoop", "EditorLoop", "Update.ScriptRunBehaviourUpdate", "FixedUpdate.PhysicsFixedUpdate",
		};

		[SerializeField] private ProfilerTreeViewState _treeState;
		[SerializeField] private int _selectedFrame;
		private ProfilerTreeView _treeView;

		private readonly Dictionary<int, string> _nameById = new();
		private readonly HashSet<int> _blacklistMarkerIds = new();

		private void OnEnable()
		{
			_selectedFrame = ProfilerDriver.lastFrameIndex;
		}

		private void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			_selectedFrame = EditorGUILayout.IntSlider(_selectedFrame, ProfilerDriver.firstFrameIndex-1,
				ProfilerDriver.lastFrameIndex);
			if (EditorGUI.EndChangeCheck())
			{
				CollectData();
			}

			if (_treeView?.GetRows() == null)
			{
				return;
			}

			var lastRect = GUILayoutUtility.GetLastRect();

			_treeView.OnGUI(new(0, lastRect.height+8, position.width, position.height-lastRect.height+8));
		}
	
		private void CollectData()
		{
			_treeView  = null;
			_treeState = null;

			if (_selectedFrame < ProfilerDriver.firstFrameIndex || _selectedFrame > ProfilerDriver.lastFrameIndex)
			{
				return;
			}

			using var frameData = ProfilerDriver.GetHierarchyFrameDataView(_selectedFrame, 0,
				HierarchyFrameDataView.ViewModes.HideEditorOnlySamples, HierarchyFrameDataView.columnDontSort, false);
			if (!frameData.valid)
			{
				return;
			}

			var gcMarkerId = frameData.GetMarkerId("GC.Alloc");
			foreach (var blacklistMarkerName in BlacklistMarkerNames)
			{
				_blacklistMarkerIds.Add(frameData.GetMarkerId(blacklistMarkerName));
			}
		
			var samplesCount = frameData.sampleCount;

			var markerIds     = new int[samplesCount];
			var parentIds     = new int[samplesCount];
			var childrenIds   = new int[samplesCount][];
			var calls         = new ushort[samplesCount];
			var ownTimes      = new float[samplesCount];
			var totalTimes    = new float[samplesCount];
			var totalGcAllocs = new float[samplesCount];
			var ownGcAllocs   = new float[samplesCount];
		
			var children = new List<int>(32);

			var toInvestigate = new Queue<int>();
			toInvestigate.Enqueue(frameData.GetRootItemID()); // 0

			while (toInvestigate.Count > 0)
			{
				var currentId = toInvestigate.Dequeue();
			
				var markerId = frameData.GetItemMarkerID(currentId);
				markerIds[currentId] = markerId;

				if (!_nameById.ContainsKey(markerId))
				{
					_nameById[markerId] = frameData.GetMarkerName(markerId);
				}
			
				frameData.GetItemChildren(currentId, children);
				childrenIds[currentId] = new int[children.Count];
				for (var i = 0; i < children.Count; i++)
				{
					var child = children[i];
					toInvestigate.Enqueue(child);
					childrenIds[currentId][i] = child;
					parentIds[child]          = currentId;
				}
			
				var time = frameData.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnSelfTime);
				ownTimes[currentId] = time;
			
				time = frameData.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnTotalTime);
				totalTimes[currentId] = time;
			
				var call = (ushort)frameData.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnCalls);
				calls[currentId] = call;

				var gc = frameData.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnGcMemory);
				totalGcAllocs[currentId] = gc;
			}

			CalculateOwnGc(0, childrenIds, totalGcAllocs, ownGcAllocs, markerIds, gcMarkerId);
		
			var headers = ProfilerTreeView.CreateColumnsState();

			_treeState = new(new()
			{
				MarkerIds     = markerIds,
				ParentIds     = parentIds,
				ChildrenIds   = childrenIds,
				OwnTimes      = ownTimes,
				TotalTimes    = totalTimes,
				OwnGcAllocs   = ownGcAllocs,
				TotalGcAllocs = totalGcAllocs,
				Calls         = calls,
				NameById      = _nameById,
			});
			_treeView = new(_treeState, headers, _blacklistMarkerIds);
			headers.ResizeToFit();
		}

		private static void CalculateOwnGc(
			int id, int[][] childrenIds, float[] totalGcAllocs, float[] ownGcAllocs, int[] markerIds, int gcMarkerId)
		{
			if (markerIds[id] == gcMarkerId)
			{
				totalGcAllocs[id] = 0;
				ownGcAllocs[id]   = 0;
				return;
			}
			var ownGc    = totalGcAllocs[id];
			var children = childrenIds[id];
			foreach (var child in children)
			{
				CalculateOwnGc(child, childrenIds, totalGcAllocs, ownGcAllocs, markerIds, gcMarkerId);
				if (markerIds[child] == gcMarkerId)
				{
					continue;
				}
				ownGc -= totalGcAllocs[child];
			}
			ownGcAllocs[id] = ownGc;
		}

		#region Show window
		[MenuItem("Window/Analysis/Kvd Profiler")]
		private static void Init()
		{
			// Get existing open window or if none, make a new one:
			var window = (KvdProfiler)EditorWindow.GetWindow(typeof(KvdProfiler));
			window.Show();
		}
		#endregion Show window
	}
}
