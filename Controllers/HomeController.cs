using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GoogleAPIService.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using RestSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using Syroot.Windows.IO;
using Microsoft.AspNetCore.Hosting;


// Syroot.Windows.IO.KnownFolders should be installed from Nuget Package.

namespace GoogleAPIService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            return View();
            //var client_ID = "232263937112-dvr848pqbhi33vs6vo7nkkmp2932siai.apps.googleusercontent.com";

            //try
            //{
            //    var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
            //                        "scope=https://www.googleapis.com/auth/drive.appdata+https://www.googleapis.com/auth/drive.file&" +
            //                        "access_type=offline&" +
            //                        "include_granted_scopes=true&" +
            //                        "response_type=code&" +
            //                        "state=heyThere&" +
            //                        "redirect_uri=https://localhost:44357/OAuth/CallBack&" +
            //                        "client_id=" + client_ID;

            //    return Redirect(redirectUrl);
            //    //return Redirect("https://google.com");
            //}
            //catch (Exception ex)
            //{
            //    return Redirect("https://google.com");
            //}
            ////return Redirect("https://google.com");
            ////return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //[HttpPost]
        public IActionResult UploadFile( string File,            
                                    string FileName,
                                    string MimeType
                                    )
        {
            return Redirect("https://google.com");
            //try {
            //    //OuthRedirect();
            //    return RedirectToAction("OuthRedirect", "Home");

            //    //GoogleDriveFilesRepository.FileUpload(File,FileName,MimeType);

            //}
            //catch (Exception ex)
            //{
            //    return RedirectToAction("OuthRedirect", "Home");

            //    //return Json(new { success = false, responseText = ex.Message });

            //}
            //return Json(new { success = true, responseText = "Payment is successful" });

        }


        
    

        public ActionResult OuthRedirect()
        {
            string webRootPath = _webHostEnvironment.WebRootPath;
            string contentRootPath = _webHostEnvironment.ContentRootPath;

            //var CredFile = "D:\\client_secret.json";
            var CredFile = contentRootPath + "\\GDriveJsonFiles\\client_secret.json";

            JObject credential = JObject.Parse(System.IO.File.ReadAllText(CredFile));

            var client_ID = credential["client_id"];


            //redirecturi



            //var client_ID = "232263937112-dvr848pqbhi33vs6vo7nkkmp2932siai.apps.googleusercontent.com";

            try
            {
                var redirectUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                                    "scope=https://www.googleapis.com/auth/drive.appdata+https://www.googleapis.com/auth/drive.file&" +
                                    "access_type=offline&prompt=consent&" +
                                    "include_granted_scopes=true&" +
                                    "response_type=code&" +
                                    "state=heyThere&" +
                                    "redirect_uri=https://localhost:44357/OAuth/CallBack&" +
                                    "client_id=" + client_ID;

                return Redirect(redirectUrl);
              
            }
            catch (Exception ex)
            {
                return Redirect("https://google.com");
            }
        }


        [HttpPost]
        public IActionResult UploadGDrive(FileObject model)
        {
           
            var img = model.MyImage;
            var imgCaption = model.ImageCaption;

            var fileName = Path.GetFileName(model.MyImage.FileName);
           

            using (var ms = new MemoryStream())
            {
                img.CopyTo(ms);
                var fileBytes = ms.ToArray();
    


                try
                {
                    string webRootPath = _webHostEnvironment.WebRootPath;
                    string contentRootPath = _webHostEnvironment.ContentRootPath;

                    //-------------------------project root-------------------------------- 

                    // Please set the tokenFile & uploadFileResponse  path inside your application directory.
                    // Not like the following set up. 
                    //var TokenFile = "D:\\TokenFile.json";
                    //var uploadFileResponse = "D:\\UploadResponse.json";

                    var TokenFile = contentRootPath + "\\GDriveJsonFiles\\TokenFile.json";
                    var uploadFileResponse = contentRootPath + "\\GDriveJsonFiles\\UploadResponse.json";



                    var token = JObject.Parse(System.IO.File.ReadAllText(TokenFile));
                    if (token != null)
                    {
                        

                        FileMeta fm = new FileMeta();

                        fm.name = fileName;

                        

                        var jModel = JsonConvert.SerializeObject(fm, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        });

                        //===================== Preparing content the for Request Body for Multipart Upload API======================
                       
                        var boundary = "xxxxxxxxxx";
                        var data = "--" + boundary + "\r\n";
                        data += "Content-Disposition: form-data; name=\"metadata\"\r\n";
                        data += "Content-Type: application/json; charset=UTF-8\r\n\r\n";
                        data += jModel + "\r\n";
                        data += "--" + boundary + "\r\n";
                        data += "Content-Disposition: form-data; name=\"file\"\r\n\r\n";

                        byte[] dataBytes = Encoding.ASCII.GetBytes(data);

                        byte[] lastByte = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");


                        byte[] ret = new byte[dataBytes.Length + fileBytes.Length + lastByte.Length];
                        Buffer.BlockCopy(dataBytes, 0, ret, 0, dataBytes.Length);
                        Buffer.BlockCopy(fileBytes, 0, ret, dataBytes.Length, fileBytes.Length);
                        Buffer.BlockCopy(lastByte, 0, ret, dataBytes.Length + fileBytes.Length,
                                         lastByte.Length);
                        

                        var bodyByteLength = dataBytes.Length + fileBytes.Length + lastByte.Length;

                        //================Content Preparation is finished.======================================================

                        RestClient restClient = new RestClient();
                        RestRequest restRequest = new RestRequest();

                        restRequest.AddQueryParameter("uploadType", "multipart");
                        restRequest.AddQueryParameter("key", "AIzaSyCSfmYx3A2793kdn1rjvxklhCSaDLoSLkY");
                        restRequest.AddHeader("Authorization", "Bearer " + token["access_token"]);
                        restRequest.AddHeader("Accecpt", "application/json");
                        restRequest.AddHeader("Content-Type", "multipart/related;boundary=xxxxxxxxxx");
                      

                        restRequest.AddHeader("Content-Length", bodyByteLength.ToString());

                        restRequest.AddParameter("application/json", ret, ParameterType.RequestBody);

                   

                        restClient.BaseUrl = new System.Uri("https://www.googleapis.com/upload/drive/v3/files");
                        var response = restClient.Post(restRequest);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            // upload response willl be saved in Database. 
                            System.IO.File.WriteAllText(uploadFileResponse, response.Content);
                            //.......DB operation....
                            return RedirectToAction("Index", "Home");

                        }
                        else
                        {
                            return RedirectToAction("GetAccessTokenOnly", "OAuth");
                        }
                    }
                    else
                    {
                        return RedirectToAction("GetAccessTokenOnly", "OAuth");
                    }
                }
                catch (Exception ex)
                {
                    return RedirectToAction("GetAccessTokenOnly", "OAuth");
                }
            }

        
        }


        public IActionResult DownloadFileGDrive()
        {

            try
            {

                //Here the file will be saved in Users' Downloads Folder.
                //No option will be given to choose the download folder.
                //By default System's Downloads folder will be used.

                string downloadsPath = KnownFolders.Downloads.DefaultPath.ToString();
               

                // Please set the token file path inside your application directory.
                // Not like the following set up. 
                var TokenFile = "D:\\TokenFile.json";

                // The file id & fileName will come from database..........................
                // Here it's a sample id & Name.
                string fileId = "1Cs7GhvRoe1U2uTOh-lj1rgnb67Z3QLYS";
                string fileName = "spiral_image2.png";


                var token = JObject.Parse(System.IO.File.ReadAllText(TokenFile));
                if (token != null)
                {

          


                    RestClient restClient = new RestClient();
                    RestRequest restRequest = new RestRequest();

                    
                    restRequest.AddQueryParameter("key", "AIzaSyCSfmYx3A2793kdn1rjvxklhCSaDLoSLkY");
                    restRequest.AddQueryParameter("alt", "media");

                    restRequest.AddHeader("Authorization", "Bearer " + token["access_token"]);
                    restRequest.AddHeader("Accecpt", "application/json");
               




                    restClient.BaseUrl = new System.Uri("https://www.googleapis.com/drive/v3/files/"+fileId);
                    var response = restClient.Get(restRequest);
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        String FolderPath = downloadsPath;
                        String FilePath = Path.Combine(FolderPath,fileName);
                        byte[] myByteArray = response.RawBytes;
                        MemoryStream stream = new MemoryStream();
                        stream.Write(myByteArray, 0, myByteArray.Length);
                        SaveStream(stream, FilePath);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return RedirectToAction("GetAccessTokenOnly", "OAuth");
                    }
                }
                else
                {
                    return RedirectToAction("GetAccessTokenOnly", "OAuth");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("GetAccessTokenOnly", "OAuth");
            }
        }


        private static void SaveStream(MemoryStream stream, string FilePath)
        {
            using (System.IO.FileStream file = new FileStream(FilePath, FileMode.Create, FileAccess.ReadWrite))
            {
                stream.WriteTo(file);
            }
        }

        public class FileMeta
        {
            public string name;
        }

        public class FileMedia
        {
            public string name;
            public string mimeType;
            public string title;
        }
    }
}
