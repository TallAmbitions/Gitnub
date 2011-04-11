// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Nant
{
    using System.IO;
    using NAnt.Core;
    using NAnt.Core.Attributes;
    using Tall.Gitnub.Core;

    /// <summary>
    /// Task for uploading files to Github.
    /// </summary>
    [TaskName("github-addfile")]
    public class AddFiles : Task
    {
        /// <summary>
        /// Executes the task.
        /// </summary>
        protected override void ExecuteTask()
        {
            var filename = Project.GetFullPath(Filename);
            if (!File.Exists(filename))
            {
                Project.Log(this, Level.Error, string.Format("File {0} not found.", filename));
            }
            var downloads = new Downloads(Repository, UserName, UserToken);
            Project.Log(this, Level.Info, string.Format("Uploading {0} to {1}.", filename, Repository));
            downloads.AddFile(filename, Description);
        }

        /// <summary>
        /// Gets or sets the description for the file.
        /// </summary>
        /// <value>
        /// The description for the file.
        /// </value>
        [TaskAttribute("description", Required = false)]
        protected string Description { get; set; }

        /// <summary>
        /// Gets or sets the filename to upload.
        /// </summary>
        /// <value>
        /// The filename to upload.
        /// </value>
        [TaskAttribute("filename", Required = true)]
        [StringValidator(AllowEmpty = false)]
        protected string Filename { get; set; }

        /// <summary>
        /// Gets or sets the user token.
        /// </summary>
        /// <value>
        /// The user token.
        /// </value>
        [TaskAttribute("usertoken", Required = true)]
        [StringValidator(AllowEmpty = false)]
        protected string UserToken { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        [TaskAttribute("username", Required = true)]
        [StringValidator(AllowEmpty = false)]
        protected string UserName { get; set; }

        /// <summary>
        /// Gets or sets the repository.
        /// </summary>
        /// <value>
        /// The repository.
        /// </value>
        [TaskAttribute("repository", Required = true)]
        [StringValidator(AllowEmpty = false)]
        protected string Repository { get; set; }
    }
}