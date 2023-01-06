using System;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using GUID = FMOD.GUID;

namespace OdinFMOD
{
	public class CreateFMODEventPopup
	{
		private class FolderEntry
		{
			public string name;
			public string guid;
			public List<FolderEntry> entries = new List<FolderEntry>();
		}
		
		private class BankEntry
		{
			public string name;
			public string guid;
		}

		private FolderEntry m_rootFolder;
		
		private GenericSelector<string> m_folderSelector;
		private GenericSelector<string> m_bankSelector;

		private string m_name;
		private string m_folder = "/";
		private int m_bank;
		
		private List<BankEntry> m_banks;

		public event Action<GUID, string> OnEventCreated;

		[Sirenix.OdinInspector.OnInspectorInit]
		protected void Initialize()
		{
			var rootGuid = EditorUtils.GetScriptOutput("studio.project.workspace.masterEventFolder.id");
			m_rootFolder = new FolderEntry();
			m_rootFolder.guid = rootGuid;
			
			BuildTreeItem(m_rootFolder);

			var foldersList = new List<string>();
			FoldersToPaths(foldersList, m_rootFolder, "");
			m_folderSelector = new GenericSelector<string>(foldersList);
			m_folderSelector.EnableSingleClickToSelect();
			m_folderSelector.SelectionConfirmed += FolderSelectorOnSelectionConfirmed;
			
			BuildBankList();
		}

		private void BuildBankList()
		{
			m_banks = new List<BankEntry>();

			const string buildBankTreeFunc =
				@"function() {
                    var output = """";
                    const items = [ studio.project.workspace.masterBankFolder ];
                    while (items.length > 0) {
                        var currentItem = items.shift();
                        if (currentItem.isOfType(""BankFolder"")) {
                            currentItem.items.reverse().forEach(function(val) {
                                items.unshift(val);
                            });
                        } else {
                            output += "","" + currentItem.id + currentItem.getPath().replace(""bank:/"", """");
                        }
                    }
                    return output;
                }";

			string bankList = EditorUtils.GetScriptOutput(string.Format("({0})()", buildBankTreeFunc));
			string[] bankListSplit = bankList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var bank in bankListSplit)
			{
				var entry = new BankEntry();
				entry.guid = bank.Substring(0, 38);
				entry.name = bank.Substring(38);
				m_banks.Add(entry);
			}
		}

		private void FolderSelectorOnSelectionConfirmed(IEnumerable<string> obj)
		{
			m_folder = obj.FirstOrDefault();
		}

		private void FoldersToPaths(List<string> paths, FolderEntry entry, string prefix)
		{
			foreach (var child in entry.entries)
			{
				FoldersToPaths(paths, child, $"{prefix}{entry.name}/");
				paths.Add($"{prefix}{entry.name}/{child.name}");
			}
		}
		
		private void BuildTreeItem(FolderEntry entry)
		{
			// lookup the entry
			EditorUtils.GetScriptOutput(string.Format("cur = studio.project.lookup(\"{0}\");", entry.guid));

			// get child count
			string itemCountString = EditorUtils.GetScriptOutput("cur.items.length;");
			int itemCount;
			Int32.TryParse(itemCountString, out itemCount);
            
			// iterate children looking for folder
			for (int item = 0; item < itemCount; item++)
			{
				EditorUtils.GetScriptOutput(String.Format("child = cur.items[{0}]", item));

				// check if it's a folder
				string isFolder = EditorUtils.GetScriptOutput("child.isOfExactType(\"EventFolder\")");
				if (isFolder == "false")
				{
					continue;
				}

				// Get guid and name
				string info = EditorUtils.GetScriptOutput("child.id + child.name");

				var childEntry = new FolderEntry();
				childEntry.guid = info.Substring(0, 38);
				childEntry.name = info.Substring(38);
				entry.entries.Add(childEntry);
			}

			// Recurse for child entries
			foreach(var childEntry in entry.entries)
			{
				BuildTreeItem(childEntry);
			}
		}

		[Sirenix.OdinInspector.OnInspectorGUI]
		protected void DrawGUI()
		{
		 	GUIHelper.PushGUIEnabled(!string.IsNullOrWhiteSpace(m_name));
			
			if (GUILayout.Button("Create event"))
			{
				CreateEventInStudio();
			}
			
			GUIHelper.PopGUIEnabled();
			
			m_name = SirenixEditorFields.TextField("Name", m_name);
			
			EditorGUI.BeginChangeCheck();
			m_folder = SirenixEditorFields.TextField("Folder", m_folder);
			
			if (EditorGUI.EndChangeCheck())
				m_folderSelector.SetSelection(m_folder);

			var bankNames = m_banks.Select(x => x.name).ToArray();
			m_bank = SirenixEditorFields.Dropdown("Bank", m_bank, bankNames);
			
			m_folderSelector.OnInspectorGUI();
		}
		
		private void CreateEventInStudio()
		{
			string eventGuid = EditorUtils.CreateStudioEvent(m_folder, m_name);

			if (!string.IsNullOrEmpty(eventGuid))
			{
				EditorUtils.GetScriptOutput(String.Format("studio.project.lookup(\"{0}\").relationships.banks.add(studio.project.lookup(\"{1}\"));", eventGuid, m_banks[m_bank].guid));
				EditorUtils.GetScriptOutput("studio.project.build();");

				if (!m_folder.EndsWith("/"))
				{
					m_folder += "/";
				}

				string fullPath = "event:" + m_folder + m_name;
				var guid = FMOD.GUID.Parse(eventGuid);
				
				OnEventCreated?.Invoke(guid, fullPath);
			}
		}
		
		public OdinEditorWindow ShowInPopup()
		{
			EditorWindow focusedWindow = EditorWindow.focusedWindow;
			float windowWidth = 0.0f;
			OdinEditorWindow window = windowWidth != 0.0f ? OdinEditorWindow.InspectObjectInDropDown((object) this, windowWidth) : OdinEditorWindow.InspectObjectInDropDown(this);
			
			return window;
		}
	}
}