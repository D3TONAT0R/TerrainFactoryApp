﻿using TerrainFactory;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TerrainFactoryApp
{
	public class ConsoleWindowHandler : IConsoleHandler
	{
		private RichTextBox consoleRichTextBox;
		private TextBox consoleTextBox;

		public ConsoleWindowHandler(object textBox)
		{
			if (textBox is RichTextBox box)
			{
				consoleRichTextBox = box;
				consoleRichTextBox.Document.Blocks.Clear();
				consoleRichTextBox.AppendText("\n");
			}
			else
			{
				consoleTextBox = (TextBox)textBox;
				consoleTextBox.Clear();
			}
		}

		public void DisplayProgressBar(string text, float progress)
		{

		}

		public string GetInput(string prompt, List<string> queue)
		{
			throw new NotImplementedException();
		}

		public void WriteLine(string line)
		{
			line = "> " + line + "\n";

			if (consoleRichTextBox != null)
			{
				consoleRichTextBox.AppendText(line);
			}
			else
			{
				consoleTextBox.Text += line;
			}
		}
	}
}
