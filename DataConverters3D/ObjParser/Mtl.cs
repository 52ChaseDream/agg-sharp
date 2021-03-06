/*  The MIT License(MIT)

//  Copyright(c) 2015 Stefan Gordon

//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:

//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.

//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
*/

using ObjParser.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ObjParser
{
	public class Mtl
	{
		public List<ObjMaterial> MaterialList;

		/// <summary>
		/// Constructor. Initializes VertexList, FaceList and TextureList.
		/// </summary>
		public Mtl()
		{
			MaterialList = new List<ObjMaterial>();
		}

		/// <summary>
		/// Load .obj from a filepath.
		/// </summary>
		/// <param name="file"></param>
		public void LoadMtl(string path)
		{
			LoadMtl(File.ReadAllLines(path));
		}

		/// <summary>
		/// Load .obj from a stream.
		/// </summary>
		/// <param name="file"></param>
		public void LoadMtl(Stream data)
		{
			using (var reader = new StreamReader(data))
			{
				LoadMtl(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
			}
		}

		/// <summary>
		/// Load .mtl from a list of strings.
		/// </summary>
		/// <param name="data"></param>
		public void LoadMtl(IEnumerable<string> data)
		{
			foreach (var line in data)
			{
				processLine(line);
			}
		}

		public void WriteMtlFile(string path, string[] headerStrings)
		{
			using (var outStream = File.OpenWrite(path))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
				WriteHeader(writer, headerStrings);

				MaterialList.ForEach(v => writer.WriteLine(v));
			}
		}

		private ObjMaterial currentMaterial()
		{
			if (MaterialList.Count > 0) return MaterialList.Last();
			return new ObjMaterial();
		}

		/// <summary>
		/// Parses and loads a line from an OBJ file.
		/// Currently only supports V, VT, F and MTLLIB prefixes
		/// </summary>
		private void processLine(string line)
		{
			string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0)
			{
				ObjMaterial CurrentMaterial = currentMaterial();
				ObjColor c = new ObjColor();
				switch (parts[0].Trim())
				{
					case "d":
						CurrentMaterial.Dissolve = float.Parse(parts[1]);
						break;

					case "illum":
						CurrentMaterial.IlluminationModel = int.Parse(parts[1]);
						break;

					case "Ka":
						c.LoadFromStringArray(parts);
						CurrentMaterial.AmbientReflectivity = c;
						break;

					case "Kd":
						c.LoadFromStringArray(parts);
						CurrentMaterial.DiffuseReflectivity = c;
						break;

					case "Ks":
						c.LoadFromStringArray(parts);
						CurrentMaterial.SpecularReflectivity = c;
						break;

					case "Ke":
						c.LoadFromStringArray(parts);
						CurrentMaterial.EmissiveCoefficient = c;
						break;

					case "map_Kd":
						CurrentMaterial.DiffuseTextureFileName = parts[1];
						break;

					case "Ni":
						CurrentMaterial.OpticalDensity = float.Parse(parts[1]);
						break;

					case "Ns":
						CurrentMaterial.SpecularExponent = float.Parse(parts[1]);
						break;

					case "newmtl":
						CurrentMaterial = new ObjMaterial();
						CurrentMaterial.Name = parts[1];
						MaterialList.Add(CurrentMaterial);
						break;

					case "Tf":
						c.LoadFromStringArray(parts);
						CurrentMaterial.TransmissionFilter = c;
						break;
				}
			}
		}

		private void WriteHeader(StreamWriter writer, string[] headerStrings)
		{
			if (headerStrings == null || headerStrings.Length == 0)
			{
				writer.WriteLine("# Generated by ObjParser");
				return;
			}

			foreach (var line in headerStrings)
			{
				writer.WriteLine("# " + line);
			}
		}
	}
}