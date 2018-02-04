using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading;
using System.Security.Cryptography;

namespace simPrinter3D
{

    public enum HttpVerb
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    class Program
    {

        static void Main(string[] args)
        {
            //Variable to use in place of zero index;
            const int frontIndex = 0;
            string HashOfRealObject = "";

            Console.WriteLine("Hello World!");                                      //Entry point


            //Retrieves token key from user acquired from marketplace
            //Fetches data from 'Main' args param
            string purchaseToken = SetTokenObject("AAF4D309-40F6-451E-BAE7-A0F174D98CEE"); // The Consumable Token
            string userIDToken = SetTokenObject("2a7043cd-cad3-455d-875a-ef850ca1fdbd");

            var Approval = TokenAuthentication(purchaseToken, userIDToken).Result;
            Guid singleUseAuthToken = Approval.auth_token;
            HashOfRealObject = Approval.object_hash;

            //This Auth token will be used to communicate with the 


            Console.WriteLine("Token Purchase Validated");
            Console.WriteLine("Getting Ready to print ...");
            Thread.Sleep(2000); // For dramatic effect

            //get the file from the server
            Stream fileToPrint = ReadyToPrint(singleUseAuthToken).Result;
            //Verify that the response was in fact the file to print
            Console.WriteLine("Confirming Object...");
            string resultHash = "";
            using (MemoryStream rea = new MemoryStream())
            {
                fileToPrint.CopyTo(rea);
                SHA256Managed sha = new SHA256Managed();
                resultHash = BitConverter.ToString(sha.ComputeHash(rea.ToArray())).Replace("-", String.Empty);
            }
            if (resultHash == HashOfRealObject)
            {
                Console.WriteLine("Correct Object Received!");
                Console.WriteLine("Printing Object...");
                Thread.Sleep(3000);
            }
            else
            {
                Console.WriteLine("Token is either expired or corrupted data is present.");

            }
            //Do Something here that "prints" (probably save on computer)
            //This is a simulation after all. :D


        }
        //returns string that is passed in
        static string SetTokenObject(string e_token)
        {
            return e_token;
        }


        /// <summary>
        /// Authenticates the user purchase token with PAPI
        /// Returns true if token is valid
        ///         false else token is invalid
        /// </summary>
        /// <param name="e_token"></param>
        /// <returns></returns>
        static async Task<AuthTokenJson> TokenAuthentication(string p_token, string userToken)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync($"https://localhost:44358/printerapi/AuthenticatePrintToken?consumable_license={p_token}&user_id={userToken}");

            return JsonConvert.DeserializeObject<AuthTokenJson>(await response.Content.ReadAsStringAsync());

        }

        public static async Task<Guid> TokenSender(string p_token, string userToken)
        {
            HttpClient client = new HttpClient();


            //REMEMBER TO CHANGE THE API URL
            HttpResponseMessage response = await client.PostAsJsonAsync("api/authenticate", p_token + userToken);
            response.EnsureSuccessStatusCode();
            Guid validated = await response.Content.ReadAsAsync<Guid>();

            return validated;
        }
        
        public static async Task<Stream> ReadyToPrint(Guid singleUseAuthToken)
        {
            HttpClient client = new HttpClient();
            //REMEMBER TO CHANGE THE API URL
            HttpResponseMessage response = await client.GetAsync($"https://localhost:44358/printerapi/AuthenticatedAndReadyToPrintRequest?auth_token_id={singleUseAuthToken}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync();
        }
        //singleUseReceive func

    }
    public class AuthTokenJson
    {
        public Guid auth_token { get; set; }
        public DateTime expiration { get; set; }
        public string object_hash { get; set; }
    }
}
