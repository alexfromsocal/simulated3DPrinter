using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Threading.Tasks;



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
          
           
            Console.WriteLine("Hello World!");                                      //Entry point


            //Retrieves token key from user acquired from marketplace
            //Fetches data from 'Main' args param
            string purchaseToken = SetTokenObject(args[frontIndex]);                //String used for verification
            string userIDToken = SetTokenObject(args[frontIndex+1]);

            Guid singleUseAuthToken = TokenAuthentication(purchaseToken, userIDToken).Result;
            

            
                Console.WriteLine("Token Purchase Validated");
                Object fileToPrint = ReadyToPrintEventHandler(singleUseAuthToken).Result;
                
                //Do Something here that "prints" (probably save on computer)
                //This is a simulation after all. :D
           
                Console.WriteLine("Invalid Purchase Token.");
                Console.WriteLine("Please run again.");
            
#if DEBUG
            Console.ReadLine();
#endif

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
        static async Task<Guid> TokenAuthentication(string p_token, string userToken)
        {
            Console.WriteLine("In TokenAuth \n");
            Guid tokenSent = new Guid();

            try {

                tokenSent = await TokenSender(p_token, userToken);
            }


            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return tokenSent;
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


        /// <summary>
        /// Signals to PAPI that the 3D printer has received PAPI handshake validation
        /// Posts a request to PAPI to begin printing process.
        /// </summary>
        public static async Task<Object> ReadyToPrintEventHandler(Guid singleUseAuthToken)
        {
            Object fileToPrint = new Object();
            try
            {

               fileToPrint = await ReadyToPrint(singleUseAuthToken);
                
            }


            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return fileToPrint;
        }

        public static async Task<Object> ReadyToPrint(Guid singleUseAuthToken)
        {
            HttpClient client = new HttpClient();


            //REMEMBER TO CHANGE THE API URL
            HttpResponseMessage response = await client.PostAsJsonAsync("api/recordledger", singleUseAuthToken);
            response.EnsureSuccessStatusCode();
            Object fileToPrint = await response.Content.ReadAsAsync<Guid>();
            return fileToPrint;

        }




        //singleUseReceive func
        
    }
}
