using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Easy2D
{

	public class PortListEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.Modal;
		}

		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
		{
			IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (editorService != null)
			{
				object additionalObject = context.Instance;
				GameObject testInstance = context.Instance as GameObject;
				using (PortListEditorWindow customForm = new PortListEditorWindow(value, testInstance))
				{
					if (editorService.ShowDialog(customForm) == DialogResult.OK)
					{
						value = customForm.self;
					}
				}
			}
			return value;
		}
	}
}
