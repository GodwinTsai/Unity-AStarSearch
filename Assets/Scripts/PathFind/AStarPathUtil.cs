// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AStarPathUtil
{
	public static string GetPointPathPrint(List<Vector2Int> path)
	{
		var sb = new StringBuilder();
		var i = 0;
		var count = path.Count;
		foreach (var point in path)
		{
			sb.Append(point);
			if (++i < count)
			{
				sb.Append("->");
			}
		}

		return sb.ToString();
	}
}