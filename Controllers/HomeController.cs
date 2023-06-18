using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System;
using System.Collections.Generic;
using Jose;
namespace PayByBankAPI.Controllers
{
    public class HomeController : Controller
    {

        //Replace below KID and SECRET with the values provided by AMEX
        private static string SIGNING_KID = "id_S7VVyHN1";
        private static string SIGNING_SECRET = "rgz7ZwoauA1bMM4JHBHZzqZiEQtN6eE0";
        private static string ENCRYPT_KID = "id_jenwOllw";
        private static string ENCRYPT_SECRET = "mGReEhM50cTtSHolBAIurnsC8MIguOaD";
       
     
        public ActionResult Index()
        {
            #region PayLoad  Response  get the JWS_data

            var paylod =
                "eyJraWQiOiJpZF9TN1ZWeUhOMSIsImFsZyI6IkhTMjU2In0.eyJhbW91bnQiOiIxLjMwIiwiY3VycmVuY3kiOiJHQlAiLCJ0aW1lc3RhbXAiOjE2ODcwNjY5MjU5ODUsIm9yZGVyX2lkIjoiNjM1NTI5MTEiLCJjb3JyZWxhdGlvbl9pZCI6InIyNzgyMmRkNmVmZjRjN2RhYjRlZTY1MDAzNWU5MWFjIiwicmVxdWVzdF9pZCI6InIyNzgyMmRkNmVmZjRjN2RhYjRlZTY1MDAzNWU5MWFjIiwic2Vzc2lvbl9pZCI6IjIyNGJiOTE0ODNmZDRiYTBiYzMzMDFmMTZjOGU1MDM0IiwidHJhbnNhY3Rpb25faWQiOiJhNjU3ZmUwOTdjMDk0ZjAxOTlkZWMzYTE5ODBhMTcwZSIsInBheW1lbnRfc3RhdHVzIjoiQWNjZXB0ZWRTZXR0bGVtZW50Q29tcGxldGVkIiwiYmFua19yZWZlcmVuY2VfaWQiOiJQV0JUMTY4NzA2Njg5NjU4MiJ9.OA7vUiFVFiTTc5uTsIBiD-XEQrLE2pxaR3veyQ2zxGs";
            var paylod2 = getJWSPayload(paylod);


            #endregion

            #region Get Status 

            var statusHeader = getStatusAuthHaeder();
            ViewBag.AuthHeader = statusHeader;
            #endregion

            string addressPayload = "{\n" +
                                    "\"address_line\": [\n" +
                                    "\"Flat 7\",\n" +
                                    "\"Acacia Lodge\"\n" +
                                    "],\n" +
                                    "\"street_name\": \"Acacia Avenue\",\n" +
                                    "\"building_number\": \"27\",\n" +
                                    "\"post_code\": \"GU31 2ZZ\",\n" +
                                    "\"town_name\": \"Sparsholt\",\n" +
                                    "\"country_subdivision\": \"Wessex\",\n" +
                                    "\"country\": \"GB\"\n" +
                                    "}\n";
            // Create JWE address payload
            string jweEncryptedPayload = createJWEPayload(addressPayload);
            Console.WriteLine("jweEncryptedPayload::" + jweEncryptedPayload);
            // Decrypt JWE address payload
            string jweDecryptedPayload = getDecryptJWEPayload(jweEncryptedPayload);
            Console.WriteLine("jweDecryptedPayload::" + jweDecryptedPayload);
            // Create JWS payload for order details
            // include address jweEncryptedPayload
            string payload = "{ \n" +
                             "\"order_id\": \"63552911\",\n" +
                             "\"correlation_id\": \"r27822dd6eff4c7dab4ee650035e91ac\",\n" +
                             " \"amount\": \"1.40\",\n" +
                             " \"currency\": \"GBP\",\n" +
                             " \"timestamp\": 1548903423411,\n" +
                             " \"payment_context_code\": \"EcommerceGoods\",\n" +
                                " \"customer_id\": \"rogert12322\",\n" +
                                " \"delivery_address\": \"" + jweEncryptedPayload + "\",\n" +
                                " \"redirect_url\":\"https://merchanthosted.redirecturl.com\",\n" +
                                " \"session_id\": \"224bb91483fd4ba0bc3301f16c8e5034\"\n" +
                                "}";
            // Pass this jwsPayload in order details
            string jwsPayload = createJWSPayload(payload);
            Console.WriteLine("JWS Signed Payload::" + jwsPayload);
            ViewBag.JWSData = jwsPayload;
            // Verify Singature and get Json Payload
            string jsonPayload = getJWSPayload(jwsPayload);
            Console.WriteLine("Json Payload::" + jsonPayload);
            ViewBag.Title = "Home Page";
            ViewBag.jsonPayloadTitle = "Pay By Bank";
            //  ViewBag.jwsPayload = jwsPayload;

           
         
            return View();
        }
        /**
      * Create JWE Payload
      * @param requestPayload
      * @return
*/
        public static string createJWEPayload(string requestPayload)
        {
            var headers = new Dictionary<string, object>()
            {
                { "kid", ENCRYPT_KID },
                { "alg", JweAlgorithm.A256KW},
                {"enc",JweEncryption.A256CBC_HS512}
            };
            var jweEnc = "";
            byte[] secret = System.Text.Encoding.UTF8.GetBytes(ENCRYPT_SECRET);
            string token = Jose.JWT.Encode(requestPayload, secret, JweAlgorithm.A256KW,JweEncryption.A128CBC_HS256, null, headers);
            return token;
        }
        /**
        * Decrypt JWE payload
        *
        * @param encryptedPayload
        * @return
*/
        public static string getDecryptJWEPayload(string encryptedPayload)
        {
            byte[] sharedSecretBytes = System.Text.Encoding.UTF8.GetBytes(ENCRYPT_SECRET);
            string decryptedPayload = Jose.JWT.Decode(encryptedPayload, sharedSecretBytes);
            return decryptedPayload;
        }
        /**
        * Create JWS Payload
        * @param requestPayload
        * @return
*/
        public static string createJWSPayload(string requestPayload)
        {
            var headers = new Dictionary<string, object>()
            {
                { "kid", SIGNING_KID },
                { "alg", JwsAlgorithm.HS256}
            };
            byte[] secret = System.Text.Encoding.UTF8.GetBytes(SIGNING_SECRET);
            string token = Jose.JWT.Encode(requestPayload, secret, JwsAlgorithm.HS256, headers);
            return token;
        }
        /**
        * Verify signature and return json payload
        *
        * @param jwsPayload
        * @return
        * @throws Exception
*/
        public static string getJWSPayload(string jwsPayload)
        {
            try
            {
                byte[] sharedSecretBytes = System.Text.Encoding.UTF8.GetBytes(SIGNING_SECRET);
                string payload = Jose.JWT.Decode(jwsPayload, sharedSecretBytes);
                return payload;
            }
            catch
            {
                return null;
            }
        }

        public static string getStatusAuthHaeder()
        {
            var payload = "{ \n" +
                          "\"merchant_id\": \"amex_test_5e3197d87d434bb0b25b93f82f0f567c\",\n" +
                          "\"order_id\": \"63552911\",\n" +
                          "\"country_code\": \"GB\",\n" +
                          "\"timestamp\": \"1687066925985\",\n" +
                          "}\n";
            string token = createJWSPayload(payload);
            return token;

        }
        //JSON Web Signature that’s represented in Compact
        //    Serialized form as {x.y.z
        //}
        //Where x = header, y = base64
        //encoded payload, c = Signature.After validating signature,
        //    the payload comprises of the following elements(Signature
        //    steps to be provided as needed )
        //{
        //    "merchant_id":
        //    "amex_test_5e3197d87d434bb0b25b93f82f0f567c",
        //    "order_id": "63552911",
        //    "country_code": "GB",
        //    "timestamp": 1548903423411
        //}

    }
}
