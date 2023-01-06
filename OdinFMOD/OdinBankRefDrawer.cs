using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

namespace OdinFMOD
{
	public class OdinBankRefDrawer : OdinFMODRefDrawerBase<BankRefAttribute, EditorBankRef>
	{
		private static readonly Texture2D BANK_ICON = EditorUtils.LoadImage("BankIcon.png");
		
		
		private const string BANK_PREFIX = "bank:/";

		protected override void Initialize()
		{
			OdinEventReferenceDrawer.InitStyles();
		}

		protected override void SetValueAfterDrop(EditorBankRef newEditorBankRef)
		{
			ValueEntry.SmartValue = newEditorBankRef.Name;
		}

		protected override EditorBankRef GetEditorRef(string path)
		{
			return EventManager.Banks.FirstOrDefault(x => x.Name == path);
		}

		protected override void SelectorOnSelectionConfirmed(IEnumerable<EditorBankRef> selected)
		{
			if (!selected.Any())
				return;

			ValueEntry.SmartValue = selected.First().Name;
		}

		protected override Texture GetIcon(EditorBankRef arg) => BANK_ICON;

		protected override string GetPath(EditorBankRef arg) => arg.StudioPath.Replace(BANK_PREFIX, string.Empty);
		
		protected override List<EditorBankRef> GetCollection() => EventManager.Banks;
		protected override string SelectorTitle() => "FMOD Bank selector";
	}
}