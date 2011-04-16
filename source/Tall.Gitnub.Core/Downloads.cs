// Copyright (c) 2011 Tall Ambitions, LLC
// See included LICENSE for details.
namespace Tall.Gitnub.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using JsonFx.Json;

    /// <summary>
    /// Class that performs operations on Github's download area.
    /// </summary>
    public class Downloads : IDownloads
    {
        private const string BaseGithubUrl = @"https://github.com/";
        private const string BaseAmazonS3Url = @"http://github.s3.amazonaws.com/";
        private readonly string repository;
        private readonly string userName;
        private readonly string userToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="Downloads"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="userToken">The user token.</param>
        public Downloads(string repository, string userName, string userToken)
        {
            if(string.IsNullOrEmpty(repository))
                throw new ArgumentException("Argument can not be null or empty.", "repository");
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Argument can not be null or empty.", "userName");
            if (string.IsNullOrEmpty(userToken))
                throw new ArgumentException("Argument can not be null or empty.", "userToken");

            this.repository = repository;
            this.userName = userName;
            this.userToken = userToken;
        }

        /// <summary>
        /// Adds a file to the downloads available for a repository.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public bool AddFile(string filename, string description = null)
        {
            var downloadBase = String.Format("{0}{1}/downloads", BaseGithubUrl, repository);
            var fileInfo = new FileInfo(filename);
            var localFilename = fileInfo.Name;

            var contentType = MimeMap.GetMimeTypeForExtension(fileInfo.Extension);
            var formValues = new NameValueCollection
                                              {
                                                  {"file_size", fileInfo.Length.ToString()},
                                                  {"content_type", contentType},
                                                  {"file_name", localFilename},
                                                  {"description", description ?? String.Empty},
                                                  {"login", this.userName},
                                                  {"token", this.userToken},
                                              };

            var result = PostFormData(downloadBase, formValues);

            using (var fileContents = fileInfo.OpenRead()) {
                var postValues = new List<FormEntry>
                                     {
                                         new FormEntry {Name = @"Filename", Value = localFilename},
                                         new FormEntry {Name = @"key", Value = (string)result["path"]},
                                         new FormEntry {Name = @"policy", Value = (string)result["policy"]},
                                         new FormEntry {Name = @"AWSAccessKeyId", Value = (string)result["accesskeyid"]},
                                         new FormEntry {Name = @"signature", Value = (string)result["signature"]},
                                         new FormEntry {Name = @"acl", Value = (string)result["acl"]},
                                         new FormEntry {Name = @"success_action_status", Value = @"201"},
                                         new FormEntry {Name = @"Content-Type", Value = (string)result["mime_type"], },
                                         // File must be the last entry
                                         new FormEntry {Name = @"file", Value = localFilename, FileContents = fileContents},
                                     };
                return !String.IsNullOrEmpty(PostMultipartData(BaseAmazonS3Url, postValues));
            }
        }

        private static IDictionary<string, object> PostFormData(string address, NameValueCollection formValues)
        {
            var values = formValues.AllKeys.Select(key => String.Format("{0}={1}",
                                     key, //TODO: UrlEncode
                                     formValues[key])); //TODO: UrlEncode
            var json = new JsonReader();
            return MakeRequest(address,
                               @"application/x-www-form-urlencoded",
                               String.Join("&", values.ToArray()),
                               reader => json.Read<Dictionary<string, object>>(reader.ReadToEnd()));
        }

        private static string PostMultipartData(string address, IEnumerable<FormEntry> values)
        {
            var boundary = Guid.NewGuid().ToString("n");
            var encoding = Encoding.UTF8;
            var boundaryBytes = encoding.GetBytes("--" + boundary);
            var newLineBytes = encoding.GetBytes("\r\n");
            var contentType = @"multipart/form-data; boundary=" + boundary;
            Action<Stream> writerFunc =
                stream =>
                    {
                        Action<byte[]> write = b => stream.Write(b, 0, b.Length);
                        Action<string> writeString = s => write(encoding.GetBytes(s));
                        Action writeNewLine = () => write(newLineBytes);
                        foreach (var entry in values)
                        {
                            write(boundaryBytes);
                            writeNewLine();

                            var header = String.Format(@"Content-Disposition: form-data; name=""{0}""", entry.Name);
                            var isFile = false;
                            if(entry.FileContents != null)
                            {
                                header = String.Format(@"{0}; filename=""{1}""", header, entry.Value);
                                isFile = true;
                            }
                            writeString(header);
                            writeNewLine();
                            writeNewLine();

                            if (isFile)
                            {
                                //TODO: factor out stream-to-stream copy.
                                var buf = new byte[1 << 15];
                                int count;
                                while ((count = entry.FileContents.Read(buf, 0, buf.Length)) > 0)
                                {
                                    stream.Write(buf, 0, count);
                                }
                            }
                            else
                            {
                                writeString(entry.Value);
                            }
                            writeNewLine();
                        }
                        writeString("--" + boundary);
                    };
            return MakeRequest(address, contentType, writerFunc, r => r.ReadToEnd());
        }

        private static T MakeRequest<T>(string address, string contentType, string content, Func<TextReader, T> responseHandler)
        {
            return MakeRequest(
                address, contentType, s => { using (var writer = new StreamWriter(s)) writer.Write(content); }, responseHandler);
        }

        private static T MakeRequest<T>(string address, string contentType, Action<Stream> contentWriter, Func<TextReader, T> responseHandler)
        {
            var request = WebRequest.Create(address);
            request.Method = @"POST";
            request.ContentType = contentType;

            using (var stream = request.GetRequestStream())
            {
                contentWriter(stream);
            }
            using (var response = request.GetResponse())
            {
                if (response == null)
                {
                    throw new WebException("Unable to get response.", WebExceptionStatus.ConnectFailure);
                }
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        throw new WebException("Unable to get response.", WebExceptionStatus.ConnectFailure);
                    }
                    using (var reader = new StreamReader(responseStream))
                    {
                        return responseHandler(reader);
                    }
                }
            }
        }

        private class FormEntry
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Stream FileContents { get; set; }
        }
    }
}