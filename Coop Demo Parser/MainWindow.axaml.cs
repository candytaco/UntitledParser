using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using DemoParser.Utils;
using DemoParser.Parser;
using DemoParser.Parser.Components.Abstract;
using DemoParser.Parser.Components.Messages;
using DemoParser.Parser.Components.Messages.UserMessages;
using Avalonia;

namespace Coop_Demo_Parser
{
	public class DemoListItem
	{
		public string FileName { get; private set; }
		public string FullFilePath { get; private set; }
		public SourceDemo demo { get; private set; }

		public DemoListItem(IStorageItem source)
		{
			FileName = source.Name;
			FullFilePath = source.Path.LocalPath;

			demo = new SourceDemo(FullFilePath);
			demo.Parse();
		}

		public override string ToString() => FileName;
	}
	public partial class MainWindow : Window
	{
		private TextBlock fileNameTextBlock;
		private TextBlock basicDemoInfoTextBlock;
		private ListBox filesListBox;
		private string demoFileName;
		private SourceDemo? demo = null;

		public MainWindow()
		{
			InitializeComponent();

			AddHandler(DragDrop.DropEvent, DropFile);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			fileNameTextBlock = this.FindControl<TextBlock>("FileDropTextBlock");
			basicDemoInfoTextBlock = this.FindControl<TextBlock>("BasicDemoInfoTextBlock");
			filesListBox = this.FindControl<ListBox>("FilesListBox");
		}

		private void DropFile(object? sender, DragEventArgs e)
		{
			// Todo: handle multiple files - right now it's only 1 / the first item
			IEnumerable<IStorageItem> filesDropped = e.Data.GetFiles();
			foreach (IStorageItem item in filesDropped)
			{
				AddDemoInfo(item);
			}
		}

		private void AddDemoInfo(IStorageItem file)
		{
			filesListBox.Items.Add(new DemoListItem(file));
			fileNameTextBlock.IsVisible = false;
			filesListBox.SelectedIndex = filesListBox.Items.Count - 1;
		}

		private void SetDemoInfo(SourceDemo demo)
		{
			basicDemoInfoTextBlock.Text = demo.Header.ToString();
		}

		public async void OpenDemoButton_OnClick(object? sender, RoutedEventArgs e)
		{
			IReadOnlyList<IStorageFile> files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
				new FilePickerOpenOptions
				{
					Title = "Open Demo File",
					FileTypeFilter = new[] { new FilePickerFileType("Demo files") { Patterns = new[] { "*.dem" } } },
				});
			if (files.Count > 0)
			{
				foreach (IStorageFile file in files)
				{
					AddDemoInfo(file);
				}
			}
		}

		private void ExportPortalInfo(DemoListItem item)
		{
			SourceDemo demo = item.demo;
			List<(PortalFxSurface, int)> portalMessages = new List<(PortalFxSurface, int)>();
			foreach ((SvcUserMessage message, int tick) in demo.FilterForMessage<SvcUserMessage>())
			{
				if (message.MessageType == UserMessageType.PortalFX_Surface)
					portalMessages.Add(((PortalFxSurface)message.UserMessage, tick));
			}
			portalMessages.Sort((item1, item2) => item1.Item2.CompareTo(item2.Item2));
			
			string outFileName = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(item.FullFilePath)), Path.GetFileNameWithoutExtension(item.FullFilePath) + " portals.csv");
			using (StreamWriter writer = new StreamWriter(outFileName))
			{
				writer.WriteLine("tick,Portal Entity,Owner Entity,Team,Portal Num,Effect,Origin X,Origin Y, Origin Z,Orientation X,Orientation Y,Orientation Z");
				foreach ((PortalFxSurface message, int tick) in portalMessages)
				{
					writer.Write(tick); writer.Write(",");
					writer.Write(message.PortalEnt); writer.Write(",");
					writer.Write(message.OwnerEnt); writer.Write(",");
					writer.Write(message.Team); writer.Write(",");
					writer.Write(message.PortalNum); writer.Write(",");
					switch (message.Effect)
					{
						case PortalFizzleType.PortalFizzleSuccess:
							writer.Write("+1");
							break;
						case PortalFizzleType.PortalFizzleClose:
							writer.Write("-1");
							break;
						case PortalFizzleType.PortalFizzleKilled:
							writer.Write("-2");
							break;
						case PortalFizzleType.PortalFizzleCleanser:
							writer.Write("-3");
							break;
						default:
							writer.Write("0");
							break;
					}
					writer.Write(",");
					writer.Write("{0},{1},{2},", message.Origin.X, message.Origin.Y, message.Origin.Z);
					writer.Write("{0},{1},{2},", message.Angles.X, message.Angles.Y, message.Angles.Z);
					writer.WriteLine();
				}
			}
		}

		private void ExportDemoInfoButton_OnClick(object? sender, RoutedEventArgs e)
		{
			foreach (object item in filesListBox.Items)
			{
				ExportPortalInfo((DemoListItem)item);
			}
		}

		private void FilesListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			SourceDemo thisDemo = ((DemoListItem)filesListBox.SelectedItems[0]).demo;
			SetDemoInfo(thisDemo);
		}
	}
}