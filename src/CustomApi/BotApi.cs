﻿
namespace UB3RB0T
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>
    /// Internal, private API for UB3R-B0T.
    /// TODO: Try to generalize this, notably allow for a dynamic customizeable response so others can tailor for their needs.
    /// </summary>
    public class BotApi
    {
        private Uri apiEndpoint;
        private string apiKey;
        private BotType botType;

        public BotApi(Uri endpoint, string key, BotType botType)
        {
            this.apiEndpoint = endpoint;
            this.apiKey = key;
            this.botType = botType;
        }

        public async Task<string[]> IssueRequestAsync(BotMessageData messageData, string query)
        {
            string[] responses = new string[] { };

            string requestUrl = string.Format("{0}?apikey={1}&nick={2}&host={3}&server={4}&channel={5}&query={6}",
                this.apiEndpoint,
                this.apiKey,
                WebUtility.UrlEncode(messageData.UserName),
                WebUtility.UrlEncode(messageData.UserHost),
                messageData.Server,
                WebUtility.UrlEncode(messageData.Channel),
                WebUtility.UrlEncode(query));

            var req = WebRequest.Create(requestUrl);

            WebResponse webResponse = null;
            try
            {
                webResponse = await req.GetResponseAsync();
            }
            catch (WebException)
            {
                //
            }

            if (webResponse != null)
            {
                Stream responseStream = webResponse.GetResponseStream();
                string responseData;
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    responseData = await reader.ReadToEndAsync();
                }

                BotApiResponse botResponse = null;

                try
                {
                    botResponse = JsonConvert.DeserializeObject<BotApiResponse>(responseData);
                }
                catch (Exception)
                {
                }

                if (botResponse != null)
                {
                    responses = botResponse.Msgs.Length > 0 ? botResponse.Msgs : new string[] { botResponse.Msg };

                    if (this.botType == BotType.Discord)
                    {
                        string response = string.Join("\n", responses);

                        // Extra processing for figlet/cowsay on Discord
                        if (query.StartsWith("cowsay", StringComparison.OrdinalIgnoreCase) || query.StartsWith("figlet", StringComparison.OrdinalIgnoreCase))
                        {
                            // use a non printable character to force preceeding whitespace to display correctly
                            response = "```" + (char)1 + response + "```";
                        }

                        responses = new string[] { response };
                    }
                }
            }

            return responses;
        }
    }
}