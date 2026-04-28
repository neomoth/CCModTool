using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CCModTool.Abstractions.Config;
using CCModTool.Abstractions.IoC;
using CCModTool.Logging;
using CCModTool.UI.App.Config;

namespace CCModTool.UI.App.Views;

public sealed partial class ConfigView : UserControl
{
	[Dependency] private readonly ConfigManager _cfg = null!;
	
	public ConfigView()
	{
		IoCManager.InjectDependencies(this);
		InitializeComponent();
		
		((Panel)Content!).Children.Add(_focusSink);
		
		AddConfigs();
		
		AddHandler(PointerPressedEvent, OnGlobalPointerPressed, handledEventsToo: true);
	}
	
	private readonly Control _focusSink = new Border
	{
		IsHitTestVisible = false,
		Focusable = false
	};

	private void AddConfigs()
	{
		foreach (var item in _cfg.ConfigTypes()
			         .Select(t => new { Type = t, Config = _cfg.Get(t) as IConfig })
			         .Where(x => x.Config != null)
			         .OrderBy(x => x.Config!.Priority))
		{
			var config = item.Config;
			var type = item.Type;

			var panel = new Border
			{
				CornerRadius = new CornerRadius(6),
				Background = new SolidColorBrush(new Color(255, 34, 34, 37)),
				Padding = new Thickness(10),
				Margin = new Thickness(0, 0, 0, 10),
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto), // name
					new RowDefinition(GridLength.Auto), // separator
					new RowDefinition(GridLength.Auto) // props
				},
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			// =========================
			// Config name header
			// =========================
			var cfgName = new TextBlock
			{
				Text = config?.GetType().Name.Replace("Config", ""),
				FontWeight = FontWeight.Bold,
				FontSize = 16,
				HorizontalAlignment = HorizontalAlignment.Left
			};

			Grid.SetRow(cfgName, 0);

			// separator line
			var separator = new Border
			{
				Height = 1,
				Margin = new Thickness(0, 6, 0, 6),
				Background = Brushes.Gray,
				HorizontalAlignment = HorizontalAlignment.Stretch
			};

			Grid.SetRow(separator, 1);

			// =========================
			// Properties container
			// =========================
			var propsPanel = new StackPanel
			{
				Orientation = Orientation.Vertical,
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Spacing = 6
			};

			Grid.SetRow(propsPanel, 2);

			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (!prop.CanRead || !prop.CanWrite)
					continue;

				var configProp = prop.GetCustomAttributes()
					.OfType<ConfigPropAttribute>()
					.FirstOrDefault();

				if (configProp is null || prop.Name == "SchemaVersion")
					continue;

				var value = prop.GetValue(config);

				var propSize = configProp.PropSize switch
				{
					PropSize.Small => new GridLength(120),
					PropSize.Medium => new GridLength(320),
					PropSize.Large => new GridLength(550),
					_ => GridLength.Star
				};

				var row = new Grid
				{
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Auto),
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Auto),
						new ColumnDefinition(propSize)
					},
					HorizontalAlignment = HorizontalAlignment.Stretch,
					Background = Brushes.Transparent,
					IsHitTestVisible = true
				};

				if (configProp.Tooltip is not null)
				{
					ToolTip.SetTip(row, configProp.Tooltip);
					ToolTip.SetShowDelay(row, 50);
				}

				// label
				var propName = new TextBlock
				{
					Text = ToReadableName(prop.Name),
					VerticalAlignment = VerticalAlignment.Center,
					Margin = new Thickness(0, 0, 10, 0)
				};

				Grid.SetColumn(propName, 0);

				var spacer = new Border
				{
					Width = 50 // padding between label and controls
				};
				Grid.SetColumn(spacer, 1);

				// optional file button
				Button? fileBtn = null;

				if (prop.GetCustomAttributes().Any(a => a is PathProp))
				{
					fileBtn = new Button
					{
						Content = "Select File",
						Margin = new Thickness(0, 0, 6, 0),
						VerticalAlignment = VerticalAlignment.Center
					};

					fileBtn.Classes.Add("PanelButton");
					Grid.SetColumn(fileBtn, 2);
				}
				
				Control inputControl;
				
				if (prop.PropertyType == typeof(bool))
				{
					row.ColumnDefinitions.Last().Width = new GridLength(120); // Force small
					var cb = new CheckBox
					{
						IsChecked = value is true,
						VerticalAlignment = VerticalAlignment.Center,
						HorizontalAlignment = HorizontalAlignment.Right
					};

					var text = new TextBlock
					{
						Text = value is true ? "True" : "False",
						VerticalAlignment = VerticalAlignment.Center,
						Margin = new Thickness(0, 0, 6, 0)
					};

					cb.IsCheckedChanged += (_, _) =>
					{
						var state = cb.IsChecked == true;

						text.Text = state ? "True" : "False";

						prop.SetValue(config, state);
						_cfg.Save(config!);
					};

					var boolPanel = new StackPanel
					{
						Orientation = Orientation.Horizontal,
						HorizontalAlignment = HorizontalAlignment.Right,
						VerticalAlignment = VerticalAlignment.Center
					};

					boolPanel.Children.Add(text);
					boolPanel.Children.Add(cb);

					inputControl = boolPanel;
				}
				else if (prop.PropertyType.IsEnum)
				{
					var combo = new ComboBox
					{
						ItemsSource = Enum.GetValues(prop.PropertyType)
							.Cast<object>()
							.Select(e => new { Value = e, Name = e.ToString() }),
						SelectedItem = value,
						VerticalAlignment = VerticalAlignment.Center
					};

					combo.SelectionChanged += (_, _) =>
					{
						if (combo.SelectedItem is not null)
						{
							prop.SetValue(config, combo.SelectedItem);
							_cfg.Save(config!);
						}
					};

					inputControl = combo;
				}
				else
				{
					var input = new TextBox
					{
						Text = value?.ToString() ?? configProp.Default?.ToString(),
						PlaceholderText = configProp.Default?.ToString(),
						HorizontalAlignment = HorizontalAlignment.Stretch,
						TextAlignment = TextAlignment.Right
					};

					input.GotFocus += (_, _) =>
					{
						_focusSink.Focusable = true;
					};
					
					input.LostFocus += (_, _) => Commit(input, prop, config!);

					input.AddHandler(KeyDownEvent, (sender, e) =>
					{
						if (e.Key != Key.Enter) return;
						Commit(input, prop, config!);
						_focusSink.Focus();
						e.Handled = true;
					}, handledEventsToo: true);

					inputControl = input;
				}
				
				// var input = new TextBox
				// {
				// 	Text = value?.ToString() ?? configProp.Default?.ToString(),
				// 	PlaceholderText = configProp.Default?.ToString(),
				// 	HorizontalAlignment = HorizontalAlignment.Stretch,
				// 	TextAlignment = TextAlignment.Right
				// };
				
				fileBtn?.Click += async (_, _) =>
				{
					var topLevel = TopLevel.GetTopLevel(this);
					if (topLevel?.StorageProvider is null)
						return;

					string? result = null;

					var attr = prop.GetCustomAttribute<PathProp>();

					if (attr?.Type == PathType.Directory)
					{
						var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
						{
							Title = "Select Directory",
							AllowMultiple = false
						});

						result = folders.FirstOrDefault()?.Path.LocalPath;
					}
					else
					{
						var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
						{
							Title = "Select File",
							AllowMultiple = false
						});

						result = files.FirstOrDefault()?.Path.LocalPath;
					}

					if (string.IsNullOrWhiteSpace(result))
						return;

					// update UI
					if (inputControl is not TextBox input) return;
					input.Text = result;

					// update config
					prop.SetValue(config, result);

					_cfg.Save(config!);
				};

				Grid.SetColumn(inputControl, 3);

				row.Children.Add(propName);
				
				row.Children.Add(spacer);

				if (fileBtn != null)
					row.Children.Add(fileBtn);

				row.Children.Add(inputControl);

				propsPanel.Children.Add(row);
			}

			grid.Children.Add(cfgName);
			grid.Children.Add(separator);
			grid.Children.Add(propsPanel);

			panel.Child = grid;
			ConfigList.Children.Add(panel);
		}
	}
	
	void Commit(TextBox input, PropertyInfo prop, IConfig config)
	{
		try
		{
			var text = input.Text;

			object? converted = text;

			if (prop.PropertyType == typeof(string))
			{
				converted = text;
			}
			else if (prop.PropertyType == typeof(int) && int.TryParse(text, out var i))
			{
				converted = i;
			}
			else if (prop.PropertyType == typeof(bool) && bool.TryParse(text, out var b))
			{
				converted = b;
			}
			else if (prop.PropertyType == typeof(float) && float.TryParse(text, out var f))
			{
				converted = f;
			}
			else if (prop.PropertyType == typeof(Version) && Version.TryParse(text, out var v))
			{
				converted = v;
			}
			else if (prop.PropertyType.IsEnum)
			{
				if (Enum.TryParse(prop.PropertyType, text, true, out var enumVal))
					converted = enumVal;
				else
					throw new Exception("Invalid enum value");
			}

			prop.SetValue(config, converted);
			_cfg.Save(config);
		}
		catch
		{
			input.Text = prop.GetValue(config)?.ToString();
		}
	}

	private void OnGlobalPointerPressed(object? sender, PointerPressedEventArgs args)
	{
		var source = args.Source as Visual;

		// Ignore clicks inside text inputs
		if (source?.FindAncestorOfType<TextBox>() != null)
			return;

		// Move focus somewhere harmless
		_focusSink.Focus();
		_focusSink.Focusable = false;
	}
	
	private static string ToReadableName(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
			return input;

		var sb = new System.Text.StringBuilder();
		sb.Append(input[0]);

		for (int i = 1; i < input.Length; i++)
		{
			char c = input[i];
			char prev = input[i - 1];

			bool prevIsUpper = char.IsUpper(prev);
			bool currIsUpper = char.IsUpper(c);

			bool prevIsDigit = char.IsDigit(prev);
			bool currIsDigit = char.IsDigit(c);

			bool nextIsLower =
				i + 1 < input.Length &&
				char.IsLower(input[i + 1]);

			// boundaries:
			bool camelCaseBoundary =
				char.IsLower(prev) && currIsUpper;

			bool acronymBoundary =
				prevIsUpper && currIsUpper && nextIsLower;

			bool numberBoundary =
				(prevIsDigit && !currIsDigit) ||
				(!prevIsDigit && currIsDigit);

			if (camelCaseBoundary || acronymBoundary || numberBoundary)
			{
				sb.Append(' ');
			}

			sb.Append(c);
		}

		return sb.ToString();
	}
}