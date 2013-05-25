/*
 * Copyright (C) 2007, Dave Watson <dwatson@mimvista.com>
 * Copyright (C) 2008, Robin Rosenberg <robin.rosenberg@dewire.com>
 * Copyright (C) 2010, Henon <meinrad.recheis@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GitSharp.Core;

namespace GitSharp
{
	public class RepositoryStatus
	{

		private GitIndex _index;
		private Core.Tree _tree;
		private string _root_path;
		private bool _recursive;
		private string _file_path;

		public RepositoryStatus(Repository repository)
			: this(repository, new RepositoryStatusOptions { ForceContentCheck = true })
		{
		}

		public RepositoryStatus(Repository repository, RepositoryStatusOptions options)
		{
			Repository = repository;
			Options = options;
			_root_path = string.Empty;
			_recursive = true;
			Update();
		}

		internal RepositoryStatus(Repository repository, RepositoryStatusOptions options, string singleFile, string rootDir, bool recursive)
		{
			Repository = repository;
			Options = options ?? new RepositoryStatusOptions { ForceContentCheck = true };
			IgnoreHandler = new IgnoreHandler(Repository);
			_root_path = Repository.ToGitPath (rootDir);
			_recursive = recursive;
			_file_path = Repository.ToGitPath (singleFile);
			Update();
		}

		public Repository Repository
		{
			get;
			private set;
		}
		
		/// <summary>
		/// 
		/// </summary>
		public RepositoryStatusOptions Options { get; set; }

		/// <summary>
		/// List of files added to the index, which are not in the current commit
		/// </summary>
		public HashSet<string> Added { get; private set; }

		/// <summary>
		/// List of files added to the index, which are already in the current commit with different content
		/// </summary>
		public HashSet<string> Staged { get; private set; }

		/// <summary>
		/// List of files removed from the index but are existent in the current commit
		/// </summary>
		public HashSet<string> Removed { get; private set; }

		/// <summary>
		/// List of files existent in the index but are missing in the working directory
		/// </summary>
		public HashSet<string> Missing { get; private set; }

		/// <summary>
		/// List of files with unstaged modifications. A file may be modified and staged at the same time if it has been modified after adding.
		/// </summary>
		public HashSet<string> Modified { get; private set; }

		/// <summary>
		/// List of files existing in the working directory but are neither tracked in the index nor in the current commit.
		/// </summary>
		public HashSet<string> Untracked { get; private set; }

		/// <summary>
		/// List of files with staged modifications that conflict.
		/// </summary>
		public HashSet<string> MergeConflict { get; private set; }

		///// <summary>
		///// Returns the number of files checked into the git repository
		///// </summary>
		//public int IndexSize { get { return _index.Members.Count; } }

		public bool AnyDifferences { get; private set; }

		/// <summary>
		/// Recalculates the status
		/// </summary>
		public void Update()
		{
			AnyDifferences = false;
			Added = new HashSet<string>();
			Staged = new HashSet<string>();
			Removed = new HashSet<string>();
			Missing = new HashSet<string>();
			Modified = new HashSet<string>();
			Untracked = new HashSet<string>();
			MergeConflict = new HashSet<string>();
			IgnoreHandler = new IgnoreHandler(Repository);
			
			if (_file_path != null)
				UpdateSingleFile (_file_path);
			else if (_recursive)
				UpdateDirectoryRecursive (_root_path);
			else
				UpdateDirectoryNotRecursive (_root_path);
		}

		/// <summary>
		/// Run the diff operation. Until this is called, all lists will be empty
		/// </summary>
		/// <returns>true if anything is different between index, tree, and workdir</returns>
		private void UpdateDirectoryRecursive (string path)
		{
			var commit = Repository.Head.CurrentCommit;
			_tree = (commit != null ? commit.Tree : new Core.Tree(Repository));
			_index = Repository.Index.GitIndex;
			_index.RereadIfNecessary();
			DirectoryInfo root = _index.Repository.WorkingDirectory;
			var visitor = new AbstractIndexTreeVisitor { VisitEntryAux = OnVisitEntry };
			new IndexTreeWalker(_index, _tree, CreateWorkingDirectoryTree(path), root, visitor).Walk();
		}

		private void UpdateDirectoryNotRecursive (string path)
		{
			_index = Repository.Index.GitIndex;
			
			// Tree that will hold the working dir file entries
			var wtree = new Core.Tree(Repository._internal_repo);
			
			// Get a list of a leaves in the path
			
			Tree commitTree = null;
			var commit = Repository.Head.CurrentCommit;
			commitTree = commit != null ? commit.Tree : null;
			if (commitTree != null)
				commitTree = commitTree[path] as Tree;
			
			Dictionary<string,Leaf> commitEntries;
			if (commitTree != null)
				commitEntries = commitTree.Leaves.ToDictionary (l => l.Path);
			else
				commitEntries = new Dictionary<string, Leaf> ();
			
			HashSet<string> visited = new HashSet<string> ();

			// Compare commited files and working tree files
			
			DirectoryInfo dir = new DirectoryInfo (Repository.FromGitPath (path));
			if (dir.Exists) {
				foreach (FileInfo fileInfo in dir.GetFiles ()) {
					string file = path + "/" + fileInfo.Name;
					
					Leaf lf;
					if (commitEntries.TryGetValue (file, out lf)) {
						// Remove from the collection. At the end of the loop, entries still remaining in the
						// collection will be processed as not having a corresponding working dir file
						commitEntries.Remove (file);
					}
					
					TreeEntry wdirEntry = null;
					if (!IgnoreHandler.IsIgnored (file) && fileInfo.Exists)
						wdirEntry = wtree.AddFile (file);
					
					GitIndex.Entry indexEntry = _index.GetEntry (file);
					
					OnVisitEntry (lf != null ? lf.InternalEntry : null, wdirEntry, indexEntry, fileInfo);
					visited.Add (file);
				}
			}
			
			// Now visit entries for which a working dir file was not found
			
			foreach (var lf in commitEntries) {
				string file = lf.Key;
				FileInfo fileInfo = new FileInfo (Repository.FromGitPath (file));
				GitIndex.Entry indexEntry = _index.GetEntry (file);
				OnVisitEntry (lf.Value.InternalEntry, null, indexEntry, fileInfo);
				visited.Add (file);
			}
			
			// Finally, visit remaining index entries which are not in the working dir nor in the commit
			
			foreach (var ie in _index.Members) {
				string file = ie.Name;
				// Exclude entries in subdirectories of _root_path 
				int i = file.LastIndexOf ('/');
				string fdir = i != -1 ? file.Substring (0, i) : string.Empty;
				if (fdir == _root_path && !visited.Contains (file))
					OnVisitEntry (null, null, ie, new FileInfo (Repository.FromGitPath (file)));
			}
		}
		
		private void UpdateSingleFile (string file)
		{
			TreeEntry treeEntry = null;
			var commit = Repository.Head.CurrentCommit;
			_tree = commit != null ? commit.Tree : null;
			if (_tree != null)
				treeEntry = _tree.FindBlobMember (file);
			
			_index = Repository.Index.GitIndex;
			_index.RereadIfNecessary();
			GitIndex.Entry indexEntry = _index.GetEntry (file);
			
			TreeEntry wdirEntry = null;
			FileInfo fileInfo = new FileInfo (Path.Combine (Repository.WorkingDirectory, file.Replace ('/', Path.DirectorySeparatorChar)));
			if (fileInfo.Exists && !IgnoreHandler.IsIgnored(file)) {
				var tree = new Core.Tree(Repository._internal_repo);
				wdirEntry = tree.AddFile (file);
			}
			
			OnVisitEntry (treeEntry, wdirEntry, indexEntry, fileInfo);
		}

		private GitSharp.Core.Tree CreateWorkingDirectoryTree(string subdir)
		{
			var mainTree = new Core.Tree(Repository._internal_repo);
			var tree = mainTree;
			
			// Create the subdir branch
			foreach (string sdir in subdir.Split (new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries))
				tree = tree.AddTree (sdir);
			
			DirectoryInfo root = new DirectoryInfo (Repository.FromGitPath (subdir));
			if (root.Exists)
				FillTree(root, tree);
			return mainTree;
		}

		private IgnoreHandler IgnoreHandler
		{
			get;
			set;
		}

		private void FillTree(DirectoryInfo dir, Core.Tree tree)
		{
			foreach (var subdir in dir.GetDirectories())
			{
				if (subdir.Name == Constants.DOT_GIT || IgnoreHandler.IsIgnored(tree.FullName + "/" + subdir.Name))
					continue;
				var t = tree.AddTree(subdir.Name);
				FillTree(subdir, t);
			}
			foreach (var file in dir.GetFiles())
			{
				if (IgnoreHandler.IsIgnored(tree.FullName + "/" + file.Name))
					continue;
				tree.AddFile((file.Name));
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="treeEntry"></param>
		/// <param name="wdirEntry">Note: wdirEntry is the non-ignored working directory entry.</param>
		/// <param name="indexEntry"></param>
		/// <param name="file">Note: gitignore patterns do not influence this parameter</param>
		private void OnVisitEntry(TreeEntry treeEntry, TreeEntry wdirEntry, GitIndex.Entry indexEntry, FileInfo file)
		{
			//Console.WriteLine(" ----------- ");
			//if (treeEntry != null)
			//   Console.WriteLine("tree: " + treeEntry.Name);
			//if (wdirEntry != null)
			//   Console.WriteLine("w-dir: " + wdirEntry.Name);
			//if (indexEntry != null)
			//   Console.WriteLine("index: " + indexEntry.Name);
			//Console.WriteLine("file: " + file.Name);
			
			string subdir_prefix = !string.IsNullOrEmpty (_root_path) ? _root_path + "/" : null;
			
			PathStatus path_status = null;
			if (indexEntry != null)
			{
				if (subdir_prefix != null && !indexEntry.Name.StartsWith (subdir_prefix))
					return; // File outside the directory
				
				if (treeEntry == null)
				{
					path_status = OnAdded(indexEntry.Name, path_status);
				}
				if (treeEntry != null && !treeEntry.Id.Equals(indexEntry.ObjectId))
				{
					Debug.Assert(treeEntry.FullName == indexEntry.Name);
					path_status = OnStaged(indexEntry.Name, path_status);
				}
				if (!file.Exists)
				{
					path_status = OnMissing(indexEntry.Name, path_status);
				}
				if (file.Exists && indexEntry.IsModified(new DirectoryInfo(Repository.WorkingDirectory), Options.ForceContentCheck))
				{
					path_status = OnModified(indexEntry.Name, path_status);
				}
				if (indexEntry.Stage != 0)
				{
					path_status = OnMergeConflict(indexEntry.Name, path_status);
				}
			}
			else // <-- index entry == null
			{
				if (treeEntry != null && subdir_prefix != null && !treeEntry.FullName.StartsWith (subdir_prefix))
					return; // File outside the directory
				
				if (treeEntry != null && !(treeEntry is Core.Tree))
				{
					path_status = OnRemoved(treeEntry.FullName, path_status);
				}
				if (wdirEntry != null) // actually, we should enforce (treeEntry == null ) here too but original git does not, may be a bug. 
					path_status = OnUntracked(wdirEntry.FullName, path_status);
			}
			if (Options.PerPathNotificationCallback != null && path_status != null)
				Options.PerPathNotificationCallback(path_status);
		}

		private PathStatus OnAdded(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.IndexPathStatus = IndexPathStatus.Added;
			}
			Added.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnStaged(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.IndexPathStatus = IndexPathStatus.Staged;
			}
			Staged.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnMissing(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.WorkingPathStatus = WorkingPathStatus.Missing;
			}
			Missing.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnModified(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.WorkingPathStatus = WorkingPathStatus.Modified;
			}
			Modified.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnMergeConflict(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.IndexPathStatus = IndexPathStatus.MergeConflict;
			}
			MergeConflict.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnRemoved(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.IndexPathStatus = IndexPathStatus.Removed;
			}
			Removed.Add(path);
			AnyDifferences = true;
			return status;
		}

		private PathStatus OnUntracked(string path, PathStatus status)
		{
			if (Options.PerPathNotificationCallback != null)
			{
				if (status == null)
					status = new PathStatus(Repository, path);
				status.WorkingPathStatus = WorkingPathStatus.Untracked;
			}
			Untracked.Add(path);
			AnyDifferences = true;
			return status;
		}
	}

	/// <summary>
	/// RepositoryStatus options allow customizing of the status checking routines. 
	/// </summary>
	public class RepositoryStatusOptions
	{
		/// <summary>
		/// If filetime and index entry time are equal forces a full content check. This can be costly for large repositories.
		/// </summary>
		public bool ForceContentCheck { get; set; }
		/// <summary>
		/// If you want to get instant per path status info while the algorithm traverses working directry, index and commit tree set this callback. Note,
		/// that it is fired only if RepositoryStatus detects differences.
		/// </summary>
		public Action<PathStatus> PerPathNotificationCallback { get; set; }
	}

	/// <summary>
	/// Status information for a single path in the working directry or index. See RepositoryStatusOptions.PerPathNotification for more information.
	/// </summary>
	public class PathStatus
	{
		public PathStatus(Repository repo, string path)
		{
			Repository = repo;
			Path = path;
			WorkingPathStatus = WorkingPathStatus.Unchanged;
			IndexPathStatus = IndexPathStatus.Unchanged;
		}

		public Repository Repository { get; set; }
		/// <summary>
		/// Relative repository path.
		/// </summary>
		public string Path { get; set; }
		/// <summary>
		/// Name of the file
		/// </summary>
		public string Name
		{
			get
			{
				if (Path == null)
					return null;
				return System.IO.Path.GetFileName(Path);
			}
		}
		public WorkingPathStatus WorkingPathStatus { get; set; }
		public IndexPathStatus IndexPathStatus { get; set; }
	}

	public enum WorkingPathStatus { Unchanged, Modified, Missing, Untracked }
	public enum IndexPathStatus { Unchanged, Added, Removed, Staged, MergeConflict }
}
