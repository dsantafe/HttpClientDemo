namespace HttpClientDemo
{
    public class App(HttpService httpService)
    {
        public async Task RunAsync()
        {
            Console.WriteLine("=== JSON Example (GitHub API) ===");

            var user = await httpService.SendAsync<GitHubUserDTO>(
                clientName: "resilient",
                method: HttpMethod.Get,
                url: "https://api.github.com/users/octocat",
                responseFormat: "json"
            );

            try
            {
                var badCall = await httpService.SendAsync<string>(
                    clientName: "resilient",
                    method: HttpMethod.Get,
                    url: "https://api.github.com/invalid-endpoint",
                    responseFormat: "json"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }

            Console.WriteLine($"User: {user?.Login}, ID: {user?.Id}, URL: {user?.Url}");
            Console.WriteLine();

            Console.WriteLine("=== XML Example (W3Schools Note) ===");
            var note = await httpService.SendAsync<NoteDTO>(
                clientName: "resilient",
                method: HttpMethod.Get,
                url: "https://www.w3schools.com/xml/note.xml",
                responseFormat: "xml"
            );

            Console.WriteLine($"To: {note?.To}, From: {note?.From}, Title: {note?.Heading}, Body: {note?.Body}");

            Console.WriteLine();
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
