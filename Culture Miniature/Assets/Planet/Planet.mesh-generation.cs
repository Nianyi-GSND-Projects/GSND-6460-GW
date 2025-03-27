using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CultureMiniature
{
	public partial class Planet
	{
		ProceduralMesh pm;

		void RegeneratePlanetMeshFromPM()
		{
			ProceduralMesh copy = new(pm);
			copy.Dualize();
			foreach(var v in copy.vertices)
			{
				v.uv = new(Mathf.Atan2(v.position.z, v.position.x), Mathf.Asin(v.position.y));
				v.color = Color.black;
			}
			copy.Triangularize();
			copy.CalculateNormals();
			UpdatePlanetMesh(copy.CreateMesh());
		}

		public void CreateMesh()
		{
			pm = CreateIcosahedron();
			RegeneratePlanetMeshFromPM();
		}

		public void SubdivideMesh()
		{
			pm.Subdivide();
			foreach(var v in pm.vertices)
			{
				v.position = v.position.normalized;
			}
			++subdivisionLevel;
			RegeneratePlanetMeshFromPM();
		}

		public void FinalizeMesh()
		{
			pm = null;
		}

		static ProceduralMesh CreateIcosahedron()
		{
			float p = 0.5f * (1 + Mathf.Sqrt(5));
			float s = 1 / Mathf.Sqrt(1 + p * p);

			List<ProceduralMesh.Vertex> vertices = new List<Vector3>() {
				new(00, +1, -p),
				new(00, +1, +p),
				new(00, -1, +p),
				new(00, -1, -p),
				new(+p, 00, +1),
				new(-p, 00, +1),
				new(-p, 00, -1),
				new(+p, 00, -1),
				new(+1, -p, 00),
				new(+1, +p, 00),
				new(-1, +p, 00),
				new(-1, -p, 00),
			}.Select(v => new ProceduralMesh.Vertex()
			{
				position = v * s,
			}).ToList();

			List<List<ProceduralMesh.Vertex>> faces = new (int, int, int)[] {
				// The top corner at A.
				(0xA, 0x1, 0xB),
				(0xA, 0x8, 0x1),
				(0xA, 0x5, 0x8),
				(0xA, 0x2, 0x5),
				(0xA, 0xB, 0x2),
				// The middle strip.
				(0x1, 0x4, 0x7),
				(0x1, 0x8, 0x4),
				(0x8, 0x9, 0x4),
				(0x8, 0x5, 0x9),
				(0x5, 0x3, 0x9),
				(0x5, 0x2, 0x3),
				(0x2, 0x6, 0x3),
				(0x2, 0xB, 0x6),
				(0xB, 0x7, 0x6),
				(0xB, 0x1, 0x7),
				// The bottom corner at B.
				(0xC, 0x7, 0x4),
				(0xC, 0x4, 0x9),
				(0xC, 0x9, 0x3),
				(0xC, 0x3, 0x6),
				(0xC, 0x6, 0x7),
			}
				.Select(((int, int, int) t) =>
				{
					var (a, b, c) = t;
					return new List<ProceduralMesh.Vertex> {
						vertices[a - 1],
						vertices[b - 1],
						vertices[c - 1],
					};
				})
				.ToList();

			return new()
			{
				vertices = vertices,
				faces = faces,
			};
		}
	}
}
