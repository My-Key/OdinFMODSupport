using FMODUnity;
using OdinFMOD;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Validation;

[assembly: RegisterValidator(typeof(EventReferenceValidator))]

namespace OdinFMOD
{
	public class EventReferenceValidator : ValueValidator<EventReference>
	{
		private static readonly string PATH = $"Fix is in <b>context menu</b> or in <b>{EventReferenceUpdater.MenuPath}</b>";

		protected override void Validate(ValidationResult result)
		{
			var editorEventRef = OdinEventReferenceDrawer.GetEditorEventRef(Value);
			var eventReference = Value;

			if (editorEventRef != null)
			{
				if (EventManager.GetEventLinkage(eventReference) == EventLinkage.Path)
				{
					if (eventReference.Guid != editorEventRef.Guid)
					{
						result.AddError(
								$"GUID doesn't match path. {PATH}")
							.WithFix(Fix.Create("Fix GUID", FixGUIDMismatch))
							.WithContextClick("Fix GUID", FixGUIDMismatch);
					}
				}
				else // EventLinkage.GUID
				{
					if (eventReference.Path != editorEventRef.Path)
					{
						result.AddError(
								$"Path doesn't match GUID. {PATH}")
							.WithFix(Fix.Create("Fix Path", FixPathMismatch))
							.WithContextClick("Fix Path", FixPathMismatch);
					}
				}
			}
			else
			{
				EditorEventRef renamedEvent = GetRenamedEventRef(Value);

				if (renamedEvent != null)
				{
					result.AddError(
							$"Moved to {renamedEvent.Path}. {PATH}")
						.WithFix(Fix.Create("Fix rename", FixRename))
						.WithContextClick("Fix rename", FixRename);
				}
				else
					result.AddWarning("Event Not Found");
			}
		}
		
		public static EditorEventRef GetRenamedEventRef(EventReference eventReference)
		{
			if (Settings.Instance.EventLinkage != EventLinkage.Path || eventReference.Guid.IsNull)
				return null;
			
			EditorEventRef editorEventRef = EventManager.EventFromGUID(eventReference.Guid);

			return editorEventRef != null && editorEventRef.Path != eventReference.Path ? editorEventRef : null;
		}

		private void FixRename() => FixRename(ValueEntry);

		public static void FixRename(IPropertyValueEntry<EventReference> valueEntry)
		{
			EditorEventRef renamedEvent = GetRenamedEventRef(valueEntry.SmartValue);
			var val = valueEntry.SmartValue;
			val.Path = renamedEvent.Path;
			valueEntry.SmartValue = val;
		}

		private void FixGUIDMismatch() => FixGUIDMismatch(ValueEntry);

		public static void FixGUIDMismatch(IPropertyValueEntry<EventReference> valueEntry)
		{
			var editorEventRef = OdinEventReferenceDrawer.GetEditorEventRef(valueEntry.SmartValue);
			var val = valueEntry.SmartValue;
			val.Guid = editorEventRef.Guid;
			valueEntry.SmartValue = val;
		}

		private void FixPathMismatch() => FixPathMismatch(ValueEntry);

		public static void FixPathMismatch(IPropertyValueEntry<EventReference> valueEntry)
		{
			var editorEventRef = OdinEventReferenceDrawer.GetEditorEventRef(valueEntry.SmartValue);
			var val = valueEntry.SmartValue;
			val.Path = editorEventRef.Path;
			valueEntry.SmartValue = val;
		}
	}
}