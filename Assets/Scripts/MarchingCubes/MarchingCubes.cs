using UnityEngine;

public static class MarchingCubes 
{
	public static DensityMap padMap(DensityMap input)
	{
		for (int x = 0; x < input.map.GetLength(0) ; x++) {
			for (int y = 0; y <  input.map.GetLength(1); y++) {
				for (int z = 0; z < input.map.GetLength(2); z++) {
					if (x != 0 && x != input.map.GetLength(0)-1 && y != 0 && y != input.map.GetLength(1)-1 && z != 0 && z != input.map.GetLength(2)-1) {
						continue;
					} 
					input.map[x, y, z] = 0;
				}
			}
		}
		return input;
	}
	public static MeshData March (DensityMap densityMap, MarchingCubesSettings settings)
	{
		MeshData meshData = new MeshData();
		if(settings.showSides){ densityMap = padMap(densityMap); }
		for (int x = 0; x < densityMap.size.x - 1; x++) {
			for (int y = 0; y < densityMap.size.y - 1; y++) {
				for (int z = 0; z < densityMap.size.z - 1; z++) {

					// Set values at the corners of the cube
					float[] cubeValues = new float[] {
						densityMap.map[x, y, z + 1],
						densityMap.map[x + 1, y, z + 1],
						densityMap.map[x + 1, y, z],
						densityMap.map[x, y, z],
						densityMap.map[x, y + 1, z + 1],
						densityMap.map[x + 1, y + 1, z + 1],
						densityMap.map[x + 1, y + 1, z],
						densityMap.map[x, y + 1, z]
					};

					// Find the triangulation index
					int cubeIndex = 0;
					if (cubeValues[0] <= settings.isoLevel) {cubeIndex |= 1;}
					if (cubeValues[1] <= settings.isoLevel) {cubeIndex |= 2;}
					if (cubeValues[2] <= settings.isoLevel) {cubeIndex |= 4;}
					if (cubeValues[3] <= settings.isoLevel) {cubeIndex |= 8;}
					if (cubeValues[4] <= settings.isoLevel) {cubeIndex |= 16;}
					if (cubeValues[5] <= settings.isoLevel) {cubeIndex |= 32;}
					if (cubeValues[6] <= settings.isoLevel) {cubeIndex |= 64;}
					if (cubeValues[7] <= settings.isoLevel) {cubeIndex |= 128;}

					// Get the intersecting edges
					int[] edges = MarchingCubesTables.triTable[cubeIndex];
					Vector3 mapPos = new Vector3(x, y, z);

					// Triangulate
					for (int i = 0; edges[i] != -1; i += 3) {
						int e00 = MarchingCubesTables.edgeConnections[edges[i]][0];
						int e01 = MarchingCubesTables.edgeConnections[edges[i]][1];

						int e10 = MarchingCubesTables.edgeConnections[edges[i + 1]][0];
						int e11 = MarchingCubesTables.edgeConnections[edges[i + 1]][1];

						int e20 = MarchingCubesTables.edgeConnections[edges[i + 2]][0];
						int e21 = MarchingCubesTables.edgeConnections[edges[i + 2]][1];

						Vector3 a;
						Vector3 b;
						Vector3 c;
						if (settings.interpolation) {
							a = Interp(MarchingCubesTables.cubeCorners[e00], cubeValues[e00], MarchingCubesTables.cubeCorners[e01], cubeValues[e01], settings.isoLevel) + mapPos;
							b = Interp(MarchingCubesTables.cubeCorners[e10], cubeValues[e10], MarchingCubesTables.cubeCorners[e11], cubeValues[e11], settings.isoLevel) + mapPos;
							c = Interp(MarchingCubesTables.cubeCorners[e20], cubeValues[e20], MarchingCubesTables.cubeCorners[e21], cubeValues[e21], settings.isoLevel) + mapPos;
						} else {
							a = Default(MarchingCubesTables.cubeCorners[e00],MarchingCubesTables.cubeCorners[e01]) + mapPos;
							b = Default(MarchingCubesTables.cubeCorners[e10],MarchingCubesTables.cubeCorners[e11]) + mapPos;
							c = Default(MarchingCubesTables.cubeCorners[e20],MarchingCubesTables.cubeCorners[e21]) + mapPos;
						}
						meshData.AddTriangle(a, b, c);
					}
				}
			}
		}
		return meshData;
	} 
	
	static private Vector3 Interp(Vector3 edgeVertex1, float valueAtVertex1, Vector3 edgeVertex2, float valueAtVertex2, float isoLevel) {
		return (edgeVertex1 + (isoLevel - valueAtVertex1) * (edgeVertex2 - edgeVertex1) / (valueAtVertex2 - valueAtVertex1));
	}

	static private Vector3 Default(Vector3 edgeVertex1, Vector3 edgeVertex2) {
		return (edgeVertex1 + edgeVertex2) / 2;
	}
}
