// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using UnityEngine;

public class PathNode
{
	public Vector2Int point;
	public int x;
	public int y;
	
	public int g;
	public int h;
	public int f;
	
	public int cost = 1;

	public PathNode preNode;

	public bool isOpened;
	public bool isClosed;

	public void Init(Vector2Int pointParam)
	{
		x = pointParam.x;
		y = pointParam.y;
		point = new Vector2Int(x, y);
	}

	public bool IsSame(PathNode node)
	{
		return node != null && node.point == point;
	}

	public void Reset()
	{
		point = Vector2Int.zero;
		x = y = 0;
		g = h = f = 0;
		cost = 1;
		preNode = null;
		isOpened = false;
		isClosed = false;
	}
}