using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Harder_WPF
{
    public partial class ToolDetailsWindow : Window
    {
        private Dictionary<string, bool> toolStates;
        private List<CheckBox> checkBoxes;
        
        public ToolDetailsWindow(string category, Dictionary<string, bool> states)
        {
            InitializeComponent();
            
            toolStates = new Dictionary<string, bool>(states);
            checkBoxes = new List<CheckBox>();
            
            HeaderText.Text = $"{category} Security Tools";
            
            PopulateToolsList();
        }
        
        private void PopulateToolsList()
        {
            foreach (var tool in toolStates)
            {
                var checkBox = new CheckBox
                {
                    Content = tool.Key,
                    IsChecked = tool.Value,
                    Style = (Style)FindResource("ToolToggleStyle"),
                    Margin = new Thickness(0, 0, 0, 8)
                };
                
                checkBoxes.Add(checkBox);
                ToolsPanel.Children.Add(checkBox);
            }
        }
        
        public Dictionary<string, bool> GetUpdatedStates()
        {
            var updatedStates = new Dictionary<string, bool>();
            
            for (int i = 0; i < checkBoxes.Count; i++)
            {
                var toolName = toolStates.Keys.ElementAt(i);
                updatedStates[toolName] = checkBoxes[i].IsChecked ?? false;
            }
            
            return updatedStates;
        }
        
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in checkBoxes)
            {
                checkBox.IsChecked = true;
            }
        }
        
        private void DeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in checkBoxes)
            {
                checkBox.IsChecked = false;
            }
        }
        
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
