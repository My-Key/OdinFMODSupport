using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace OdinFMOD
{
	public abstract class OdinFMODRefDrawerBase<TAttribute, TEditorRef> : OdinAttributeDrawer<TAttribute, string> 
		where TAttribute : System.Attribute where TEditorRef : ScriptableObject
	{
		protected override void Initialize()
		{
			OdinEventReferenceDrawer.InitStyles();
		}
		
		protected override void DrawPropertyLayout(GUIContent label)
		{
			EditorGUILayout.BeginHorizontal();
			
			var editorRef = GetEditorRef(ValueEntry.SmartValue);
			
			Rect rect = EditorGUILayout.GetControlRect(label != null);

			ValueEntry.SmartValue = SirenixEditorFields.TextField(rect, label, ValueEntry.SmartValue,
				editorRef != null ? OdinEventReferenceDrawer.m_textStyle : null);
			
			if (editorRef != null)
			{
				var iconRect = rect;
				iconRect.xMin += GUIHelper.ActualLabelWidth;
				iconRect.width = 25;
				EditorGUI.LabelField(iconRect, new GUIContent(GetIcon(editorRef)), OdinEventReferenceDrawer.m_iconStyle);
			}

			var dropRect = rect;
			
			if (label != null)
				dropRect.xMin += GUIHelper.ActualLabelWidth;

			var newEditorRef = DragAndDropUtilities.DropZone(dropRect, editorRef);

			if (newEditorRef != editorRef)
			{
				SetValueAfterDrop(newEditorRef);
			}
			
			if (SirenixEditorGUI.IconButton(OdinEventReferenceDrawer.BROWSE_ICON, OdinEventReferenceDrawer.m_buttonStyle, 20))
			{
				var selector = new GenericSelector<TEditorRef>(SelectorTitle(), false);
				
				selector.SelectionTree.AddRange(GetCollection(), GetPath, GetIcon);
				selector.SetSelection(editorRef);
				
				selector.SelectionConfirmed += SelectorOnSelectionConfirmed;

				selector.ShowInPopup();
			}
			
			EditorGUILayout.EndHorizontal();
		}

		protected abstract void SetValueAfterDrop(TEditorRef newEditorBankRef);

		protected abstract TEditorRef GetEditorRef(string path);

		protected abstract void SelectorOnSelectionConfirmed(IEnumerable<TEditorRef> selected);
		
		protected abstract Texture GetIcon(TEditorRef arg);
		
		protected abstract string GetPath(TEditorRef arg);

		protected abstract List<TEditorRef> GetCollection();

		protected abstract string SelectorTitle();
	}
}