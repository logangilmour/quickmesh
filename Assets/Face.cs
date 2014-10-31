
using System;
using System.Collections.Generic;
using UnityEngine;
namespace QuickMesh
{
		public class Face
		{
			public static Vector3 DefaultOrientation = Vector3.left;	
			public static Vector3 DefaultNormal = Vector3.forward;
		
			public List<Vertex> Vertices;

			public Vector3 Normal = DefaultNormal;
			public Vector3 Orientation = DefaultOrientation;
		
	}
}

