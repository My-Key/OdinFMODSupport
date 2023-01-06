using System;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

namespace OdinFMOD
{
	public class OdinParamRefDrawer : OdinFMODRefDrawerBase<ParamRefAttribute, EditorParamRef>
	{
		private static readonly Texture2D CONTINUOUS_PARAMETER_ICON = EditorUtils.LoadImage("ContinuousParameterIcon.png");
		private static readonly Texture2D DISCRETE_PARAMETER_ICON = EditorUtils.LoadImage("DiscreteParameterIcon.png");
		private static readonly Texture2D LABELED_PARAMETER_ICON = EditorUtils.LoadImage("LabeledParameterIcon.png");
		
		private const string PARAMETER_PREFIX = "parameter:/";

		protected override void SetValueAfterDrop(EditorParamRef newEditorBankRef)
		{
			ValueEntry.SmartValue = newEditorBankRef.Name;
		}

		protected override EditorParamRef GetEditorRef(string path)
		{
			return EventManager.Parameters.FirstOrDefault(x => x.Name == path);
		}

		protected override  void SelectorOnSelectionConfirmed(IEnumerable<EditorParamRef> selected)
		{
			if (!selected.Any())
				return;

			ValueEntry.SmartValue = selected.First().Name;
		}

		protected override  Texture GetIcon(EditorParamRef arg) =>
			arg.Type switch
			{
				ParameterType.Continuous => CONTINUOUS_PARAMETER_ICON,
				ParameterType.Discrete => DISCRETE_PARAMETER_ICON,
				_ => LABELED_PARAMETER_ICON
			};

		protected override  string GetPath(EditorParamRef arg) => arg.StudioPath.Replace(PARAMETER_PREFIX, "Global Parameters/");
		protected override List<EditorParamRef> GetCollection() => EventManager.Parameters;
		protected override string SelectorTitle() => "FMOD Global Parameter selector";
	}
}