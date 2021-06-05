using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace GoogleAPIService.Controllers
{
    public class OAuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult CallBack(string code, string error, string state)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                this.GetToken(code);

            }



            var TokenFile = "D:\\TokenFile.json";
            var token = JObject.Parse(System.IO.File.ReadAllText(TokenFile));

            if (token != null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Error in Getting Token");
            }
            //return RedirectToAction("Privacy", "Home");

        }


        private ActionResult GetToken(string code)
        {
            var TokenFile = "D:\\TokenFile.json";
            //var CredFile = "C:\\Users\\Owner\\source\\repos\\GSuiteAPITest\\Files\\ClientCred.json";
            //var credential = JObject.Parse(System.IO.File.ReadAllText(CredFile));

            RestClient restClient = new RestClient();
            RestRequest restRequest = new RestRequest();

            restRequest.AddQueryParameter("client_id", "232263937112-dvr848pqbhi33vs6vo7nkkmp2932siai.apps.googleusercontent.com");
            restRequest.AddQueryParameter("client_secret", "k_VD9FGOr4HwrAYQA6K7Y8Ua");
            restRequest.AddQueryParameter("code", code);
            restRequest.AddQueryParameter("grant_type", "authorization_code");
            restRequest.AddQueryParameter("redirect_uri", "https://localhost:44357/OAuth/CallBack");

            restClient.BaseUrl = new System.Uri("https://oauth2.googleapis.com/token");
            var response = restClient.Post(restRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                System.IO.File.WriteAllText(TokenFile, response.Content);
                return RedirectToAction("Index", "Home");
            }

            return View("Error in Getting Token");
        }


        public ActionResult GetAccessTokenOnly()
        {
            try
            {
                var TokenFile = "D:\\TokenFile.json";
                var CredFile = "D:\\client_secret.json";
                var credential = JObject.Parse(System.IO.File.ReadAllText(CredFile));
                var token = JObject.Parse(System.IO.File.ReadAllText(TokenFile));

                if (token != null)
                {
                    RestClient restClient = new RestClient();
                    RestRequest restRequest = new RestRequest();

                    restRequest.AddQueryParameter("client_id", credential["client_id"].ToString());
                    restRequest.AddQueryParameter("client_secret", credential["client_secret"].ToString());
                    restRequest.AddQueryParameter("grant_type", "refresh_token");
                    restRequest.AddQueryParameter("refresh_token", token["refresh_token"].ToString());

                    restClient.BaseUrl = new System.Uri("https://oauth2.googleapis.com/token");
                    var response = restClient.Post(restRequest);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        JObject newToken = JObject.Parse(response.Content);
                        newToken["refresh_token"] = token["refresh_token"].ToString();
                        System.IO.File.WriteAllText(TokenFile, newToken.ToString());
                        return RedirectToAction("CreateUser", "GApiCaller");
                    }
                    else
                    {
                        return RedirectToAction("OuthRedirect", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("OuthRedirect", "Home");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("OuthRedirect", "Home");
            }


        }



    }
}
