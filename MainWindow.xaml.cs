using TerrainFactory;
using TerrainFactory.Formats;
using TerrainFactory.Modification;
using TerrainFactory.Util;
using TerrainFactoryApp.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using TerrainFactory.Commands;

namespace TerrainFactoryApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		ConsoleWindowHandler console;

		public Project project = new Project();

		Dictionary<Modifier, ModifierStackEntry> stackEntries = new Dictionary<Modifier, ModifierStackEntry>();

		public MainWindow()
		{

			InitializeComponent();

			console = new ConsoleWindowHandler(consoleWindow);
			ConsoleOutput.consoleHandler = console;

			string modulesDLLDirectory = AppContext.BaseDirectory;
			TerrainFactoryManager.Initialize();

			InputList.ItemsSource = project.InputData.Files;

			RemoveFileButton.IsEnabled = false;
			PreviewFileButton.IsEnabled = false;

			foreach (var mod in Modifier.availableModifierTypes)
			{
				var cbi = new ComboBoxItem()
				{
					Content = mod.Name
				};
				modificatorDropDown.Items.Add(cbi);
			}

			foreach (var ff in FileFormatManager.GetSupportedFormats(FileFormat.FileSupportFlags.Import))
			{
				var toggle = new CheckBox()
				{
					Content = ff.Identifier,
					Tag = ff,
				};
				toggle.Checked += OnExportFormatChecked;
				toggle.Unchecked += OnExportFormatUnchecked;
				exportFormatToggleList.Children.Add(toggle);
			}

			UpdateModificationStack();
		}

		internal void UpdateModificationStack()
		{
			ModificationStack.Children.Clear();
			for (int i = 0; i < project.modificationChain.chain.Count; i++)
			{
				var m = project.modificationChain.chain[i];
				stackEntries[m].StackIndex = i;
				ModificationStack.Children.Add(stackEntries[m]);
			}
		}

		internal void OnModifierAdded(Modifier mod)
		{
			var entry = new ModifierStackEntry(this, mod, project.modificationChain, project.modificationChain.chain.Count - 1);
			stackEntries.Add(mod, entry);
			ModificationStack.Children.Add(entry);
		}

		internal void OnModifierRemoved(ModifierStackEntry entry)
		{
			project.modificationChain.chain.Remove(entry.mod);
			stackEntries.Remove(entry.mod);
			ModificationStack.Children.Remove(entry);
		}

		private void OnAddFile(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Multiselect = false,
				Title = "Add file to import"
			};
			if (dialog.ShowDialog() == true)
			{
				string path = dialog.FileName;
				project.InputData.Add(path);
				InputList.SelectedIndex = project.InputData.FileCount - 1;
			}
		}

		private void OnRemoveSelectedFile(object sender, RoutedEventArgs e)
		{
			if (InputList.SelectedIndex >= 0)
			{
				project.InputData.Files.RemoveAt(InputList.SelectedIndex);
			}
		}

		private void OnInputSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool b = InputList.SelectedIndex >= 0;
			RemoveFileButton.IsEnabled = b;
			PreviewFileButton.IsEnabled = b;
		}

		private void OnModificatorDropdownClose(object sender, EventArgs e)
		{
			int i = modificatorDropDown.SelectedIndex;
			if (i > 0)
			{
				i--;
				var m = Modifier.CreateModifier(Modifier.availableModifierTypes[i]);
				project.modificationChain.AddModifier(m, false);
				OnModifierAdded(m);
				//AddModifierComposite(m);
			}
			modificatorDropDown.SelectedIndex = 0;
		}

		private void OnExportFormatChecked(object sender, RoutedEventArgs args)
		{
			var ff = (FileFormat)((CheckBox)sender).Tag;
			project.outputFormats.AddFormat(ff);
		}

		private void OnExportFormatUnchecked(object sender, RoutedEventArgs args)
		{
			var ff = (FileFormat)((CheckBox)sender).Tag;
			for (int i = 0; i < project.outputFormats.Count; i++)
			{
				if (project.outputFormats.list[i].GetType() == ff.GetType())
				{
					project.outputFormats.list.RemoveAt(i);
					return;
				}
			}
		}

		private void OnExportClick(object sender, RoutedEventArgs e)
		{

			if (Directory.Exists(Path.GetDirectoryName(outputPathBox.Text)))
			{
				project.OutputPath = outputPathBox.Text;
				project.ProcessAll();
			}
			else
			{
				MessageBox.Show($"Directory does not exist!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
