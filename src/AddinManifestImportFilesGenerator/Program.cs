//
// Program.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace AddinManifestImportFilesGenerator
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try {
				Run ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		static void Run ()
		{
			var settings = new XmlWriterSettings {
				Indent = true,
				IndentChars = "\t\t",
				NewLineChars = "\r\n",
				OmitXmlDeclaration = true
			};

			var xml = new StringBuilder ();
			using (var stringWriter = new StringWriter (xml)) {
				using (var writer = XmlWriter.Create (stringWriter, settings)) {
					writer.WriteStartElement ("Runtime");

					WriteImportAssemblyElement (writer, "MonoDevelop.PackageManagement.Extensions.dll");
					WriteImportFileElement (writer, "MonoDevelop.EnvDTE.dll");
					WriteImportFileElement (writer, "MonoDevelop.PackageManagement.PowerShell.Protocol.dll");
					WriteImportFileElement (writer, "NuGet.Core.dll");
					WriteImportFileElement (writer, "NuGet.Resolver.dll");
					WriteImportFileElement (writer, "NuGet-LICENSE.txt");
					WriteImportFileElement (writer, "StreamJsonRpc.dll");

					WritePowerShellHostImports (writer);

					writer.WriteEndElement ();
				}
			}

			Console.WriteLine (xml);
		}

		static void WriteImportAssemblyElement (XmlWriter writer, string assembly)
		{
			writer.WriteStartElement ("Import");
			writer.WriteAttributeString ("assembly", assembly);
			writer.WriteEndElement ();
		}

		static void WriteImportFileElement (XmlWriter writer, string file)
		{
			writer.WriteStartElement ("Import");
			writer.WriteAttributeString ("file", file);
			writer.WriteEndElement ();
		}

		static void WritePowerShellHostImports (XmlWriter writer)
		{
			string applicationDirectory = Path.GetDirectoryName (typeof (MainClass).Assembly.Location);
			string publishDirectory = Path.Combine (applicationDirectory, "..", "..", "..", "bin", "PowerShellConsoleHost", "publish");
			publishDirectory = Path.GetFullPath (publishDirectory);

			if (!Directory.Exists (publishDirectory)) {
				throw new ApplicationException (
					string.Format ("Publish directory does not exist '{0}'", publishDirectory)
				);
			}

			var files = new List<string> ();
			AddFiles (files, publishDirectory, SearchOption.TopDirectoryOnly);

			string modulesDirectory = Path.Combine (publishDirectory, "Modules");
			AddFiles (files, modulesDirectory, SearchOption.AllDirectories);

			string runtimeDirectory = Path.Combine (publishDirectory, "runtimes", "osx");
			AddFiles (files, runtimeDirectory, SearchOption.AllDirectories);

			runtimeDirectory = Path.Combine (publishDirectory, "runtimes", "unix");
			AddFiles (files, runtimeDirectory, SearchOption.AllDirectories);

			files.Sort ();

			foreach (string file in files) {
				string relativePath = file.Substring (publishDirectory.Length + 1);
				relativePath = Path.Combine ("PowerShellConsoleHost", relativePath);
				WriteImportFileElement (writer, relativePath);
			}
		}

		static void AddFiles (List<string> files, string publishDirectory, SearchOption option)
		{
			foreach (string file in Directory.EnumerateFiles (publishDirectory, "*", option)) {
				string extension = Path.GetExtension (file);
				if (!IsExcluded (extension)) {
					files.Add (file);
				}
			}
		}

		static bool IsExcluded (string extension)
		{
			return StringComparer.OrdinalIgnoreCase.Equals (".pdb", extension) ||
				StringComparer.OrdinalIgnoreCase.Equals (".DS_Store", extension);
		}
	}
}
