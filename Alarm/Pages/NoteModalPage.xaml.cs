using System;
using Microsoft.Maui.Controls;

namespace Alarm.Pages
{
	public partial class NoteModalPage : ContentPage
	{
		public NoteModalPage(string noteContent)
		{
			InitializeComponent();
			NoteContentLabel.Text = noteContent;
		}

		private async void OnCloseClicked(object sender, EventArgs e)
		{
			await Navigation.PopModalAsync();
		}
	}
}
