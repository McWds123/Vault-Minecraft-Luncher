using System;
using System.IO;

namespace Demo
{
    /// <summary>
    /// Utility for simple file reading. Method name and behavior are preserved to avoid breaking existing callers.
    /// </summary>
    public class LoadFile
    {
        /// <summary>
        /// Read all text from the provided file path. Exceptions are rethrown after logging to Console as before.
        /// </summary>
        /// <param name="filePath">Path to the file to read</param>
        /// <returns>File contents</returns>
        public string LoadFiles(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty or whitespace.", nameof(filePath));

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
}