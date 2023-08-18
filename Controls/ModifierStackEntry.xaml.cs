using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Modification;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TerrainFactoryApp.Controls
{
	/// <summary>
	/// Interaction logic for ModifierStackEntry.xaml
	/// </summary>
	public partial class ModifierStackEntry : UserControl, INotifyPropertyChanged
	{

		public static readonly List<Type> supportedTypes = new List<Type>()
		{
			typeof(string),
			typeof(byte),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(float),
			typeof(double)
		};

		internal Modifier mod;

		MainWindow parentWindow;
		ModificationChain modificationChain;

		public event PropertyChangedEventHandler PropertyChanged;

		private int stackIndex;
		internal int StackIndex
		{
			get
			{
				return stackIndex;
			}
			set
			{
				stackIndex = value;
				PropertyChanged(this, new PropertyChangedEventArgs("StackIndex"));
				PropertyChanged(this, new PropertyChangedEventArgs("StackIndexString"));
			}
		}

		public string ModifierName
		{
			get => mod?.Name ?? "NULL";
			set { }
		}

		public string StackIndexString
		{
			get { return "Index "+stackIndex; }
			set { }
		}


		public ModifierStackEntry()
		{
			InitializeComponent();
		}

		public ModifierStackEntry(MainWindow parent, Modifier modifier, ModificationChain modificationChain, int i)
		{
			parentWindow = parent;
			mod = modifier;
			this.modificationChain = modificationChain;
			stackIndex = i;
			InitializeComponent();

			inspector.Children.Clear();

			var fields = GetExposedFields(mod.GetType());

			if (fields.Length > 0)
			{
				foreach (var (field, name) in fields)
				{
					if (IsEditSupported(field.FieldType))
					{
						inspector.Children.Add(new InspectorField(name, modifier, field));
					}
					else
					{
						inspector.Children.Add(new TextBlock() { Text = $"Unsupported Type: " + field.FieldType.FullName });
					}
				}
			} else
			{
				inspector.Children.Add(new TextBlock() { Text = $"{mod.Name} does not have any editable properties" });
			}
		}

		private void OnMoveDown(object sender, RoutedEventArgs e)
		{
			if (stackIndex + 1 < modificationChain.chain.Count)
			{
				var belowItem = modificationChain.chain[stackIndex + 1];
				modificationChain.chain[stackIndex + 1] = mod;
				modificationChain.chain[stackIndex] = belowItem;
				stackIndex++;
				parentWindow.UpdateModificationStack();
			}
		}

		private void OnMoveUp(object sender, RoutedEventArgs e)
		{
			if (stackIndex > 0)
			{
				var aboveItem = modificationChain.chain[stackIndex - 1];
				modificationChain.chain[stackIndex - 1] = mod;
				modificationChain.chain[stackIndex] = aboveItem;
				stackIndex--;
				parentWindow.UpdateModificationStack();
			}
		}

		private void OnDelete(object sender, RoutedEventArgs e)
		{
			parentWindow.OnModifierRemoved(this);
		}

		(FieldInfo field, string name)[] GetExposedFields(Type t)
		{
			List<(FieldInfo, string)> exposed = new List<(FieldInfo, string)>();
			var fields = t.GetFields();
			foreach (var f in fields)
			{
				var attr = f.GetCustomAttribute<DrawInInspectorAttribute>();
				if (attr != null)
				{
					exposed.Add((f, attr.label));
				}
			}
			return exposed.ToArray();
		}

		private bool IsEditSupported(Type type)
		{
			return supportedTypes.Contains(type);
		}

		private void OnReset(object sender, RoutedEventArgs e)
		{
			var resetMod = Modifier.CreateModifier(mod.GetType());
			foreach(var c in inspector.Children)
			{
				if(c is InspectorField f)
				{
					f.field.SetValue(mod, f.field.GetValue(resetMod));
					f.UpdateBinding();
				}
			}
			
			/*resetMod.sourceCommand = mod.sourceCommand;
			int indexof = parentWindow.job.exportSettings.modificationChain.IndexOf(mod);
			parentWindow.job.exportSettings.modificationChain[indexof] = resetMod;
			mod = resetMod;
			foreach(var elem in inspector.Children)
			{
				if(elem is InspectorField f)
				{
					f.UpdateBinding();
				}
			}*/
		}

		private void OnInfo(object sender, RoutedEventArgs e)
		{
			ConsoleOutput.WriteLine(mod.VerboseOutput());
		}
	}
}
