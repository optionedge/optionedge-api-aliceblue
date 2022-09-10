using OptionEdge.API.AliceBlue.Records;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OptionEdge.API.AliceBlue
{
    internal class AliceBlueAuthenticator : AuthenticatorBase
    {
        readonly string _userId;
        readonly string _apiKey;
        readonly string _baseUrl;
        readonly string _encryptionKeyEndpoint;
        readonly string _sessionIdEndpoint;
        readonly bool _enableLogging;

        Action<string> _onAccessTokenGenerated;
        Func<string> _cachedAccessTokenProvider;
        public AliceBlueAuthenticator(string userId, string apiKey, string baseUrl, string encryptionKeyEndpoint, string sessionIdEndpoint, bool enableLogging = false, Action<string> onAccessTokenGenerated = null, Func<string> cachedAccessTokenProvider = null) : base("")
        {
            _userId = userId.ToUpper();
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            _encryptionKeyEndpoint = encryptionKeyEndpoint;
            _sessionIdEndpoint = sessionIdEndpoint;
            _enableLogging = enableLogging;
            _onAccessTokenGenerated = onAccessTokenGenerated;
            _cachedAccessTokenProvider = cachedAccessTokenProvider;
        }

        protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
        {
            var cachedToken = string.Empty;

            if (string.IsNullOrEmpty(this.Token) && _cachedAccessTokenProvider != null)
            {
                cachedToken = _cachedAccessTokenProvider?.Invoke();
                if (!string.IsNullOrEmpty(cachedToken)) this.Token = cachedToken;
            }

            if (string.IsNullOrEmpty(Token))
            {
                this.Token = await GetAccessToken();
                _onAccessTokenGenerated?.Invoke(this.Token);
            }
            else
                this.Token = cachedToken;

            var bearer = $"Bearer {_userId} {Token}";
            return new HeaderParameter(KnownHeaders.Authorization, bearer);
        }
        async Task<string> GetAccessToken()
        {
            if (_enableLogging)
                Utils.LogMessage("Getting the Access Token (Session Id).");

            var options = new RestClientOptions(_baseUrl);
            var restClient = new RestClient(options);

            var request = new RestRequest(_encryptionKeyEndpoint);

            var encryptionKeyParams = new EncryptionKeyParams
            {
                UserId = _userId,
            };

            request.AddStringBody(JsonConvert.SerializeObject(encryptionKeyParams), ContentType.Json);

            if (_enableLogging)
                Utils.LogMessage($"Calling encryption key endpoint: {_encryptionKeyEndpoint}");

            var encryptionKeyResponse = await restClient.PostAsync<EncryptionKeyResult>(request);

            if (_enableLogging)
                Utils.LogMessage($"Encryption Key Result. Status: {encryptionKeyResponse.Status}-{encryptionKeyResponse.ErrorMessage}");

            if (encryptionKeyResponse.Status == Constants.API_RESPONSE_STATUS_Not_OK)
                throw new Exception($"Error getting encryption key. Status: {encryptionKeyResponse.Status}, Error Message: {encryptionKeyResponse.ErrorMessage}");

            var userData = Utils.GetSHA256($"{_userId}{_apiKey}{encryptionKeyResponse.EncryptionKey}");

            request = new RestRequest(_sessionIdEndpoint);

            var createSessionIdParams = new WebsocketSessionIdParams
            {
                UserId = _userId,
                UserData = userData
            };
            request.AddStringBody(JsonConvert.SerializeObject(createSessionIdParams), ContentType.Json);

            if (_enableLogging)
                Utils.LogMessage($"Calling create session endpoint: {_sessionIdEndpoint}");

            var createSessonResponse = await restClient.PostAsync<CreateSessonDetailResult>(request);
            if (createSessonResponse.Status == Constants.API_RESPONSE_STATUS_Not_OK)
                if (_enableLogging)
                    Utils.LogMessage($"Error creating sesson. Status: {createSessonResponse.Status}, Error Message: {createSessonResponse.ErrorMessage}");
                else if (_enableLogging)
                    Utils.LogMessage($"Create Session Result. Status: {createSessonResponse.Status}-{createSessonResponse.ErrorMessage}");

            if (restClient != null) restClient.Dispose();

            return createSessonResponse?.SessionID;
        }
    }
}
