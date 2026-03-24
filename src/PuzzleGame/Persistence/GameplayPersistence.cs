using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using PuzzleGame.Screens;
using PuzzleGame.StateManagement;

namespace PuzzleGame.Persistence;

internal sealed class GameplayPersistence
{
    private const string SaveDirectoryName = "ChrisPuzzleGame";
    private const string SaveFileName = "gameplay-state.bin";
    private const int SaveFileVersion = 1;
    private static readonly byte[] SaveFileMagic = [(byte)'C', (byte)'P', (byte)'G', (byte)'S'];

    private readonly string? saveDirectoryPath;
    private readonly string? saveFilePath;
    private readonly string? temporarySaveFilePath;

    public GameplayPersistence()
    {
        var applicationDataPath = GetApplicationDataPath();
        if (string.IsNullOrWhiteSpace(applicationDataPath))
        {
            return;
        }

        saveDirectoryPath = Path.Combine(applicationDataPath, SaveDirectoryName);
        saveFilePath = Path.Combine(saveDirectoryPath, SaveFileName);
        temporarySaveFilePath = $"{saveFilePath}.tmp";
    }

    public GameplayScreen? TryLoad()
    {
        if (string.IsNullOrWhiteSpace(saveFilePath))
        {
            return null;
        }

        RestorePendingSaveIfNeeded();
        if (!File.Exists(saveFilePath))
        {
            return null;
        }

        try
        {
            using var stream = File.Open(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            ValidateSaveFileHeader(stream);

            var gameplayScreen = GameplayScreen.CreateForLoad();
            gameplayScreen.Deserialize(stream);
            return gameplayScreen;
        }
        catch (Exception exception) when (IsPersistenceException(exception))
        {
            Debug.WriteLine($"Failed to load gameplay state from '{saveFilePath}'. {exception}");
            TryDeleteSaveArtifacts();
            return null;
        }
    }

    public void SaveOrDelete(ScreenManager screenManager)
    {
        ArgumentNullException.ThrowIfNull(screenManager);

        if (string.IsNullOrWhiteSpace(saveDirectoryPath) || string.IsNullOrWhiteSpace(saveFilePath) || string.IsNullOrWhiteSpace(temporarySaveFilePath))
        {
            return;
        }

        var gameplayScreen = FindGameplayScreen(screenManager);
        if (gameplayScreen is null)
        {
            TryDeleteSaveArtifacts();
            return;
        }

        try
        {
            Directory.CreateDirectory(saveDirectoryPath);

            using (var stream = File.Open(temporarySaveFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                WriteSaveFileHeader(stream);
                gameplayScreen.Serialize(stream);
            }

            File.Move(temporarySaveFilePath, saveFilePath, overwrite: true);
        }
        catch (Exception exception) when (IsPersistenceException(exception))
        {
            Debug.WriteLine($"Failed to save gameplay state to '{saveFilePath}'. {exception}");
            TryDeleteFile(temporarySaveFilePath);
        }
    }

    private void RestorePendingSaveIfNeeded()
    {
        if (string.IsNullOrWhiteSpace(saveFilePath) || string.IsNullOrWhiteSpace(temporarySaveFilePath))
        {
            return;
        }

        try
        {
            if (File.Exists(saveFilePath))
            {
                if (File.Exists(temporarySaveFilePath))
                {
                    File.Delete(temporarySaveFilePath);
                }

                return;
            }

            if (File.Exists(temporarySaveFilePath))
            {
                File.Move(temporarySaveFilePath, saveFilePath);
            }
        }
        catch (Exception exception) when (IsPersistenceException(exception))
        {
            Debug.WriteLine($"Failed to reconcile gameplay save files in '{saveDirectoryPath}'. {exception}");
        }
    }

    private GameplayScreen? FindGameplayScreen(ScreenManager screenManager)
    {
        var screens = screenManager.GetScreens();
        for (var index = screens.Length - 1; index >= 0; index--)
        {
            if (screens[index] is InGameOptionsScreen optionsScreen
                && optionsScreen.TryGetGameplayScreenForPersistence(out var pausedGameplayScreen))
            {
                return pausedGameplayScreen;
            }

            if (screens[index] is GameplayScreen gameplayScreen && gameplayScreen.CanResume)
            {
                return gameplayScreen;
            }
        }

        return null;
    }

    private void WriteSaveFileHeader(Stream stream)
    {
        stream.Write(SaveFileMagic);
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(SaveFileVersion);
    }

    private void ValidateSaveFileHeader(Stream stream)
    {
        Span<byte> actualMagic = stackalloc byte[4];
        stream.ReadExactly(actualMagic);
        for (var index = 0; index < SaveFileMagic.Length; index++)
        {
            if (actualMagic[index] != SaveFileMagic[index])
            {
                throw new InvalidDataException("Save file header is not recognized.");
            }
        }

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var saveFileVersion = reader.ReadInt32();
        if (saveFileVersion != SaveFileVersion)
        {
            throw new InvalidDataException($"Unsupported save file version '{saveFileVersion}'.");
        }
    }

    private void TryDeleteSaveArtifacts()
    {
        TryDeleteFile(saveFilePath);
        TryDeleteFile(temporarySaveFilePath);
    }

    private void TryDeleteFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        try
        {
            File.Delete(filePath);
        }
        catch (Exception exception) when (IsPersistenceException(exception))
        {
            Debug.WriteLine($"Failed to delete gameplay save file '{filePath}'. {exception}");
        }
    }

    private static string? GetApplicationDataPath()
    {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(localApplicationData))
        {
            return localApplicationData;
        }

        var roamingApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return string.IsNullOrWhiteSpace(roamingApplicationData) ? null : roamingApplicationData;
    }

    private static bool IsPersistenceException(Exception exception)
    {
        return exception is IOException
            or UnauthorizedAccessException
            or InvalidDataException
            or NotSupportedException
            or ArgumentException;
    }
}
