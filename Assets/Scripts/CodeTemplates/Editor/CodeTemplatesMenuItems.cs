using UnityEditor;
using UnityEngine;

namespace XORHEAD.CodeTemplates
{
	/// <summary>
	/// Definitions for all the Code templates menu items.
	/// </summary>
	public class CodeTemplatesMenuItems
	{
		/// <summary>
		/// The root path for code templates menu items.
		/// </summary>
		private const string MENU_ITEM_PATH = "Assets/Create/";

		/// <summary>
		/// Menu items priority (so they will be grouped/shown next to existing scripting menu items).
		/// </summary>
		private const int MENU_ITEM_PRIORITY = 75;
			
		/// <summary>
		/// Creates a new C# class.
		/// </summary>
		[MenuItem(MENU_ITEM_PATH + "AI Script", false, MENU_ITEM_PRIORITY)]
		private static void CreateClass()
		{
			CodeTemplates.CreateFromTemplate(
				"NewAI.cs", 
				@"Assets/scripts/CodeTemplates/Editor/Templates/AITemplate.txt");
		}
	}
}