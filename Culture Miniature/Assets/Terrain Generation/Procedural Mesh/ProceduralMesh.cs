using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CultureMiniature
{
	public class ProceduralMesh
	{
		public class Vertex
		{
			public Vector3 position;
			public Vector3 normal;
			public Vector2 uv;
			public Color color = Color.white;
		}

		public List<Vertex> vertices = new();
		public List<List<Vertex>> faces = new();

		public Mesh CreateMesh()
		{
			var vertices = this.vertices;
			var faces = this.faces;
			// Use a look-up map to speed up index querying.
			Dictionary<Vertex, int> indexMap = new();

			foreach(var (v, i) in vertices.Select((v, i) => (v, i)))
				indexMap[v] = i;

			Mesh mesh = new()
			{
				subMeshCount = 1,
			};
			mesh.SetVertices(vertices.Select(v => v.position).ToList());
			mesh.SetNormals(vertices.Select(v => v.normal).ToList());
			mesh.SetColors(vertices.Select(v => v.color).ToList());
			mesh.SetUVs(0, vertices.Select(v => v.uv).ToList());

			List<int> triangles = new(faces.Count * 3);
			foreach(var face in faces)
			{
				foreach(var v in face)
					triangles.Add(indexMap[v]);
			}
			mesh.SetTriangles(triangles, 0);

			return mesh;
		}

		public void CalculateNormals()
		{
			foreach(var v in vertices)
				v.normal = default;
			foreach(var f in faces)
			{
				Vector3 fn = CalculateFaceNormal(f);
				foreach(var v in f)
					v.normal += fn;
			}
			foreach(var v in vertices)
				v.normal = v.normal.normalized;
		}

		Vector3 CalculateVertexNormal(Vertex v)
		{
			Vector3 r = Vector3.zero;
			foreach(var f in faces)
			{
				if(!f.Contains(v))
					continue;
				r += CalculateFaceNormal(f);
			}
			return r.normalized;
		}

		Vector3 CalculateFaceNormal(List<Vertex> f)
		{
			if(f.Count < 3)
				return Vector3.zero;
			return Vector3.Cross(f[0].position - f[1].position, f[0].position - f[2].position).normalized;
		}

		Vertex Midpoint(IEnumerable<Vertex> vertices)
		{
			Vector3 pos = default, normal = default;
			int c = 0;
			foreach(var v in vertices)
			{
				pos += v.position;
				normal += v.normal;
				++c;
			}
			pos /= c;
			normal /= c;
			return new()
			{
				position = pos,
				normal = normal,
			};
		}

		public void Subdivide()
		{
			List<Vertex> newV = new();
			Dictionary<(Vertex, Vertex), Vertex> midPoints = new();
			List<List<Vertex>> newF = new();

			Vertex GetMidPoint(Vertex a, Vertex b)
			{
				if(midPoints.ContainsKey((a, b)))
					return midPoints[(a, b)];
				if(midPoints.ContainsKey((b, a)))
					return midPoints[(b, a)];
				Vertex v = Midpoint(new Vertex[] { a, b });
				midPoints[(a, b)] = v;
				newV.Add(v);
				return v;
			}

			newV.AddRange(vertices);
			foreach(var f in faces)
			{
				int l = f.Count;
				List<Vertex> newM = new();
				for(int i = 0; i < l; ++i)
					newM.Add(GetMidPoint(f[i], f[(i + 1) % l]));
				for(int i = 0; i < l; ++i)
				{
					Vertex next = GetMidPoint(f[i], f[(i + 1) % l]);
					Vertex prev = GetMidPoint(f[i], f[(i - 1 + l) % l]);
					newF.Add(new() { f[i], next, prev });
				}
				newF.Add(newM);
			}

			vertices = newV;
			faces = newF;
		}

		Dictionary<Vertex, List<List<Vertex>>> GetV2FsMap()
		{
			Dictionary<Vertex, List<List<Vertex>>> v2fs = new();
			foreach(var v in vertices)
				v2fs[v] = new();
			foreach(var f in faces)
			{
				foreach(var v in f)
					v2fs[v].Add(f);
			}

			// Use a look-up table to omit repeated calculations.
			Dictionary<List<Vertex>, (Vector3, Vector3)> fCache = new();
			foreach(var f in faces)
				fCache[f] = (Midpoint(f).position, CalculateFaceNormal(f));

			foreach(var (v, fs) in v2fs)
			{
				Vector3 n = fCache[fs[0]].Item2;
				Vector3 t = (fCache[fs[0]].Item1 - v.position).normalized;
				Vector3 t2 = Vector3.Cross(n, t);

				float GetAngle(List<Vertex> f)
				{
					Vector3 d = fCache[f].Item1 - v.position;
					float x = Vector3.Dot(d, t), y = Vector3.Dot(d, t2);
					return Mathf.Atan2(y, x);
				}

				fs.Sort((a, b) => GetAngle(a).CompareTo(GetAngle(b)));
			}

			return v2fs;
		}

		public void Dualize()
		{
			Dictionary<Vertex, List<List<Vertex>>> v2fs = GetV2FsMap();
			Dictionary<List<Vertex>, Vertex> midPoints = new();
			foreach(var f in faces)
				midPoints.Add(f, Midpoint(f));

			faces = new(vertices.Count);
			foreach(var v in vertices)
				faces.Add(v2fs[v].Select(f => midPoints[f]).ToList());
			vertices = midPoints.Values.ToList();
		}

		public void Triangularize()
		{
			var badFaces = faces.Where(f => f.Count > 3).ToList();
			faces = faces.Where(f => f.Count == 3).ToList();
			foreach(var f in badFaces)
			{
				var mid = Midpoint(f);
				vertices.Add(mid);
				for(int i = 0; i < f.Count; ++i)
					faces.Add(new() { mid, f[i], f[(i + 1) % f.Count] });
			}
		}
	}
}
