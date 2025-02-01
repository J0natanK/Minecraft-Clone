using Unity.Mathematics;

public class Blocks
{
	public static readonly byte Air = 0;
	public static readonly byte Stone = 1;
	public static readonly byte Dirt = 2;
	public static readonly byte Grass = 3;
	public static readonly byte Sand = 4;
	public static readonly byte Water = 5;
	public static readonly byte Log = 6;
	public static readonly byte Leaves = 7;

	//The bottom left UVs for all 6 faces
	public static FaceUVs GetUVs(int blockIndex)
	{
		FaceUVs uvs = new FaceUVs();
		switch (blockIndex)
		{
			case 1:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, 0),
					uv1 = new float2(0, 0),
					uv2 = new float2(0, 0),
					uv3 = new float2(0, 0),
					uv4 = new float2(0, 0),
					uv5 = new float2(0, 0)
				};

				break;

			case 2:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .4f),
					uv1 = new float2(0, .4f),
					uv2 = new float2(0, .4f),
					uv3 = new float2(0, .4f),
					uv4 = new float2(0, .4f),
					uv5 = new float2(0, .4f)
				};
				break;
			case 3:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .4f),
					uv1 = new float2(.5f, .4f),
					uv2 = new float2(.5f, .4f),
					uv3 = new float2(.5f, .4f),
					uv4 = new float2(.5f, .4f),
					uv5 = new float2(0, .2f)
				};
				break;
			case 4:
				uvs = new FaceUVs()
				{
					uv0 = new float2(.5f, 0),
					uv1 = new float2(.5f, 0),
					uv2 = new float2(.5f, 0),
					uv3 = new float2(.5f, 0),
					uv4 = new float2(.5f, 0),
					uv5 = new float2(.5f, 0)
				};
				break;
			case 5:
				uvs = new FaceUVs()
				{
					uv0 = new float2(.5f, .2f),
					uv1 = new float2(.5f, .2f),
					uv2 = new float2(.5f, .2f),
					uv3 = new float2(.5f, .2f),
					uv4 = new float2(.5f, .2f),
					uv5 = new float2(.5f, .2f)
				};
				break;
			case 6:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .8f),
					uv1 = new float2(.5f, .6f),
					uv2 = new float2(.5f, .6f),
					uv3 = new float2(.5f, .6f),
					uv4 = new float2(.5f, .6f),
					uv5 = new float2(0, .8f)
				};
				break;
			case 7:
				uvs = new FaceUVs()
				{
					uv0 = new float2(0, .6f),
					uv1 = new float2(0, .6f),
					uv2 = new float2(0, .6f),
					uv3 = new float2(0, .6f),
					uv4 = new float2(0, .6f),
					uv5 = new float2(0, .6f)
				};
				break;
		}

		return uvs;
	}
}

public struct FaceUVs
{
	public float2 uv0;
	public float2 uv1;
	public float2 uv2;
	public float2 uv3;
	public float2 uv4;
	public float2 uv5;

	public float2 GetUV(int i)
	{
		switch (i)
		{
			case 0: return uv0;
			case 1: return uv1;
			case 2: return uv2;
			case 3: return uv3;
			case 4: return uv4;
			case 5: return uv5;
			default: return new float2(0, 0);
		}
	}
}
