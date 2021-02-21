using System.Linq;
using FluentAssertions;
using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using System.Threading;

namespace PathLengthChecker.Tests
{
	public class FilesFixtureForClassSetupAndTeardown : IDisposable
	{
		public List<PathInfo> Directories { get; }
		public List<PathInfo> Files { get; }
		public List<PathInfo> AllPaths { get; }
		public string RootPath { get; }
		public string EmptyDirectoryPath { get; }

		public FilesFixtureForClassSetupAndTeardown()
		{
			RootPath = Path.Combine(Environment.CurrentDirectory, "UnitTestTemp");
			EmptyDirectoryPath = Path.Combine(RootPath, "EmptyDir");

			// Specify the directories and files that the unit tests expect to be there.
			Directories = new List<PathInfo>
			{
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir1")},
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir2")},
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestDir3")},
				new PathInfo(){Path = EmptyDirectoryPath}
			};

			Files = new List<PathInfo>
			{
				new PathInfo(){Path = Path.Combine(RootPath, "TestFile0.test")},
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir1\\TestFile1.test")},
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestFile2.test")},
				new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestDir3\\TestFile3.test")}
			};

			// Create a list that contains all paths.
			AllPaths = new List<PathInfo>(Directories);
			AllPaths.AddRange(Files);

			// Create the paths on the local hard drive.
			CreateDirectoriesAndFiles();
		}

		public void Dispose()
		{
			DeleteDirectoriesAndFiles();
		}

		private void CreateDirectoriesAndFiles()
		{
			// Create the directories and files that the unit tests expect to be there.
			foreach (var directory in Directories)
			{
				var directoryPath = Path.Combine(RootPath, directory.Path);
				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);
			}

			foreach (var file in Files)
			{
				var filePath = Path.Combine(RootPath, file.Path);
				if (!File.Exists(filePath))
					File.Create(filePath);
			}
		}

		private void DeleteDirectoriesAndFiles()
		{
			// Delete the directories and files created for the unit tests.
			foreach (var file in Files)
			{
				var filePath = Path.Combine(RootPath, file.Path);
				if (File.Exists(filePath))
					File.Delete(filePath);
			}

			foreach (var diretory in Directories)
			{
				var directoryPath = Path.Combine(RootPath, diretory.Path);
				if (Directory.Exists(directoryPath))
					Directory.Delete(directoryPath, true);
			}
		}
	}

	/// <summary>
	/// Tests to test the PathLengthChecker class.
	/// </summary>
	public class PathLengthCheckerTests : IClassFixture<FilesFixtureForClassSetupAndTeardown>
	{
		private readonly FilesFixtureForClassSetupAndTeardown _filesFixture;
		
		public PathLengthCheckerTests(FilesFixtureForClassSetupAndTeardown filesFixture)
		{
			_filesFixture = filesFixture;
		}

		[Fact]
		public void GetAllPaths()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the paths.
			paths.Should().Contain(_filesFixture.Directories).And.Contain(_filesFixture.Files);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _filesFixture.Directories.Contains(p) || _filesFixture.Files.Contains(p));
		}

		[Fact]
		public void GetAllDirectories()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of directory paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the directory paths.
			paths.Should().Contain(_filesFixture.Directories);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _filesFixture.Directories.Contains(p));
		}

		[Fact]
		public void GetAllFiles()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of file paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the file paths.
			paths.Should().Contain(_filesFixture.Files);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _filesFixture.Files.Contains(p));
		}

		[Fact]
		public void GetAllPathsInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllDirectoriesInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the directory paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllFilesInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the file paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), _filesFixture.RootPath));
		}

		[Fact]
		public void GetAllFilesFromEmptyDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.EmptyDirectoryPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths from an empty directory.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// There shouldn't be any paths found.
			paths.Should().HaveCount(0);
		}

		[Fact]
		public void InvalidDirectorySoShouldThrowException()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = Path.Combine(_filesFixture.RootPath, "ADirectoryThatDoesNotExist"),
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths from a directory that does not exist.
			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None).Count();

			// A DirectoryNotFound exception should be thrown.
			act.Should().Throw<DirectoryNotFoundException>();
		}

		[Fact]
		public void GetAllPathsLessThanXCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int maxPathLength = _filesFixture.AllPaths.Min(p => p.Length) + 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length <= maxPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = maxPathLength,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void GetAllPathsMoreThanXCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int minPathLength = _filesFixture.AllPaths.Max(p => p.Length) - 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length >= minPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void GetAllPathsMoreThanXAndLessThanYCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int minPathLength = _filesFixture.AllPaths.Min(p => p.Length) + 1;
			int maxPathLength = _filesFixture.AllPaths.Max(p => p.Length) - 1;
			var expectedPaths = _filesFixture.AllPaths.Where(p => p.Length >= minPathLength && p.Length <= maxPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void MinimumPathLengthGreaterThanMaximumPathLengthSoShouldThrowException()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = 2,
				MaximumPathLength = 1,
				UrlEncodePaths = false
			};

			// Because we try and get a list of paths from a directory that does not exist.
			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None).Count();

			// A DirectoryNotFound exception should be thrown.
			act.Should().Throw<MinPathLengthGreaterThanMaxPathLengthException>();
		}

		[Fact]
		public void ReplacingTheStartingDirectoryShouldAlterThePathsProperly()
		{
			// Setup the Search Options
			var newRootDirectoryName = "NewRootDirectory";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = newRootDirectoryName,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = false
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				return new PathInfo()
				{
					Path = p.Path.Replace(_filesFixture.RootPath, newRootDirectoryName)
				};
			});

			// Act.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should not have any of the original paths.
			paths.Should().NotContain(_filesFixture.AllPaths);

			// We should have the expected transformed paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void UrlEncodingThePathsShouldAlterThePathsProperly()
		{
			// Setup the Search Options
			var newRootDirectoryName = "NewRootDirectory";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = true
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				return new PathInfo()
				{
					Path = p.Path.Replace(" ", "%20")
						.Replace(@"\", "%5C")
						.Replace(":", "%3A")
				};
			});

			// Act.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should not have any of the original paths.
			paths.Should().NotContain(_filesFixture.AllPaths);

			// We should have the expected transformed paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[Fact]
		public void ReplacingTheStartingDirectoryAndUsingUrlEncodingShouldAlterThePathsProperly()
		{
			// Setup the Search Options
			var newRootDirectoryName = "NewRootDirectory";
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = _filesFixture.RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = newRootDirectoryName,
				MinimumPathLength = -1,
				MaximumPathLength = -1,
				UrlEncodePaths = true
			};

			var expectedPaths = _filesFixture.AllPaths.Select(p =>
			{
				return new PathInfo()
				{
					Path = p.Path.Replace(_filesFixture.RootPath, newRootDirectoryName)
						.Replace(" ", "%20")
						.Replace(@"\", "%5C")
						.Replace(":", "%3A")
				};
			});

			// Act.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions, CancellationToken.None);

			// We should not have any of the original paths.
			paths.Should().NotContain(_filesFixture.AllPaths);

			// We should have the expected transformed paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}
	}
}
