using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using GUID = FMOD.GUID;

namespace OdinFMOD
{
	public class OdinEventReferenceDrawer : OdinValueDrawer<EventReference>
	{
		public static readonly Texture BROWSE_ICON = EditorUtils.LoadImage("SearchIconBlack.png");
		private static readonly Texture OPEN_ICON = EditorUtils.LoadImage("BrowserIcon.png");
		private static readonly Texture ADD_ICON = EditorUtils.LoadImage("AddIcon.png");
		private static readonly Texture COPY_ICON = EditorUtils.LoadImage("CopyIcon.png");
		
		private static readonly Texture2D EVENT_ICON = EditorUtils.LoadImage("EventIcon.png");
		private static readonly Texture2D SNAPSHOT_ICON = EditorUtils.LoadImage("SnapshotIcon.png");
		
		private static readonly Color PREFAB_CHANGE_COLOR = new Color(0.003921569f, 0.6f, 0.9215686f, 0.75f);
		
		private const string EVENT_PREFIX = "event:/";
		private const string SNAPSHOT_PREFIX = "snapshot:/";

		public static GUIStyle m_buttonStyle;
		public static GUIStyle m_textStyle;

		private InspectorProperty m_path;

		protected override void Initialize()
		{
			InitStyles();

			m_path = Property.Children["Path"];
		}

		public static void InitStyles()
		{
			if (m_buttonStyle == null)
			{
				m_buttonStyle = new GUIStyle(GUI.skin.button);
				m_buttonStyle.padding.top = 3;
				m_buttonStyle.padding.bottom = 3;
				m_buttonStyle.padding.left = 1;
				m_buttonStyle.padding.right = 1;
			}

			if (m_textStyle == null)
			{
				m_textStyle = new GUIStyle(GUI.skin.textField);
				m_textStyle.padding.left = 25;
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			var eventRef = ValueEntry.SmartValue;
			var editorEventRef = GetEditorEventRef(eventRef);
			var isSet = editorEventRef != null;

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.BeginHorizontal();
			
			SirenixEditorGUI.BeginIndentedHorizontal();
			
			var type = Event.current.type;
			var boldLabel = ValueEntry != null && type == EventType.Repaint;
			var valueChanged = false;
			
			if (boldLabel)
			{
				valueChanged = m_path.ValueEntry.ValueChangedFromPrefab;
				var isBold = valueChanged;
				
				if (GUIHelper.IsDrawingDictionaryKey)
					isBold |= GUIHelper.IsBoldLabel;
				
				GUIHelper.PushIsBoldLabel(isBold);
			}

			Rect rect;
			
			if (isSet)
			{
				Property.State.Expanded = SirenixEditorGUI.Foldout(Property.State.Expanded, label, out rect);
				label = null;
			}
			else
			{
				rect = EditorGUILayout.GetControlRect(label != null);
				
				if (label!= null)
					rect = EditorGUI.PrefixLabel(rect, label);
			}
			  
			eventRef.Path = SirenixEditorFields.TextField(rect, null, eventRef.Path, editorEventRef != null ? m_textStyle : null);

			if (boldLabel)
				GUIHelper.PopIsBoldLabel();

			if (valueChanged)
			{
				if (type == EventType.Repaint)
				{
					if (GlobalConfig<GeneralDrawerConfig>.Instance.ShowPrefabModifiedValueBar)
					{
						var lastDrawnValueRect = Property.LastDrawnValueRect;

						if (lastDrawnValueRect != new Rect())
						{
							Rect lastDrawnValueRect2 = Property.LastDrawnValueRect;
							lastDrawnValueRect2.width = 2f;
							lastDrawnValueRect2.x -= 2.5f;
							lastDrawnValueRect2.x += GUIHelper.CurrentIndentAmount;
							
							GUIHelper.PushGUIEnabled(true);
							SirenixEditorGUI.DrawSolidRect(lastDrawnValueRect2, PREFAB_CHANGE_COLOR);
							GUIHelper.PopGUIEnabled();
						}
					}
				}
			}


			if (isSet)
			{
				var iconRect = rect;
				iconRect.width = 25;
				iconRect.height = 18;
				GUI.DrawTexture(iconRect, GetIcon(editorEventRef), ScaleMode.ScaleToFit);
			}

			var dropRect = rect;
			
			if (label != null)
				dropRect.xMin += GUIHelper.ActualLabelWidth;
			
			var newEditorEventRef = DragAndDropUtilities.DropZone(dropRect, editorEventRef);

			if (newEditorEventRef != editorEventRef)
			{
				eventRef.Path = newEditorEventRef.Path;
				eventRef.Guid = newEditorEventRef.Guid;
			}
			
			if (SirenixEditorGUI.IconButton(BROWSE_ICON, m_buttonStyle, 20, tooltip: PathToTooltip(eventRef.Path)))
			{
				var selector = new GenericSelector<EditorEventRef>("FMOD Event selector", false);
				
				selector.SelectionTree.AddRange(EventManager.Events, GetPath, GetIcon);
				selector.SetSelection(editorEventRef);
				
				selector.SelectionConfirmed += SelectorOnSelectionConfirmed;

				selector.ShowInPopup();
			}
			
			if (isSet && SirenixEditorGUI.IconButton(OPEN_ICON, m_buttonStyle, 20))
			{
				EventBrowser.ShowWindow();
				EventBrowser eventBrowser = EditorWindow.GetWindow<EventBrowser>();
				eventBrowser.FrameEvent(eventRef.Path);
			}
			
			if (SirenixEditorGUI.IconButton(ADD_ICON, m_buttonStyle, 20))
			{
				var eventCreator = new CreateFMODEventPopup();
				eventCreator.ShowInPopup();
				eventCreator.OnEventCreated += EventCreated;
			}
			
			SirenixEditorGUI.EndIndentedHorizontal();
			
			EditorGUILayout.EndHorizontal();
			
			ShowDetails(isSet, eventRef, editorEventRef);

			if (EditorGUI.EndChangeCheck()) 
				ValueEntry.SmartValue = eventRef;
		}

		private void EventCreated(GUID guid, string path)
		{
			var value = ValueEntry.SmartValue;
			value.Guid = guid;
			value.Path = path;
			ValueEntry.SmartValue = value;
		}

		private void SelectorOnSelectionConfirmed(IEnumerable<EditorEventRef> selected)
		{
			if (!selected.Any())
				return;
			
			var value = ValueEntry.SmartValue;
			var chosen = selected.First();
			value.Path = chosen.Path;
			value.Guid = chosen.Guid;
			ValueEntry.SmartValue = value;
		}

		private Texture GetIcon(EditorEventRef eventRef) => eventRef.Path.StartsWith(SNAPSHOT_PREFIX) ? SNAPSHOT_ICON : EVENT_ICON;

		private string GetPath(EditorEventRef eventRef) =>
			eventRef.Path.Replace(EVENT_PREFIX, "Events/").Replace(SNAPSHOT_PREFIX, "Snapshots/");

		public static string PathToTooltip(string path)
		{
			if (string.IsNullOrEmpty(path))
				return path;
			return path.Replace("/", "\n↳");
		}

		private void ShowDetails(bool isSet, EventReference eventReference,  EditorEventRef editorEventRef)
		{
			if (!isSet)
				return;
			
			GUIHelper.PushIndentLevel(EditorGUI.indentLevel + 1);

			if (SirenixEditorGUI.BeginFadeGroup(this, Property.State.Expanded))
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("GUID", editorEventRef.Guid.ToString());

				if (SirenixEditorGUI.IconButton(COPY_ICON, m_buttonStyle, 20, tooltip:"Copy To Clipboard"))
				{
					EditorGUIUtility.systemCopyBuffer = eventReference.Guid.ToString();
				}

				EditorGUILayout.EndHorizontal();

				EditorGUILayout.LabelField("Banks",
					string.Join(", ", editorEventRef.Banks.Select(x => x.Name).ToArray()));

				EditorGUILayout.LabelField("Panning", editorEventRef.Is3D ? "3D" : "2D");

				EditorGUILayout.LabelField("Stream", editorEventRef.IsStream.ToString());

				EditorGUILayout.LabelField("Oneshot", editorEventRef.IsOneShot.ToString());

				EditorGUILayout.EndVertical();
			}
			
			SirenixEditorGUI.EndFadeGroup();

			GUIHelper.PopIndentLevel();
		}

		public static EditorEventRef GetEditorEventRef(EventReference eventReference)
		{
			if (EventManager.GetEventLinkage(eventReference) == EventLinkage.Path)
			{
				return EventManager.EventFromPath(eventReference.Path ?? string.Empty);
			}
			else // Assume EventLinkage.GUID
			{
				return EventManager.EventFromGUID(eventReference.Guid);
			}
		}
	}
}