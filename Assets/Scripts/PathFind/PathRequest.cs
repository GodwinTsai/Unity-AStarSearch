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

public class PathRequest
{
	#region Set Once Fields
	public PathFindMapData mapData;
	
	public EnumPathNeighbourNum pathNeighbourNum;

	public bool returnNearestWhenFail;

	public bool needCheckJumpPoint;
	
	public Func<Vector2Int, bool> canWalkFunc;
	/// <summary>
	/// 终点可能不满足canWalkFunc，若为空则用canWalkFunc
	/// </summary>
	public Func<Vector2Int, bool> endPointCanWalkFunc;
	
	#endregion

	#region Set EveryTime Fields

	public Vector2Int startPoint;
	public Vector2Int endPoint;
	public Action<PathRequest> resultCallback;

	#endregion

	#region Reture Fields
	
	public List<Vector2Int> pointPath;

	public EnumPathFindStatus findStatus;
	public EnumPathResultStatus resultStatus;

	#endregion

}