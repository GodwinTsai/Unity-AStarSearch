// ==================================================
// Copyright (c) All rights reserved.
// @Author: GodWinTsai
// @Maintainer: 
// @Date: 
// @Desc: 
// ==================================================

public class PathFindConst
{
	
}

public enum EnumPathFindStatus
{
	Created = 0,
	PathQueue = 1,
	Processing = 2,
	ReturnQueue = 3,
	Returned = 4
}

public enum EnumPathResultStatus
{
	NotYet = 0,
	Success = 1,
	Fail = 2,
	Partial = 3,
}

public enum EnumPathHeuristic
{
	Manhattan = 0,
}

public enum EnumPathNeighbourNum
{
	Four = 4,
	Eight = 8,
}

public enum EnumWalkType
{
	Walk = 0,
	Portal = 1,
}
