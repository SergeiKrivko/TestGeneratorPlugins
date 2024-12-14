using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Git.Services;

namespace Git.UI;

public partial class BranchButton : UserControl
{
    public BranchButton()
    {
        InitializeComponent();
        BranchesTree.ItemsSource = BranchService.Instance.Groups;
        BranchService.Instance.CurrentChanged += branch => BranchNameBlock.Text = branch?.Name;
    }
}