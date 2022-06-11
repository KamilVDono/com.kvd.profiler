using System.Collections.Generic;
using System.Linq;
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
			"Main Thread",
		};

		[SerializeField] private ProfilerTreeViewState _treeState;
		[SerializeField] private int _selectedFrameMin;
		[SerializeField] private int _selectedFrameMax;
		private ProfilerTreeView _treeView;

		private readonly HashSet<int> _blacklistMarkerIds = new();
		private readonly List<FrameDataView.MarkerInfo> _allMarkersInfo = new();
		private string[] _markerNames;
		private List<int>[] _samplesByMarkers;

		private void OnEnable()
		{
			_selectedFrameMin = _selectedFrameMax = -1;
		}

		private void OnGUI()
		{
			if (ProfilerDriver.lastFrameIndex < 0)
			{
				EditorGUILayout.LabelField("Collect profiler frames at first", EditorStyles.boldLabel);
				return;
			}
			EditorGUILayout.BeginHorizontal();
			EditorGUI.BeginChangeCheck();
			
			var minIndex = ProfilerDriver.firstFrameIndex;
			var maxIndex = ProfilerDriver.lastFrameIndex;
			
			float min = _selectedFrameMin;
			float max = _selectedFrameMax;
			
			EditorGUILayout.LabelField($"{minIndex}", GUILayout.Width(25));
			EditorGUILayout.MinMaxSlider(ref min, ref max, minIndex, maxIndex);
			EditorGUILayout.LabelField($"{maxIndex}", GUILayout.Width(25));
			
			var minRound = Mathf.RoundToInt(min);
			var maxRound = Mathf.RoundToInt(max);
			
			minRound = EditorGUILayout.DelayedIntField(minRound, GUILayout.Width(35));
			EditorGUILayout.LabelField("/", GUILayout.Width(15));
			maxRound = EditorGUILayout.DelayedIntField(maxRound, GUILayout.Width(35));
			
			EditorGUI.BeginChangeCheck();
			var singleFrame = _selectedFrameMin == _selectedFrameMax ? _selectedFrameMin : -1;
			singleFrame = EditorGUILayout.DelayedIntField(singleFrame, GUILayout.Width(35));
			if (EditorGUI.EndChangeCheck())
			{
				maxRound = minRound = singleFrame;
			}
			
			if (EditorGUI.EndChangeCheck() && (minRound != _selectedFrameMin || maxRound != _selectedFrameMax))
			{
				_selectedFrameMin = Mathf.Clamp(minRound, minIndex, maxIndex);
				_selectedFrameMax = Mathf.Clamp(maxRound, minIndex, maxIndex);
				if (_selectedFrameMax < _selectedFrameMin)
				{
					(_selectedFrameMin, _selectedFrameMax) = (_selectedFrameMax, _selectedFrameMin);
				}
				CollectData();
			}
			EditorGUILayout.EndHorizontal();

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

			var range  = _selectedFrameMax-_selectedFrameMin+1;
			var frames = new List<HierarchyFrameDataView>(_selectedFrameMax-_selectedFrameMin+1);
			var frameSamplesRange = new List<Vector2Int>(_selectedFrameMax-_selectedFrameMin+1);
			for (var i = 0; i < range; i++)
			{
				CollectFrame(i, frames, frameSamplesRange);
			}

			if (frames.Count < 0)
			{
				return;
			}
			
			var gcMarkerId = CollectMarkers(frames);

			var samplesCount    = frameSamplesRange.Last().y;
			var markerIds       = new int[samplesCount];
			var parentIds       = new int[samplesCount];
			var childrenIds     = new int[samplesCount][];
			var ownTimes        = new float[samplesCount];
			var totalTimes      = new float[samplesCount];
			var totalGcAllocs   = new float[samplesCount];
			var ownGcAllocs     = new float[samplesCount];

			var children = new List<int>(32);

			// Root
			markerIds[0]   = -1;
			childrenIds[0] = new int[frames.Count];

			for (var i = 0; i < frames.Count; i++)
			{
				childrenIds[0][i] = frameSamplesRange[i].x;
			}

			for (var globalCurrentId = 1; globalCurrentId < samplesCount; globalCurrentId++)
			{
				var        frameId = 0;
				Vector2Int frameRange;
				while (globalCurrentId >= (frameRange = frameSamplesRange[frameId]).y)
				{
					frameId++;
				}

				var frame         = frames[frameId];
				var startSampleId = frameRange.x;

				var currentId = globalCurrentId-startSampleId;
				
				var markerId = frame.GetItemMarkerID(currentId);
				markerIds[globalCurrentId] = markerId;

				if (!_blacklistMarkerIds.Contains(markerId) && markerId >= 0)
				{
					var samples = _samplesByMarkers[markerId];
					samples.Add(globalCurrentId);
				}

				frame.GetItemChildren(currentId, children);
				childrenIds[globalCurrentId] = new int[children.Count];
				for (var c = 0; c < children.Count; c++)
				{
					var childLocalId  = children[c];
					var childGlobalId = startSampleId+childLocalId;
					childrenIds[globalCurrentId][c] = childGlobalId;
					parentIds[childGlobalId]        = globalCurrentId;
				}
				
				var time = frame.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnSelfTime);
				ownTimes[globalCurrentId] = time;

				time = frame.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnTotalTime);
				totalTimes[globalCurrentId] = time;

				var gc = frame.GetItemColumnDataAsFloat(currentId, HierarchyFrameDataView.columnGcMemory);
				totalGcAllocs[globalCurrentId] = gc;
			}
			
			for (var i = 0; i < frames.Count; i++)
			{
				frames[i].Dispose();
			}
			frames.Clear();

			CalculateOwnGc(0, childrenIds, totalGcAllocs, ownGcAllocs, markerIds, gcMarkerId);

			CreateProfilerTree(markerIds, parentIds, childrenIds, ownTimes, totalTimes, ownGcAllocs, totalGcAllocs);
		}

		private void CollectFrame(int i, List<HierarchyFrameDataView> frames, List<Vector2Int> frameSamplesRange)
		{
			var frame = ProfilerDriver.GetHierarchyFrameDataView(_selectedFrameMin+i, 0,
				HierarchyFrameDataView.ViewModes.HideEditorOnlySamples, HierarchyFrameDataView.columnDontSort, false);
			if (!frame.valid)
			{
				frame.Dispose();
				return;
			}
			frames.Add(frame);
			var frameRange = new Vector2Int(0, frame.sampleCount);
			if (i > 0)
			{
				var previousEnd = frameSamplesRange[i-1].y;
				frameRange.x += previousEnd;
				frameRange.y += previousEnd;
			}
			else
			{
				frameRange.x += 1;
				frameRange.y += 1;
			}
			frameSamplesRange.Add(frameRange);
		}
		
		private int CollectMarkers(List<HierarchyFrameDataView> frames)
		{
			frames[0].GetMarkers(_allMarkersInfo);
			var maxMarkerId = 0;
			foreach (var markerInfo in _allMarkersInfo)
			{
				if (maxMarkerId < markerInfo.id)
				{
					maxMarkerId = markerInfo.id;
				}
			}
			maxMarkerId++;
			_markerNames      = new string[maxMarkerId];
			_samplesByMarkers = new List<int>[maxMarkerId];
			foreach (var markerInfo in _allMarkersInfo)
			{
				if (markerInfo.id < 0)
				{
					continue;
				}
				_markerNames[markerInfo.id]      = markerInfo.name;
				_samplesByMarkers[markerInfo.id] = new(16);
			}

			var gcMarkerId = frames[0].GetMarkerId("GC.Alloc");
			foreach (var blacklistMarkerName in BlacklistMarkerNames)
			{
				_blacklistMarkerIds.Add(frames[0].GetMarkerId(blacklistMarkerName));
			}
			_blacklistMarkerIds.Add(FrameDataView.invalidMarkerId);
			return gcMarkerId;
		}

		private static void CalculateOwnGc(int id, int[][] childrenIds,
			float[] totalGcAllocs, float[] ownGcAllocs, int[] markerIds, int gcMarkerId)
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
		
		private void CreateProfilerTree(int[] markerIds, int[] parentIds, int[][] childrenIds,
			float[] ownTimes, float[] totalTimes, float[] ownGcAllocs, float[] totalGcAllocs)
		{
			var headers = ProfilerTreeView.CreateColumnsState();
			_treeState = new(new()
			{
				MarkerIds        = markerIds,
				ParentIds        = parentIds,
				ChildrenIds      = childrenIds,
				OwnTimes         = ownTimes,
				TotalTimes       = totalTimes,
				OwnGcAllocs      = ownGcAllocs,
				TotalGcAllocs    = totalGcAllocs,
				NameById         = _markerNames,
				SamplesByMarkers = _samplesByMarkers,
			});
			_treeView = new(_treeState, headers, _blacklistMarkerIds);
			headers.ResizeToFit();
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
