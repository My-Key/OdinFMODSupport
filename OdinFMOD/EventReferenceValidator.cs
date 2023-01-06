using FMODUnity;
using OdinFMOD;
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
							.WithFix(FixGUIDMismatch)
							.WithContextClick("Fix GUID", FixGUIDMismatch);
					}
				}
				else // EventLinkage.GUID
				{
					if (eventReference.Path != editorEventRef.Path)
					{
						result.AddError(
								$"Path doesn't match GUID. {PATH}")
							.WithFix(FixPathMismatch)
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
						.WithFix(FixRename)
						.WithContextClick("Fix rename", FixRename);
				}
				else
					result.AddWarning("Event Not Found");
			}
		}
		
		private static EditorEventRef GetRenamedEventRef(EventReference eventReference)
		{
			if (Settings.Instance.EventLinkage != EventLinkage.Path || eventReference.Guid.IsNull)
				return null;
			
			EditorEventRef editorEventRef = EventManager.EventFromGUID(eventReference.Guid);

			return editorEventRef != null && editorEventRef.Path != eventReference.Path ? editorEventRef : null;
		}

		private void FixRename()
		{
			EditorEventRef renamedEvent = GetRenamedEventRef(Value);
			var val = ValueEntry.SmartValue;
			val.Path = renamedEvent.Path;
			ValueEntry.SmartValue = val;
		}
		
		private void FixGUIDMismatch()
		{
			var editorEventRef = OdinEventReferenceDrawer.GetEditorEventRef(Value);
			var val = ValueEntry.SmartValue;
			val.Guid = editorEventRef.Guid;
			ValueEntry.SmartValue = val;
		}

		private void FixPathMismatch()
		{
			var editorEventRef = OdinEventReferenceDrawer.GetEditorEventRef(Value);
			var val = ValueEntry.SmartValue;
			val.Path = editorEventRef.Path;
			ValueEntry.SmartValue = val;
		}
	}
}