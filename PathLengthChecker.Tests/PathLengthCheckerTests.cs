using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;

namespace PathLengthChecker.Tests
{
	/// <summary>
	/// Tests to test the PathLengthChecker class.
	/// </summary>
	[TestClass()]
	public class PathLengthCheckerTests
	{
		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext { get; set; }

		private static List<PathInfo> _directories;
		private static List<PathInfo> _files;
		private static List<PathInfo> _allPaths;
		private static readonly string RootPath = Path.Combine(Environment.CurrentDirectory, "UnitTestTemp");
		private static readonly string EmptyDirectoryPath = Path.Combine(RootPath, "EmptyDir");

		#region Additional test attributes
		/// <summary>
		/// You can use the following additional attributes as you write your tests:
		/// Use ClassInitialize to run code before running the first test in the class
		/// </summary>
		/// <param name="testContext">The test context.</param>
		[ClassInitialize()]
		public static void MyClassInitialize(TestContext testContext)
		{
			// Specify the directories and files that the unit tests expect to be there.
			_directories = new List<PathInfo>()
			               	{
			               		new PathInfo(){Path = Path.Combine(RootPath, "TestDir1")},
								new PathInfo(){Path = Path.Combine(RootPath, "TestDir2")}, 
								new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestDir3")},
								new PathInfo(){Path = EmptyDirectoryPath}
			               	};

			_files = new List<PathInfo>()
			         	{
							new PathInfo(){Path = Path.Combine(RootPath, "TestFile0.test")},
							new PathInfo(){Path = Path.Combine(RootPath, "TestDir1\\TestFile1.test")},
							new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestFile2.test")},
							new PathInfo(){Path = Path.Combine(RootPath, "TestDir2\\TestDir3\\TestFile3.test")}
			         	};

			// Create a list that contains all paths.
			_allPaths = new List<PathInfo>(_directories);
			_allPaths.AddRange(_files);

			// Create the paths on the local hard drive.
			CreateDirectoriesAndFiles();
		}

		/// <summary>
		/// Use ClassCleanup to run code after all tests in a class have run
		/// </summary>
		[ClassCleanup()]
		public static void MyClassCleanup()
		{
			DeleteDirectoriesAndFiles();
		}

		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//

		private static void CreateDirectoriesAndFiles()
		{
			// Create the directories and files that the unit tests expect to be there.
			foreach (var diretory in _directories)
			{
				var directoryPath = Path.Combine(RootPath, diretory.Path);
				if (!Directory.Exists(directoryPath))
					Directory.CreateDirectory(directoryPath);
			}

			foreach (var file in _files)
			{
				var filePath = Path.Combine(RootPath, file.Path);
				if (!File.Exists(filePath))
					File.Create(filePath);
			}
		}

		private static void DeleteDirectoriesAndFiles()
		{
			// Delete the directories and files created for the unit tests.
			foreach (var file in _files)
			{
				var filePath = Path.Combine(RootPath, file.Path);
				if (File.Exists(filePath))
					File.Delete(filePath);
			}

			foreach (var diretory in _directories)
			{
				var directoryPath = Path.Combine(RootPath, diretory.Path);
				if (Directory.Exists(directoryPath))
					Directory.Delete(directoryPath, true);
			}
		}
		#endregion


		/// <summary>
		///A test for PathCollector Constructor
		///</summary>
		[TestMethod()]
		public void GetAllPaths()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
									{
										RootDirectory = RootPath,
										SearchOption = SearchOption.AllDirectories,
										TypesToGet = FileSystemTypes.All,
										SearchPattern = string.Empty,
										RootDirectoryReplacement = null,
										MinimumPathLength = -1,
										MaximumPathLength = -1
									};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the paths.
			paths.Should().Contain(_directories).And.Contain(_files);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _directories.Contains(p) || _files.Contains(p));
		}

		[TestMethod]
		public void GetAllDirectories()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of directory paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the directory paths.
			paths.Should().Contain(_directories);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _directories.Contains(p));
		}

		[TestMethod]
		public void GetAllFiles()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of file paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the file paths.
			paths.Should().Contain(_files);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => _files.Contains(p));
		}

		[TestMethod]
		public void GetAllPathsInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));
		}

		[TestMethod]
		public void GetAllDirectoriesInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Directories,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the directory paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));
		}

		[TestMethod]
		public void GetAllFilesInTopLevelDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.TopDirectoryOnly,
				TypesToGet = FileSystemTypes.Files,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the file paths directly off of the Root Path.
			paths.Should().Contain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => string.Equals(Path.GetDirectoryName(p.Path), RootPath));
		}

		[TestMethod]
		public void GetAllFilesFromEmptyDirectory()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = EmptyDirectoryPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths from an empty directory.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// There shouldn't be any paths found.
			paths.Should().HaveCount(0);
		}

		[TestMethod]
		public void InvalidDirectorySoShouldThrowException()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = Path.Combine(RootPath, "ADirectoryThatDoesNotExist"),
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = -1,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths from a directory that does not exist.
			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions).Count();

			// A DirectoryNotFound exception should be thrown.
			act.ShouldThrow<DirectoryNotFoundException>();
		}

		[TestMethod]
		public void GetAllPathsLessThanXCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int maxPathLength = _allPaths.Min(p => p.Length) + 1;
			var expectedPaths = _allPaths.Where(p => p.Length <= maxPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
									{
										RootDirectory = RootPath,
										SearchOption = SearchOption.AllDirectories,
										TypesToGet = FileSystemTypes.All,
										SearchPattern = string.Empty,
										RootDirectoryReplacement = null,
										MinimumPathLength = -1,
										MaximumPathLength = maxPathLength
									};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[TestMethod]
		public void GetAllPathsMoreThanXCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int minPathLength = _allPaths.Max(p => p.Length) - 1;
			var expectedPaths = _allPaths.Where(p => p.Length >= minPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = -1
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[TestMethod]
		public void GetAllPathsMoreThanXAndLessThanYCharacters()
		{
			// Get a length that doesn't include all paths, and then get the list of paths that should match the length condition.
			int minPathLength = _allPaths.Min(p => p.Length) + 1;
			int maxPathLength = _allPaths.Max(p => p.Length) - 1;
			var expectedPaths = _allPaths.Where(p => p.Length >= minPathLength && p.Length <= maxPathLength);

			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = minPathLength,
				MaximumPathLength = maxPathLength
			};

			// Because we try and get a list of paths.
			var paths = PathLengthChecker.GetPathsWithLengths(searchOptions);

			// We should have all of the paths.
			paths.Should().Contain(expectedPaths);

			// And we should not have any extra paths.
			paths.Should().OnlyContain(p => expectedPaths.Contains(p));
		}

		[TestMethod]
		public void MinimumPathLengthGreaterThanMaximumPathLengthSoShouldThrowException()
		{
			// Setup the Search Options
			var searchOptions = new PathLengthSearchOptions()
			{
				RootDirectory = RootPath,
				SearchOption = SearchOption.AllDirectories,
				TypesToGet = FileSystemTypes.All,
				SearchPattern = string.Empty,
				RootDirectoryReplacement = null,
				MinimumPathLength = 2,
				MaximumPathLength = 1
			};

			// Because we try and get a list of paths from a directory that does not exist.
			Action act = () => PathLengthChecker.GetPathsWithLengths(searchOptions).Count();

			// A DirectoryNotFound exception should be thrown.
			act.ShouldThrow<MinPathLengthGreaterThanMaxPathLengthException>();
		}
	}
}
