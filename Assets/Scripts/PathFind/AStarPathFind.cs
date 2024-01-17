// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using System.Collections.Generic;
using System;
using UnityEngine;

public class AStarPathFind
{
	public static bool cheatPrintPath = false;
	
	private ClassPool<PathNode> _pathNodePool;

	private const int DEFAULT_NODE_POOL_COUNT = 64;
	
	private readonly List<Vector2Int> _neighbourOffsets = new List<Vector2Int>()
	{
		new Vector2Int(0, 1),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(-1, 0),
		new Vector2Int(1, 1),
		new Vector2Int(1, -1),
		new Vector2Int(-1, -1),
		new Vector2Int(-1, 1),
	};
	
	private Dictionary<Vector2Int, PathNode> _allNodes = new Dictionary<Vector2Int, PathNode>();
	private List<PathNode> _openList;
	private List<PathNode> _tempLinkNodeList = new List<PathNode>();

	private PathNode _startNode;
	private PathNode _endNode;

	private List<List<PathCellData>> _cellDataList;
	private Func<Vector2Int, bool> _canWalkFunc;
	private Func<Vector2Int, bool> _endPointCanWalkFunc;
	private EnumPathNeighbourNum _pathNeighbourNum;

	private int _row;
	private int _col;
	private bool _returnNearestWhenFail;
	private PathRequest _request;
	
	public AStarPathFind()
	{
		Init(DEFAULT_NODE_POOL_COUNT);
	}

	public AStarPathFind(int nodePoolCount)
	{
		Init(nodePoolCount);
	}

	private void Init(int nodePoolCount)
	{
		_pathNodePool = new ClassPool<PathNode>(null, null);
		_openList = new List<PathNode>(nodePoolCount);
	}

	public List<Vector2Int> FindSync(PathRequest pathRequest)
	{
		var isRequestValid = CheckRequestValid(pathRequest);
		if (!isRequestValid)
		{
			ReturnFindResult(pathRequest, null);
			return null;
		}
		
		pathRequest.findStatus = EnumPathFindStatus.Processing;

		InitFind(pathRequest);

		var pointPath = BeginFind();
		
		ReturnFindResult(pathRequest, pointPath);

		ResetFind();
		return pointPath;
	}

	private bool CheckRequestValid(PathRequest pathRequest)
	{
		if (pathRequest.canWalkFunc == null)
		{
			return false;
		}

		if (pathRequest.mapData == null)
		{
			return false;
		}
		var cellDataList = pathRequest.mapData.cellDataList;
		if (cellDataList == null || cellDataList.Count == 0)
		{
			return false;
		}

		return true;
	}

	private void ResetFind()
	{
		_cellDataList = null;
		_startNode = null;
		_endNode = null;
		_row = _col = 0;
		_canWalkFunc = null;
		_endPointCanWalkFunc = null;
		_returnNearestWhenFail = false;
		_request = null;
		
		_tempLinkNodeList.Clear();
		_openList.Clear();

		foreach (var node in _allNodes.Values)
		{
			ReleasePathNode(node);
		}
		_allNodes.Clear();
	}

	private void InitFind(PathRequest pathRequest)
	{
		_request = pathRequest;
		_tempLinkNodeList.Clear();
		_openList.Clear();
		_allNodes.Clear();

		_returnNearestWhenFail = pathRequest.returnNearestWhenFail;
		_cellDataList = pathRequest. mapData.cellDataList;
		_row = _cellDataList.Count;
		_col = _cellDataList[0].Count;
		
		_canWalkFunc = pathRequest.canWalkFunc;
		_endPointCanWalkFunc = pathRequest.endPointCanWalkFunc;
		_pathNeighbourNum = pathRequest.pathNeighbourNum;
		
		_startNode = GetOrCreateNode(pathRequest.startPoint);
		_endNode = GetOrCreateNode(pathRequest.endPoint);
		AddToOpenList(_startNode);
	}

	private void ReturnFindResult(PathRequest pathRequest, List<Vector2Int> pointPath)
	{
		pathRequest.findStatus = EnumPathFindStatus.Returned;
		pathRequest.pointPath = pointPath;
		var hasPath = pointPath != null && pointPath.Count > 0;
		pathRequest.resultStatus = hasPath ? EnumPathResultStatus.Success : EnumPathResultStatus.Fail;
		pathRequest.resultCallback?.Invoke(pathRequest);
	}

	private List<Vector2Int> BeginFind()
	{
		var curNode = GetMinCostOpenNode();
		while (curNode != null)
		{
			var isTarget = curNode.IsSame(_endNode);
			if (isTarget)
			{
				//找到路径，return path
				var pointPath = GeneratePointPath(curNode);
				return pointPath;
			}

			CalculateLinkNodes(curNode);
			RemoveFromOpenList(curNode);
			AddToCloseList(curNode);
			
			curNode = GetMinCostOpenNode();
		}
		//找不到路径，返回最接近的终点，或者返回失败

		return null;
	}

	private void CalculateLinkNodes(PathNode curNode)
	{
		var linkNodes = GetLinkNodes(curNode);
		foreach (var linkNode in linkNodes)
		{
			CalculateCost(curNode, linkNode, out int g, out int h, out int f);
			if (IsInOpenList(linkNode))
			{
				if (f < linkNode.f)
				{
					UpdateLinkNode(curNode, linkNode, g, h, f);
					SortOpenList();
				}
			}
			else if (IsInCloseList(linkNode))
			{
				if (f < linkNode.f)
				{
					UpdateLinkNode(curNode, linkNode, g, h, f);
					RemoveFromCloseList(linkNode);
					AddToOpenList(linkNode);
				}
			}
			else
			{
				UpdateLinkNode(curNode, linkNode, g, h, f);
				AddToOpenList(linkNode);
			}
		}
	}

	private void CalculateCost(PathNode curNode, PathNode linkNode, out int g, out int h, out int f)
	{
		g = curNode.g + linkNode.cost;
		h = CalculateHScore(linkNode);
		f = g + h;
	}
	
	private void UpdateLinkNode(PathNode curNode, PathNode linkNode, int g, int h, int f)
	{
		linkNode.g = g;
		linkNode.h = h;
		linkNode.f = f;
		linkNode.preNode = curNode;
	}

	private int CalculateHScore(PathNode linkNode)
	{
		return Mathf.Abs(_endNode.x - linkNode.x) + Mathf.Abs(_endNode.y - linkNode.y);
	}

	private void AddToOpenList(PathNode node)
	{
		AddToList(_openList, node);
		node.isOpened = true;
	}

	private void RemoveFromOpenList(PathNode node)
	{
		for (int i = _openList.Count - 1; i >= 0; i--)
		{
			if (_openList[i].IsSame(node))
			{
				_openList.RemoveAt(i);
				node.isOpened = false;
				break;
			}
		}
	}

	private void SortOpenList()
	{
		_openList.Sort(SortByF);
	}

	//大到小排序
	private int SortByF(PathNode x, PathNode y)
	{
		return y.f - x.f;
	}

	//大到小排序
	private void AddToList(List<PathNode> nodeList, PathNode node)
	{
		var count = nodeList.Count;

		int l = 0;
		int r = count - 1;
		while (l <= r)
		{
			var mid = l + ((r - l) >> 1);
			var curNode = nodeList[mid];
			if (node.f > curNode.f)
			{
				r = mid - 1;
			}
			else
			{
				l = mid + 1;
			}
		}
		nodeList.Insert(l, node);
	}

	private bool IsInOpenList(PathNode node)
	{
		return node.isOpened;
	}

	private PathNode GetMinCostOpenNode()
	{
		var count = _openList.Count;
		if (count == 0)
		{
			return null;
		}

		var node = _openList[count - 1];
		return node;
	}
	
	private void AddToCloseList(PathNode node)
	{
		node.isClosed = true;
	}
	
	private bool IsInCloseList(PathNode node)
	{
		return node.isClosed;
	}
	
	private void RemoveFromCloseList(PathNode node)
	{
		node.isClosed = false;
	}

	private List<PathNode> GetLinkNodes(PathNode node)
	{
		_tempLinkNodeList.Clear();

		AddNeighbourLinkNodes(ref _tempLinkNodeList, node);
		AddJumpLinkNodes(ref _tempLinkNodeList, node);

		return _tempLinkNodeList;
	}

	private void AddNeighbourLinkNodes(ref List<PathNode> list, PathNode node)
	{
		var neighbourNum = (int)_pathNeighbourNum;
		for (int i = 0; i < neighbourNum; i++)
		{
			var offset = _neighbourOffsets[i]; 
			var linkPoint = node.point + offset;
			var linkNode = TryGetLinkNode(node, linkPoint);
			if (linkNode == null)
			{
				continue;
			}
			list.Add(linkNode);
		}
	}

	private void AddJumpLinkNodes(ref List<PathNode> list, PathNode node)
	{
		if (!_request.needCheckJumpPoint)
		{
			return;
		}
		var jumpDic = _request.mapData.cellLinkDataDic;
		if (jumpDic == null || jumpDic.Count == 0)
		{
			return;
		}

		var point = node.point;
		if (!jumpDic.TryGetValue(point, out var jumpList))
		{
			return;
		}

		if (jumpList == null || jumpList.Count == 0)
		{
			return;
		}

		foreach (var linkData in jumpList)
		{
			var linkNode = TryGetLinkNode(node, linkData.endPoint);
			if (linkNode == null)
			{
				continue;
			}
			list.Add(linkNode);
		}
	}

	private PathNode TryGetLinkNode(PathNode node, Vector2Int linkPoint)
	{
		if (!IsPointValid(linkPoint))
		{
			return null;
		}

		var isPreNode = node.preNode != null && node.preNode.point == linkPoint;
		if (isPreNode)
		{
			return null;
		}

		var isWalkable = IsWalkable(linkPoint);
		if (!isWalkable)
		{
			return null;
		}

		var linkNode = GetOrCreateNode(linkPoint);
		return linkNode;
	}

	private bool IsWalkable(Vector2Int point)
	{
		if (point == _endNode.point && _endPointCanWalkFunc != null)
		{
			return _endPointCanWalkFunc.Invoke(point);
		}

		return _canWalkFunc(point);
	}

	private bool IsPointValid(Vector2Int point)
	{
		return point.x >= 0 && point.x < _row && point.y >= 0 && point.y < _col;
	}

	private PathNode GetOrCreateNode(Vector2Int point)
	{
		if (!_allNodes.TryGetValue(point, out PathNode node))
		{
			node = NewPathNode();
			node.Init(point);
			var cellData = _cellDataList[point.x][point.y];
			node.cost = cellData.cost;
			_allNodes.Add(point, node);
		}

		return node;
	}

	private List<Vector2Int> GeneratePointPath(PathNode endNode)
	{
		var list = new List<Vector2Int>();
		while (endNode != null)
		{
			list.Add(new Vector2Int(endNode.x, endNode.y));
			endNode = endNode.preNode;
		}
		list.Reverse();
#if MT_DEBUG_LOG
		PrintPointPath(list);
#endif
		return list;
	}

	private void PrintPointPath(List<Vector2Int> path)
	{
		if (!cheatPrintPath)
		{
			return;
		}

		var print = GetPointPathPrint(path);
		
		Debug.Log($"[AStarPathFind][start:{_startNode.point}->end:{_endNode.point}],,path:{print}");
	}

	public string GetPointPathPrint(List<Vector2Int> path)
	{
		return AStarPathUtil.GetPointPathPrint(path);
	}

	#region Pool
	private PathNode NewPathNode()
	{
		var pathNode = _pathNodePool.Get();
		pathNode.Reset();
		return pathNode;
	}

	private void ReleasePathNode(PathNode pathNode)
	{
		pathNode.Reset();
		_pathNodePool.Release(pathNode);
	}

	#endregion
}