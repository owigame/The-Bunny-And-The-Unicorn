using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace XORHEAD.CodeTemplates
{
	public class DoCreateCodeFile : EndNameEditAction
	{
		public override void Action (int instanceId, string pathName, string resourceFile)
		{
			Object o = CodeTemplates.CreateScript(pathName, resourceFile);
			ProjectWindowUtil.ShowCreatedAsset (o);
		}
	}

	/// <summary>
	/// Editor class for creating code files from templates.
	/// </summary>
	public class CodeTemplates 
	{
		/// <summary>
		/// The C# script icon.
		/// </summary>
		private static Texture2D scriptIcon = (EditorGUIUtility.IconContent ("cs Script Icon").image as Texture2D);
		
		internal static UnityEngine.Object CreateScript(string pathName, string templatePath)
		{
			string newFilePath = Path.GetFullPath (pathName);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension (pathName);
			string className = NormalizeClassName(fileNameWithoutExtension);

			string templateText = string.Empty;

			if (File.Exists(templatePath))
			{
				using (var sr = new StreamReader (templatePath))
				{
					templateText = sr.ReadToEnd();
				}

				templateText = templateText.Replace ("##NAME##", className);
				
				UTF8Encoding encoding = new UTF8Encoding (true, false);
				
				using (var sw = new StreamWriter (newFilePath, false, encoding))
				{
					sw.Write (templateText);
				}
				
				AssetDatabase.ImportAsset (pathName);
				
				return AssetDatabase.LoadAssetAtPath (pathName, typeof(Object));
			}
			else
			{
				Debug.LogError(string.Format("The template file was not found: {0}", templatePath));

				return null;
			}
		}

		private static string NormalizeClassName(string fileName)
		{
			return fileName.Replace(" ", string.Empty);
		}

		/// <summary>
		/// Creates a new code file from a template file.
		/// </summary>
		/// <param name="initialName">The initial name to give the file in the UI</param>
		/// <param name="templatePath">The full path of the template file to use</param>
		public static void CreateFromTemplate(string initialName, string templatePath)
		{
			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance<DoCreateCodeFile>(),
				initialName,
				scriptIcon,
				templatePath);
		}
	}
}