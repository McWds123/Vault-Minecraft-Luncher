using System;
using System.IO;

public class LoadFile
{
    public string LoadFiles(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty or whitespace.", nameof(filePath));
        }

        try
        {
            return File.ReadAllText(filePath);
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Error: File not found! Path: {filePath}, Details: {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error: File read failed! Path: {filePath}, Details: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Unknown exception occurred while reading file! Path: {filePath}, Details: {ex.Message}");
            throw;
        }
    }
}