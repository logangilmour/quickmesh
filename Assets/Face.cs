
using System;
using System.Collections.Generic;
using UnityEngine;
namespace QuickMesh
{
		public class Face
		{
			public static Vector3 DefaultOrientation = Vector3.left;	
			public static Vector3 DefaultNormal = Vector3.forward;
		
			public List<Vertex> Vertices = new List<Vertex>();

			public Vector3 Orientation = DefaultOrientation;

			public Vector3 Normal(){
				Vector3 p0 = Vertices[0].Position;
				Vector3 p1 = Vertices[1].Position;
				Vector3 p2 = Vertices[2].Position;
				return Vector3.Cross((p2 - p1), (p0 - p1)).normalized;
			}

		public Vector3 Barycenter(){
						var barycenter = Vector3.zero;
						foreach (Vertex v in Vertices) {
								barycenter += v.Position;
						}
						return barycenter / Vertices.Count;
				}
		
	}
}

