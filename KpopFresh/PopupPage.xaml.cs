using CommunityToolkit.Maui.Views;
namespace KpopFresh;

public partial class PopupPage
{
	public PopupPage()
	{
		InitializeComponent();

        
    }

	private void CancelClicked(object sender, EventArgs e)
	{
		Close(); 
	}
}