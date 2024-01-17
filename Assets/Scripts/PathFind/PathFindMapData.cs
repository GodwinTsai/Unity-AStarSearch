// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

using System.Collections.Generic;
using UnityEngine;

public class PathFindMapData
{
	public List<List<PathCellData>> cellDataList;
	
	public Dictionary<Vector2Int, List<PathCellLinkData>> cellLinkDataDic;
}