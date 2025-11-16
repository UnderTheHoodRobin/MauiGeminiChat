using System.Text;
using System.Text.Json;

namespace MauiGeminiChat
{
	public partial class MainPage : ContentPage
	{
		private const string ApiKey = "<Your-API-Key>";
		private const string Model = "gemini-2.0-flash";

		public MainPage()
		{
			InitializeComponent();
		}

		private async void OnSendClicked(object sender, EventArgs e)
		{
			var prompt = PromptEntry.Text?.Trim();
			if (string.IsNullOrWhiteSpace(prompt))
				return;

			ResponseLabel.Text = "Loading...";

			try
			{
				string result = await CallGeminiAsync(prompt);
				ResponseLabel.Text = result;
				PromptEntry.Text = string.Empty;
			}
			catch (Exception ex)
			{
				ResponseLabel.Text = $"Error: {ex.Message}";
			}
		}

		private async Task<string> CallGeminiAsync(string prompt)
		{
			string url =
				$"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={ApiKey}";

			var request = new
			{
				contents = new[]
				{
					new {
						parts = new[]
						{
							new { text = prompt }
						}
					}
				}
			};

			var json = JsonSerializer.Serialize(request);

			using var http = new HttpClient();
			var response = await http.PostAsync(
				url,
				new StringContent(json, Encoding.UTF8, "application/json")
			);

			string jsonResponse = await response.Content.ReadAsStringAsync();

			try
			{
				using var doc = JsonDocument.Parse(jsonResponse);

				var text =
					doc.RootElement
					   .GetProperty("candidates")[0]
					   .GetProperty("content")
					   .GetProperty("parts")[0]
					   .GetProperty("text")
					   .GetString();

				return text ?? "(empty response)";
			}
			catch
			{
				return jsonResponse;
			}
		}

	}
}
