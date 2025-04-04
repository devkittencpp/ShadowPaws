using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace ShadowPaws
{
    public partial class MacroManagementWindow : Window
    {
        // The working list of macros.
        public List<Macro> Macros { get; set; }

        // Event to notify MainWindow that macros have been updated.
        public event Action<List<Macro>> OnMacrosUpdated;

        // Index of the macro currently being edited (-1 if none)
        private int editingMacroIndex = -1;

        public MacroManagementWindow(List<Macro> macros)
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Macros = new List<Macro>(macros);
            RenderMacroList();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void RenderMacroList()
        {
            var listBox = this.FindControl<ListBox>("MacrosListBox");
            listBox.ItemsSource = null;
            listBox.ItemsSource = Macros;
        }

        private void AddMacroButton_Click(object sender, RoutedEventArgs e)
        {
            var name = this.FindControl<TextBox>("MacroNameTextBox").Text;
            var command = this.FindControl<TextBox>("MacroCommandTextBox").Text;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(command))
            {
                return;
            }

            if (editingMacroIndex >= 0 && editingMacroIndex < Macros.Count)
            {
                Macros[editingMacroIndex].Name = name;
                Macros[editingMacroIndex].Command = command;
                editingMacroIndex = -1;
                this.FindControl<Button>("AddMacroButton").Content = "Add Macro";
            }
            else
            {
                Macros.Add(new Macro { Name = name, Command = command });
            }

            RenderMacroList();
            this.FindControl<TextBox>("MacroNameTextBox").Text = "";
            this.FindControl<TextBox>("MacroCommandTextBox").Text = "";
        }

        private void RemoveMacroButton_Click(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("MacrosListBox");
            if (listBox.SelectedItem is Macro selectedMacro)
            {
                Macros.Remove(selectedMacro);
                if (editingMacroIndex >= 0 && editingMacroIndex < Macros.Count && Macros[editingMacroIndex] == selectedMacro)
                {
                    editingMacroIndex = -1;
                    this.FindControl<Button>("AddMacroButton").Content = "Add Macro";
                }
                RenderMacroList();
            }
        }

        private void EditMacroButton_Click(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("MacrosListBox");
            if (listBox.SelectedItem is Macro selectedMacro)
            {
                editingMacroIndex = Macros.IndexOf(selectedMacro);
                if (editingMacroIndex >= 0)
                {
                    this.FindControl<TextBox>("MacroNameTextBox").Text = selectedMacro.Name;
                    this.FindControl<TextBox>("MacroCommandTextBox").Text = selectedMacro.Command;
                    this.FindControl<Button>("AddMacroButton").Content = "Update Macro";
                }
            }
        }

        private void MacrosListBox_DoubleTapped(object sender, RoutedEventArgs e)
        {
            var listBox = this.FindControl<ListBox>("MacrosListBox");
            if (listBox.SelectedItem is Macro selectedMacro)
            {
                editingMacroIndex = Macros.IndexOf(selectedMacro);
                if (editingMacroIndex >= 0)
                {
                    this.FindControl<TextBox>("MacroNameTextBox").Text = selectedMacro.Name;
                    this.FindControl<TextBox>("MacroCommandTextBox").Text = selectedMacro.Command;
                    this.FindControl<Button>("AddMacroButton").Content = "Update Macro";
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            OnMacrosUpdated?.Invoke(Macros);
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
