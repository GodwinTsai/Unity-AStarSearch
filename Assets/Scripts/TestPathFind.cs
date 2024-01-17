// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public class TestPathFind : MonoBehaviour
{
	#region Mono Fields

	public Vector2Int searchStartPoint = new Vector2Int(0, 0);
	public Vector2Int searchEndPoint = new Vector2Int(7, 7);
	public bool useJumpPoint = true;

	public int row = 8;
	public int col = 8;
	#endregion

	#region Private Fields
	private AStarPathFind _pathFind;
	private PathFindMapData _pathFindMapData;
	private PathRequest _pathRequest;
	
	#endregion

	#region Init Methods

	private void Awake()
	{
		InitPath();
		AStarPathFind.cheatPrintPath = true;
	}

	private void OnEnable()
	{
		SearchPath(searchStartPoint, searchEndPoint, useJumpPoint);
	}

	private void InitPath()
	{
		_pathFind = new AStarPathFind(row * col);
		_pathFindMapData = GetPathFindMapData();
	}

	private PathFindMapData GetPathFindMapData()
	{
		var pathFindMapData = new PathFindMapData();

		pathFindMapData.cellDataList = GetPathFindCellDataList();

		pathFindMapData.cellLinkDataDic = GetPathFindJumpDataList();

		return pathFindMapData;
	}
	
	private List<List<PathCellData>> GetPathFindCellDataList()
	{
		var pathCellRowDataList = new List<List<PathCellData>>(row);
		for (int i = 0; i < row; i++)
		{
			var colDataList = new List<PathCellData>(col);
			for (int j = 0; j < col; j++)
			{
				var pathCellData = new PathCellData();
				pathCellData.point = new Vector2Int(i, j);
				pathCellData.cost = 1;
				pathCellData.walkType = EnumWalkType.Walk;
				colDataList.Add(pathCellData);
			}
			pathCellRowDataList.Add(colDataList);
		}

		return pathCellRowDataList;
	}
	
	private Dictionary<Vector2Int, List<PathCellLinkData>> GetPathFindJumpDataList()
	{
		var result = new Dictionary<Vector2Int, List<PathCellLinkData>>();

		var start = new Vector2Int(0, 0);
		var end = new Vector2Int(5, 5);
		var linkData = new PathCellLinkData
		{
			startPoint = start,
			endPoint = end,
			walkType = EnumWalkType.Portal,
			cost = 1
		};

		if (!result.TryGetValue(start, out var list))
		{
			list = new List<PathCellLinkData>();
			result.Add(start, list);
		}
		list.Add(linkData);

		return result;
	}
	#endregion

	#region Search Methods
	public List<Vector2Int> SearchPath(Vector2Int startPoint, Vector2Int endPoint, bool needCheckJumpPoint)
	{
		List<Vector2Int> path = null;
		var request = GetPathRequest(startPoint, endPoint, null, needCheckJumpPoint);
		_pathFind.FindSync(request);
		if (request.resultStatus == EnumPathResultStatus.Success)
		{
			path = request.pointPath;
			if (AStarPathFind.cheatPrintPath)
			{
				Debug.Log($"SearchPath Success,start:{startPoint}, end:{endPoint},, path:{_pathFind.GetPointPathPrint(path)}");
			}
		}
		else
		{
			if (AStarPathFind.cheatPrintPath)
			{
				Debug.Log($"SearchPath Fail,start:{startPoint}, end:{endPoint}");
			}
		}

		return path;
	}
	
	private PathRequest GetPathRequest(Vector2Int startPoint, Vector2Int endPoint, Action<PathRequest> resultCallback, bool needCheckJumpPoint)
	{
		if (_pathRequest == null)
		{
			_pathRequest = new PathRequest();
			_pathRequest.mapData = _pathFindMapData;
			_pathRequest.pathNeighbourNum = EnumPathNeighbourNum.Four;
			_pathRequest.canWalkFunc = CanWalkFunc;
			_pathRequest.endPointCanWalkFunc = EndPointCanWalkFunc;
			
		}
		_pathRequest.needCheckJumpPoint = needCheckJumpPoint;
		_pathRequest.pointPath = null;
		_pathRequest.findStatus = EnumPathFindStatus.Created;
		_pathRequest.resultStatus = EnumPathResultStatus.NotYet;

		_pathRequest.startPoint = startPoint;
		_pathRequest.endPoint = endPoint;
		_pathRequest.resultCallback = resultCallback;

		return _pathRequest;
	}
	
	private bool CanWalkFunc(Vector2Int point)
	{
		return true;
	}
	
	private bool EndPointCanWalkFunc(Vector2Int point)
	{
		return true;
	}
	#endregion

}