//using SpotifyAPI.Web.Auth;
//using SpotifyAPI.Web.Enums;
//using SpotifyAPI.Web.Models;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SpotifyDownloader
//{
//    public class Authenticator
//    {
//        /* Authenticates with a timeout of 10 seconds. */
//        public async Task<Token> Authenticate(string clientId, string secretId)
//        {
//            AuthorizationCodeAuth auth = new AuthorizationCodeAuth(
//                clientId, secretId,
//                "http://localhost:4002", "http://localhost:4002",
//                Scope.UserReadRecentlyPlayed
//            );
//            Token token = null;

//            SemaphoreSlim signal = new SemaphoreSlim(0, 1);
//            auth.AuthReceived += async (sender, payload) =>
//            {
//                token = await auth.ExchangeCode(payload.Code);
//                signal.Release();
//            };
//            auth.Start();
//            auth.OpenBrowser();

//            await signal.WaitAsync(15000);
//            auth.Stop();

//            return token;
//        }

//    }
//}
