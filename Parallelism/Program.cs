using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static int totalImages;
    private static int downloadedImages;
    private static int parallelism;
    private static List<string> downloadedFiles;

    static void Main()
    {
        // Read input from the JSON file
        string json = File.ReadAllText("Input.json");
        InputData inputData = JsonSerializer.Deserialize<InputData>(json);

        totalImages = inputData.Count;
        parallelism = inputData.Parallelism;

        downloadedImages = 0;
        downloadedFiles = new List<string>();

        DownloadImagesAsync().Wait();

        Console.WriteLine("All images downloaded successfully!");
    }

    static async Task DownloadImagesAsync()
    {
        Console.WriteLine("Downloading images...");

        List<Task> downloadTasks = new List<Task>();

        for (int i = 0; i < totalImages; i++)
        {
            // Generate a unique filename for each downloaded image
            string filename = $"{i + 1}.png";

            // Start a new download task
            downloadTasks.Add(DownloadImageAsync(filename));

            // Limit the number of parallel downloads
            if (downloadTasks.Count >= parallelism)
            {
                await Task.WhenAny(downloadTasks);
                downloadTasks.RemoveAll(t => t.IsCompleted);
            }
        }

        await Task.WhenAll(downloadTasks);
    }

    static async Task DownloadImageAsync(string filename)
    {
        using (var client = new WebClient())
        {
            // Generate a random URL for image download
            string imageUrl = GenerateRandomImageUrl();

            // Download the image
            await client.DownloadFileTaskAsync(imageUrl, filename);

            // Update the progress
            lock (downloadedFiles)
            {
                downloadedImages++;
                downloadedFiles.Add(filename);
                Console.WriteLine($"Downloaded: {downloadedImages}/{totalImages}");
            }
        }
    }

    static string GenerateRandomImageUrl()
    {
        // Replace this with your image source URL
        string imageSourceUrl = "https://picsum.photos";

        // Generate a random image URL
        Random random = new Random();
        string imageUrl = $"{imageSourceUrl}/{random.Next(1, 1000)}/{random.Next(1, 1000)}";

        return imageUrl;
    }
}

class InputData
{
    public int Count { get; set; }
    public int Parallelism { get; set; }
    public string SavePath { get; set; }
}