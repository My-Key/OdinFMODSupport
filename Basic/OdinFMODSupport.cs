using FMODUnity;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using UnityEngine;

public class FMODEventDrawer : DrawWithUnityBaseDrawer<EventReference>
{
	private bool m_init;
	
	protected override void Initialize()
	{
		base.Initialize();

		m_init = true;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		UpdateValuesAfterExternalChange(ref m_init, Property, ValueEntry);

		base.DrawPropertyLayout(label);
	}
	

	public static void UpdateValuesAfterExternalChange<T>(ref bool init, InspectorProperty inspectorProperty, 
		IPropertyValueEntry<T> valueEntry)
	{
		if (init)
		{
			init = false;
			return;
		}
		
		var serializedProperty = inspectorProperty.Tree.GetUnityPropertyForPath(inspectorProperty.Path, out _);

		if (serializedProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
		{
			var targetObjects = serializedProperty.serializedObject.targetObjects;

			for (int index = 0; index < targetObjects.Length; ++index)
				valueEntry.Values[index] = ((EmittedScriptableObject<T>)targetObjects[index]).GetValue();
		}
	}
}

public abstract class RefAttributeDrawerBase<T> : OdinAttributeDrawer<T, string> where T : System.Attribute
{
	private bool m_init;
	
	protected override void Initialize()
	{
		base.Initialize();

		m_init = true;
	}

	protected override void DrawPropertyLayout(GUIContent label)
	{
		FMODEventDrawer.UpdateValuesAfterExternalChange(ref m_init, Property, ValueEntry);

		CallNextDrawer(label);
	}
}

[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class ParamRefOdinDrawer : RefAttributeDrawerBase<ParamRefAttribute> { }

[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class BankRefDrawer : RefAttributeDrawerBase<BankRefAttribute> { }

[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
public class EventRefDrawer : RefAttributeDrawerBase<EventRefAttribute> { }
