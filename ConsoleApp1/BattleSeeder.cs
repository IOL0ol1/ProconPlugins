using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public enum SupportedBattlelogGame
    {
        /// <summary>
        /// Battlefield 3
        /// </summary>
        BF3 = BattlelogGame.BF3,

        /// <summary>
        /// Battlefield 4
        /// </summary>
        BF4 = BattlelogGame.WARSAW,
    }

    /// <summary>
    /// Enum with battlelog games
    /// Important: Values come directly from battlelog (as of 2019-07-06) and shouldn't be altered (unless you know what you are doing)
    /// </summary>
    public enum BattlelogGame
    {
        /// <summary>
        /// Battlefield: Bad Company 2
        /// </summary>
        BFBC2 = 1,

        /// <summary>
        /// Battlefield 3
        /// </summary>
        BF3 = 2,

        /// <summary>
        /// Battlefield 1942
        /// </summary>
        BF1942 = 4,

        /// <summary>
        /// Battlefield 1943
        /// </summary>
        BF1943 = 8,

        /// <summary>
        /// Battlefield Vietnam
        /// </summary>
        BFVIETNAM = 16,

        /// <summary>
        /// Battlefield 2
        /// </summary>
        BF2 = 32,

        /// <summary>
        /// Battlefield 2142
        /// </summary>
        BF2142 = 64,

        /// <summary>
        /// Battlefield: Bad Company
        /// </summary>
        BFBC = 128,

        /// <summary>
        /// Battlefield Heroes
        /// </summary>
        BFHEROES = 256,

        /// <summary>
        /// Battlefield 2: Modern Combat
        /// </summary>
        BFMC = 512,

        /// <summary>
        /// Battlefield Play4Free
        /// </summary>
        BFP4F = 1024,

        /// <summary>
        /// Battlefield 4
        /// </summary>
        WARSAW = 2048,

        /// <summary>
        /// Medal of Honor: Warfighter
        /// </summary>
        MOHW = 4096,

        /// <summary>
        /// Battlefield Hardline
        /// </summary>
        BFH = 8192,
    }

    /// <summary>
    /// Class used to interact with battlelog and it's gameservers
    /// </summary>
    public class BattlelogSession
    {
        /// <summary>
        /// Username (E-Mail) used to login into battlelog
        /// </summary>
        private readonly string _username;

        /// <summary>
        /// Password used to login into battlelog
        /// </summary>
        private readonly string _password;

        /// <summary>
        /// Gets or sets a value that indicates whether a valid session was created
        /// </summary>
        private bool _hasSession;

        /// <summary>
        /// The <see cref="HttpClientHandler"/> used to make the requests
        /// </summary>
        private readonly HttpClientHandler _handler;

        /// <summary>
        /// The postChecksum for this session.
        /// Used to verify some special requests
        /// </summary>
        private string _postChecksum;

        /// <summary>
        /// The personaId for this user
        /// </summary>
        private long _personaId;

        /// <summary>
        /// Creates a new <see cref="BattlelogSession"/> object. Used to reserve slots on gameservers
        /// </summary>
        /// <param name="username">The username (E-Mail) to login with</param>
        /// <param name="password">The password to login with</param>
        public BattlelogSession(string username, string password)
        {
            this._username = username;
            this._password = password;

            // Initializing all fields
            this._hasSession = false;
            this._handler = new HttpClientHandler();
        }

        /// <summary>
        /// Creates a new session with the given credential.
        /// This method needs to be called before anything other is done
        /// </summary>
        public async Task CreateSessionAsync()
        {
            NameValueCollection queryParameters = HttpUtility.ParseQueryString(string.Empty);
            queryParameters.Add("redirect_uri", "https://battlelog.battlefield.com/sso/?tokentype=code");
            queryParameters.Add("response_type", "code");
            queryParameters.Add("client_id", "battlelog");

            // Create the auth url
            string url = "https://accounts.ea.com/connect/auth?" + queryParameters.ToString();

            using (HttpClient client = this.CreateHttpClient())
            {
                // Call the auth url
                // This will/should redirect several times
                HttpResponseMessage response = await client.GetAsync(url);

                // Create the data that should be posted to the given url
                // The next request will contain our credential
                FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "email", this._username },
                    { "password", this._password },
                    { "_eventId", "submit" },
                });

                // Post the login data to the given url
                response = await client.PostAsync(response.RequestMessage.RequestUri, data);

                // Read the response body
                // It contains some javascript which would redirect the user to the next page
                // Since we are not in a browser we will extract and call the given url manually
                string content = await response.Content.ReadAsStringAsync();

                // Extract the url from the response body
                string redirectUri = null;

                Match match = Regex.Match(content, @"var redirectUri\s*=\s*[""'](.*?)[""'];");
                if (match != null && match.Groups.Count > 1)
                {
                    redirectUri = match.Groups[1].Value;
                }

                // Check that a url was found
                // If not, our credential may be wrong / invalid
                if (string.IsNullOrWhiteSpace(redirectUri))
                {
                    throw new Exception($"An error occured while creating a session for { this._username }. Please check the credential!");
                }

                url = redirectUri + "&_eventId=end";

                // Call the extracted url
                // This will/should also redirect several times
                response = await client.GetAsync(url);

                // Once we reached this point all necessary request have been made and we'll be logged in

                // The next step would be to cache the postChecksum and the personaId
                // The postChecksum is a checksum which verifies some requests which we'll do later

                // Do one last request using the url in the location header
                content = await client.GetStringAsync(response.Headers.Location);

                // Find the globalContext of our session. It should contain the postChecksum and the personaId aswell
                match = Regex.Match(content, @"Surface\.globalContext = (.+);");
                if (match != null && match.Groups.Count > 1)
                {
                    JObject deserializedObject = JsonConvert.DeserializeObject(match.Groups[1].Value) as JObject;
                    this._postChecksum = (deserializedObject?["session"]?["postChecksum"] as JValue)?.Value?.ToString();

                    // Check that the postChecksum was found
                    if (string.IsNullOrWhiteSpace(this._postChecksum))
                    {
                        // If not, try to obtain it from an alternative way
                        // It's simply the first ten characters of our battlelog session cookie
                        this._postChecksum = this._handler.CookieContainer.GetCookies(response.RequestMessage.RequestUri)["beaker.session.id"].Value.Substring(0, 10);

                        if (string.IsNullOrWhiteSpace(this._postChecksum))
                        {
                            throw new Exception($"An error occured while fetching the postChecksum for { this._username }. Guess battlelog had some changes.");
                        }
                    }

                    if (long.TryParse((deserializedObject?["staticContext"]?["activePersona"]?["personaId"] as JValue)?.Value?.ToString(), out this._personaId) == false)
                    {
                        throw new Exception($"An error occured while fetching the personaId for { this._username }. Guess battlelog had some changes.");
                    }
                }
            }

            // Indicate that a session was (successfully) created
            this._hasSession = true;
        }

        /// <summary>
        /// Reserves a slot on the given server
        /// </summary>
        /// <param name="gameId">GameId of server</param>
        /// <param name="game">Game to which the gameId belongs to</param>
        public async Task<string> EnterServerAsync(long gameId, SupportedBattlelogGame game)
        {
            string result;

            // Check that the specified game is really supported
            this.CheckGame(game);

            // Check that a session was created
            this.CheckSession();

            using (HttpClient client = this.CreateHttpClient())
            {
                // Create the full url
                string url = $"http://battlelog.battlefield.com/{ game.ToString().ToLower() }/launcher/reserveslotbygameid/1/{ this._personaId }/{ gameId }/1//0";

                // Create the data that should be posted to the given url
                FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "post-check-sum", this._postChecksum },
                });

                // Reserve a slot on the server
                HttpResponseMessage response = await client.PostAsync(url, data);

                // Read the response body
                // It contains some info about the join state
                string content = await response.Content.ReadAsStringAsync();

                // Read the state from response
                string joinState = ((JsonConvert.DeserializeObject(content) as JObject)?["data"]?["joinState"] as JValue)?.Value?.ToString();

                // Set the state (or if there is no the whole content) as result
                result = string.IsNullOrWhiteSpace(joinState) ? content : joinState;
            }

            return result;
        }

        /// <summary>
        /// Removes the reservation for the given server
        /// </summary>
        /// <param name="gameId">GameId of server</param>
        /// <param name="game">Game to which the gameId belongs to</param>
        public async Task LeaveServerAsync(long gameId, SupportedBattlelogGame game)
        {
            // Check that the specified game is really supported
            this.CheckGame(game);

            // Check that a session was created
            this.CheckSession();

            using (HttpClient client = this.CreateHttpClient())
            {
                // Create the full url
                string url = $"http://battlelog.battlefield.com/{ game.ToString().ToLower() }/launcher/mpleavegameserver/1/{ this._personaId }/{ gameId }";

                // Create the data that should be posted to the given url
                FormUrlEncodedContent data = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "post-check-sum", this._postChecksum },
                });

                // Remove reservation
                _ = await client.PostAsync(url, data);
            }
        }

        /// <summary>
        /// A simple helper method that returns a new HttpClient with our handler
        /// </summary>
        private HttpClient CreateHttpClient()
        {
            return new HttpClient(this._handler, false);
        }

        /// <summary>
        /// Checks if there is a session and throws if not
        /// </summary>
        private void CheckSession()
        {
            // Check that a session was created
            if (this._hasSession == false)
            {
                throw new InvalidOperationException("There is no session yet. Please create a session first!");
            }
        }

        /// <summary>
        /// Checks if the specified game is currently supported
        /// </summary>
        /// <param name="game">Game to check</param>
        private void CheckGame(SupportedBattlelogGame game)
        {
            if (Enum.IsDefined(typeof(SupportedBattlelogGame), game) == false)
            {
                throw new NotSupportedException($"The game { game } ({ (int)game }) is currently not supported!");
            }
        }
    }

    /// <summary>
	/// A helper class to interact with battlelog
	/// </summary>
	public class BattlelogHelper
    {
        /// <summary>
        /// Retrieves the game and gameId from a server guid
        /// </summary>
        /// <param name="guid">Guid of server</param>
        public static async Task<(SupportedBattlelogGame Game, long GameId)> GetGameId(string guid)
        {
            SupportedBattlelogGame game;
            long gameId;

            using (HttpClientHandler handler = new HttpClientHandler())
            {
                // Don't allow automatic redirects
                // Redirects that may happen (for example for a guid which is not from bf4) won't have the needed query string, which we need to get a json response
                handler.AllowAutoRedirect = false;

                using (HttpClient client = new HttpClient(handler))
                {
                    // Set the baseAddress so that we can work with relative urls (actually used for the redirect that may happen later)
                    client.BaseAddress = new Uri("http://battlelog.battlefield.com/");

                    // Create the url
                    // We start with bf4, because otherwise it would may redirect to http://battlelog.battlefield.com/WARSAW/...
                    // This is basically wrong (why the hell they redirect to that url when it's not existing)
                    // All other redirects seem to be fine
                    string url = $"bf4/servers/show/pc/{ guid }/?json=1";

                    // Issue the request
                    HttpResponseMessage response = await client.GetAsync(url);

                    // Check if the response wants to redirect
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        // Rebuild the url from location header and query string
                        url = response.Headers.Location + response.RequestMessage.RequestUri.Query;
                        // Issue the new request
                        response = await client.GetAsync(url);
                    }

                    // Read the response body
                    string content = await response.Content.ReadAsStringAsync();

                    // Read the game and gameId from response body and check that they are valid
                    JToken serverInfo = (JsonConvert.DeserializeObject(content) as JObject)?["message"]?["SERVER_INFO"];

                    if (Enum.TryParse((serverInfo?["game"] as JValue)?.Value?.ToString(), out game) == false)
                    {
                        throw new Exception("The game could not be found!");
                    }

                    if (long.TryParse((serverInfo?["gameId"] as JValue)?.Value?.ToString(), out gameId) == false)
                    {
                        throw new Exception("The gameId could not be found!");
                    }
                }
            }

            return (game, gameId);
        }
    }
}