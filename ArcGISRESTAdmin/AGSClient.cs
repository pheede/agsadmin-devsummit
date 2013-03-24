// (c) Copyright ESRI.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using ArcGISRESTAdmin.Classes;

namespace ArcGISRESTAdmin
{
    public class AGSClient
    {
        public Uri ServerUrl { get; private set; }
        public string Username { get; private set; }
        public DateTime TokenExpiration { get; private set; }

        public string Token { get; private set; }
        private string Password { get; set; }

        private HttpClient http = new HttpClient();

        /// <summary>
        /// Indicates whether the connection has been authenticated.
        /// </summary>
        public bool IsAuthenticated
        {
            get
            {
                return !string.IsNullOrEmpty(Token);
            }
        }

        /// <summary>
        /// Indicates whether the current token has expired.
        /// </summary>
        public bool IsTokenExpired
        {
            get
            {
                if (!IsAuthenticated) throw new InvalidOperationException("Connection has not yet been authenticated.");

                return TokenExpiration < DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Construct a new AGSClient object for performing administrative requests against the specified ArcGIS Server admin endpoint with
        /// the given administrator credentials.
        /// </summary>
        /// <param name="serverUrl">Administrative endpoint of the form http(s)://server/arcgis/admin/</server></param>
        /// <param name="username">Admin username</param>
        /// <param name="password">Admin password</param>
        public AGSClient(string serverUrl, string username, string password)
        {
            if (!serverUrl.EndsWith("/")) serverUrl += "/";

            ServerUrl = new Uri(serverUrl);
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Authenticate against the defined ArcGIS Server and store the received token for future administrative requests.
        /// </summary>
        /// <returns></returns>
        public async Task Authenticate()
        {
            Uri encryptionInfoEndpoint = new Uri(ServerUrl, "publicKey");
            var ei = await GetStringAsync(encryptionInfoEndpoint, addToken: false);

            byte[] exponent = EncodingHelper.HexToBytes(ei["publicKey"].Value<string>());
            byte[] modulus = EncodingHelper.HexToBytes(ei["modulus"].Value<string>());

            string encryptedUsername, encryptedPassword, encryptedClient;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(512))
            {
                RSAParameters rsaParms = new RSAParameters() { Exponent = exponent, Modulus = modulus };
                rsa.ImportParameters(rsaParms);

                encryptedUsername = EncodingHelper.BytesToHex(rsa.Encrypt(Encoding.UTF8.GetBytes(Username), false));
                encryptedPassword = EncodingHelper.BytesToHex(rsa.Encrypt(Encoding.UTF8.GetBytes(Password), false));
                encryptedClient = EncodingHelper.BytesToHex(rsa.Encrypt(Encoding.UTF8.GetBytes("requestip"), false));
            }

            Uri tokenEndpoint = new Uri(ServerUrl, "generateToken");

            var data = new[] { new KeyValuePair<string, string>("username", encryptedUsername),
                               new KeyValuePair<string, string>("password", encryptedPassword), 
                               new KeyValuePair<string, string>("client", encryptedClient),
                               new KeyValuePair<string, string>("encrypted", "true")  };
            var content = new FormUrlEncodedContent(data);
                
            var tokenInfo = await PostAsync(tokenEndpoint, content, addToken: false);
            Token = tokenInfo["token"].Value<string>();

            var expirationUnixTimestamp = tokenInfo["expires"].Value<long>();
            TokenExpiration = EncodingHelper.DateTimeFromUnixTimestampMillis(expirationUnixTimestamp);
        }

        /// <summary>
        /// Get a list of all folders on the server. Optionally include default system generated folders.
        /// </summary>
        /// <param name="includeSystemFolders">Boolean signifying whether to include the system generated folders (currently System and Utilities) if they exist.</param>
        /// <returns>List of folder names</returns>
        public async Task<IEnumerable<string>> GetFolderNames(bool includeSystemFolders = false)
        {
            Uri rootFolderEndpoint = new Uri(ServerUrl, "services");

            ServicesResponse response = await GetStringAsync<ServicesResponse>(rootFolderEndpoint);

            var folders = (from folderDetail in response.foldersDetail select folderDetail.folderName);
            if (!includeSystemFolders) folders = (from folder in folders where folder != "System" && folder != "Utilities" select folder);

            return folders;
        }

        public async Task<ServiceReport[]> GetServiceReports(string folder = "")
        {
            Uri endpoint;
            if (string.IsNullOrEmpty(folder)) endpoint = new Uri(ServerUrl, "services/report");
            else endpoint = new Uri(ServerUrl, string.Format("services/{0}/report", folder));

            ServicesReportResponse response = await GetStringAsync<ServicesReportResponse>(endpoint);

            return response.reports;
        }

        /// <summary>
        /// Get a report for all services in all folders.
        /// </summary>
        /// <returns>A dictionary mapping each folder name to an array of service reports for that folder.</returns>
        public async Task<Dictionary<string, ServiceReport[]>> GetAllServiceReports()
        {
            var folders = await GetFolderNames();

            var folderTasks = new Dictionary<string, Task<ServiceReport[]>>();

            folderTasks.Add("/", GetServiceReports());

            foreach (string folder in folders)
            {
                folderTasks.Add(folder, GetServiceReports(folder));
            }

            await Task.WhenAll(folderTasks.Values.ToArray());

            return folderTasks.ToDictionary(
                dict => { return dict.Key; },
                dict => { return dict.Value.Result; }
                );
        }

        /// <summary>
        /// Get all log messages with a minimum log level within a certain date range (if specified).
        /// </summary>
        /// <param name="minimumLogLevel">The minimum log level of messages to return</param>
        /// <param name="startTime">The earliest time for which to return log entries.</param>
        /// <param name="endTime">The latest time for which to return log entries.</param>
        /// <returns>A list of LogMessage objects matching the specified log level and date range (if specified)</returns>
        public async Task<IEnumerable<LogMessage>> GetLogs(LogMessage.LogType minimumLogLevel, DateTime? startTime = null, DateTime? endTime = null)
        {
            var parms = new Dictionary<string, string>();
            parms["level"] = minimumLogLevel.ToString().ToUpper();
            parms["filter"] = @"{""server"": ""*"", ""services"": ""*"", ""machines"":""*"" }"; // for now always request all servers, services, and machine 
            if (startTime != null) parms["startTime"] = EncodingHelper.GetUnixTimestampMillis(startTime.Value).ToString();
            if (endTime != null) parms["endTime"] = EncodingHelper.GetUnixTimestampMillis(endTime.Value).ToString();

            Uri logEndpoint = new Uri(ServerUrl, "logs/query");

            LogQueryResponse response = await GetStringAsync<LogQueryResponse>(logEndpoint, parms);

            return response.logMessages;
        }

        /// <summary>
        /// Publish a pre-created service definition.
        /// </summary>
        /// <param name="fi">FileInfo pointing to the .SD file to publish</param>
        /// <returns>The jobid of the publishing job. Publishing status can be retrieved using this id.</returns>
        public async Task<string> PublishServiceDefinition(System.IO.FileInfo fi)
        {
            var uploadResponse = await UploadItem(fi);

            // Publish Service Definition GP tool takes one required parameter in_sdp_id which specifies the ID of the uploaded .sd
            // simply omitting other optional parameters
            var parms = new[] { new KeyValuePair<string, string>("in_sdp_id", uploadResponse.item.itemID) };
            
            // Publish Service Definition is a system published GP tool hosted outside the main ArcGIS for Server Admin REST API
            Uri publishEndpoint = new Uri(ServerUrl, "/arcgis/rest/services/System/PublishingTools/GPServer/Publish Service Definition/submitJob");

            var publishResponse = await GetStringAsync(publishEndpoint, parms);

            return publishResponse["jobId"].Value<string>();
        }

        /// <summary>
        /// Upload a file to the server for future use such as e.g. an .SD file for publishing as a service.
        /// </summary>
        /// <param name="fi">The file to upload</param>
        /// <param name="description">An optional description of the file to be uploaded</param>
        /// <returns>UploadResponse object containing among other things an itemId identifying the uploaded file.</returns>
        public async Task<UploadResponse> UploadItem(System.IO.FileInfo fi, string description = "")
        {
            Uri uploadEndpoint = new Uri(ServerUrl, "uploads/upload");

            MultipartFormDataContent content = new MultipartFormDataContent();
            
            // setup file content and appropriate headers
            var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fi.FullName));
            fileContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                FileName = fi.Name,
                Name = "itemFile"

            };
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent);

            // add description if provided
            if (!string.IsNullOrEmpty(description)) content.Add(new StringContent(description), "description");

            UploadResponse response = await PostAsync<UploadResponse>(uploadEndpoint, content);

            return response;
        }


        #region Internal methods not intended to be used outside of the library

        /// <summary>
        /// Add format specifier and any additional specified parameters to querystring of provided Uri and optionally an administrative token.
        /// </summary>
        /// <param name="uri">Base Uri to add values to</param>
        /// <param name="param">Any additional querystring parameters and values to add</param>
        /// <param name="addToken">Boolean indicator for whether to add administrative token to Uri</param>
        /// <returns>Modified Uri with added querystring parameters</returns>
        internal async Task<Uri> FixupUri(Uri uri, IEnumerable<KeyValuePair<string, string>> param = null, bool addToken = true)
        {
            if (addToken && (!IsAuthenticated || IsTokenExpired)) { await Authenticate(); }

            var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query.ToString());

            if (param != null)
            {
                foreach (var kv in param)
                {
                    queryString[kv.Key] = kv.Value;
                }
            }

            if (addToken) queryString["token"] = Token;
            queryString["f"] = "json";

            uri = new Uri(uri, string.Format("?{0}", queryString.ToString()));

            return uri;
        }

        /// <summary>
        /// Send a GET request to the provided Uri with optional extra querystring parameters and token added to it. Return response deserialized into object of type T.
        /// </summary>
        /// <typeparam name="T">Class to deserialize response to</typeparam>
        /// <param name="uri">Uri specifying endpoint to send GET request to</param>
        /// <param name="param">Any additional parameters to add to the querystring</param>
        /// <param name="addToken">Boolean signifying whether to add administrative token to request</param>
        /// <returns>Object of type T with response</returns>
        internal async Task<T> GetStringAsync<T>(Uri uri, IEnumerable<KeyValuePair<string, string>> param = null, bool addToken = true)
        {
            uri = await FixupUri(uri, param, addToken);
            string response = await http.GetStringAsync(uri);

            return ParseAndValidateJSONResponse<T>(response);
        }

        /// <summary>
        /// Send a GET request to the provided Uri with optional extra querystring parameters and token added to it. Return response as JObject instance.
        /// </summary>
        /// <param name="uri">Uri specifying endpoint to send GET request to</param>
        /// <param name="param">Any additional parameters to add to the querystring</param>
        /// <param name="addToken">Boolean signifying whether to add administrative token to request</param>
        /// <returns>JObject instance with response</returns>
        internal async Task<JObject> GetStringAsync(Uri uri, IEnumerable<KeyValuePair<string, string>> param = null, bool addToken = true)
        {
            uri = await FixupUri(uri, param, addToken);
            string response = await http.GetStringAsync(uri);

            return ParseAndValidateGenericJSONResponse(response);
        }

        internal async Task<string> InternalPostAsync(Uri uri, HttpContent content, bool addToken)
        {
            uri = await FixupUri(uri, addToken: addToken);

            HttpResponseMessage responseMessage = await http.PostAsync(uri, content);
            string response = await responseMessage.Content.ReadAsStringAsync();
            
            return response;
        }

        /// <summary>
        /// POST the given content to the specified Uri and optionally add an admin token to the request. Returns the response as a general JObject object.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="addToken"></param>
        /// <returns>A JObject with JSON response</returns>
        internal async Task<JObject> PostAsync(Uri uri, HttpContent content, bool addToken = true)
        {
            string response = await InternalPostAsync(uri, content, addToken);            
            return ParseAndValidateGenericJSONResponse(response);
        }

        /// <summary>
        /// POST the given content to the specified Uri and optionally add an admin token to the request. Returns the response deserialized into the specified class.
        /// </summary>
        /// <typeparam name="T">The class into which the response will be deserialized.</typeparam>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="addToken"></param>
        /// <returns>A class of type T with the deserialized JSON response</returns>
        internal async Task<T> PostAsync<T>(Uri uri, HttpContent content, bool addToken = true)
        {
            string response = await InternalPostAsync(uri, content, addToken);
            return ParseAndValidateJSONResponse<T>(response);
        }

        /// <summary>
        /// Parse a JSON string into class T. Checks for ArcGIS Server error message response and throws Exception if found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        internal T ParseAndValidateJSONResponse<T>(string response)
        {
            JObject jo = JObject.Parse(response);

            JToken error;
            if (jo.TryGetValue("status", out error) && error.Value<string>() == "error")
            {
                var messages = jo.GetValue("messages").ToArray();
                throw new Exception(messages[0].Value<string>()); // just grab first message for now
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(response);
        }

        /// <summary>
        /// Parse a JSON string into a JObject object. Checks for ArcGIS Server error message response and throw Exception if found.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        internal JObject ParseAndValidateGenericJSONResponse(string response)
        {
            JObject jo = JObject.Parse(response);

            JToken error;
            if (jo.TryGetValue("status", out error) && error.Value<string>() == "error")
            {
                var messages = jo.GetValue("messages").ToArray();
                throw new Exception(messages[0].Value<string>()); // just grab first message for now
            }

            return jo;
        }

        #endregion
    }
}
