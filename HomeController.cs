using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VimeoDotNet;
using VimeoDotNet.Net;
using System.Runtime.InteropServices;

namespace VimeoBatchReplace.Controllers
{
    public class HomeController : Controller
    {
        string accesstoken = "";

        public async Task<ActionResult> Index()
        {
            return View();
        }


        public async Task<ActionResult> Upload(HttpPostedFile[] file)
        {
            // Checks if the Use Showcase checkbox is clicked or not
            string[] useShowcaseArray = Request.Form.GetValues("useShowcase");
            bool useShowcase;
            if (useShowcaseArray[0] == "true")
            {
                useShowcase = true;
            }
            else
            {
                useShowcase = false;
            }
            string uploadStatus = "";
            try
            {
                // If useShowcase is true, replace each video in the showcase
                if (useShowcase)
                {
                    string[] requestShowcase = Request.Form.GetValues("showcase");
                    long showcaseID = long.Parse(requestShowcase[0]);

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    accesstoken = Request.Form["token"];

                    VimeoClient vimeoClient = new VimeoClient(accesstoken);

                   var authcheck = await vimeoClient.GetAccountInformationAsync();
                   var album = await vimeoClient.GetAlbumAsync(authcheck.Id, showcaseID);
                   var videosInShowcase = await vimeoClient.GetAlbumVideosAsync(authcheck.Id, showcaseID, null, null, "default");                  
                   var newList = videosInShowcase.Data;
                    // If number of videos in showcase and number of videos being uploaded are equal
                    if (newList.Count == Request.Files.Count)
                    {
                        for (int i = 0; i < newList.Count;)
                        {
                            char[] uploadChar = newList[i].StandardVideoLink.ToCharArray(33, 9);
                            string addressSTR = new string(uploadChar);

                            HttpPostedFileBase requestedFile = Request.Files[i];

                            IUploadRequest uploadRequest = new UploadRequest();
                            BinaryContent binaryContent = new BinaryContent(requestedFile.InputStream, requestedFile.ContentType);
                            int chunkSize = 0;
                            int contentLength = requestedFile.ContentLength;
                            int temp1 = contentLength / 1024;
                            if (temp1 > 1)
                            {
                                chunkSize = temp1 / 1024;
                                chunkSize /= 10;
                                chunkSize *= 1048576;
                            }
                            else
                            {
                                chunkSize = 1048576;
                            }
                            long replaceAddress = long.Parse(addressSTR);
                            uploadRequest = await vimeoClient.UploadEntireFileAsync(binaryContent, chunkSize, replaceAddress);
                            uploadStatus = "Upload Successful";
                            i++;
                        }
                    }
                    else
                    {
                        // Throws an error if the number of uploaded videos is not equal tot eh number of videos in the showcase
                        uploadStatus = "Upload Failed: Number of uploaded videos not equal to number of videos in showcase.";
                        ViewBag.uploadStatus = uploadStatus;
                        return View();
                    }
                }
                else
                {
                    // If useShowcase is false, replaces the videos that have been added
                    string[] addresses = Request.Form.GetValues("fname");
                    int i = 0;
                    foreach (string fileName in Request.Files)
                    {
                        HttpPostedFileBase requestedFile = Request.Files[i];

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        accesstoken = Request.Form["token"];

                        VimeoClient vimeoClient = new VimeoClient(accesstoken);
                        var authcheck = await vimeoClient.GetAccountInformationAsync();

                        // If the Vimeo authentication check passes
                        if (authcheck.Name != null)
                        {
                            IUploadRequest uploadRequest = new UploadRequest();
                            BinaryContent binaryContent = new BinaryContent(requestedFile.InputStream, requestedFile.ContentType);
                            int chunkSize = 0;
                            int contentLength = requestedFile.ContentLength;
                            int temp1 = contentLength / 1024;
                            if (temp1 > 1)
                            {
                                chunkSize = temp1 / 1024;
                                chunkSize /= 10;
                                chunkSize *= 1048576;
                            }
                            else
                            {
                                chunkSize = 1048576;
                            }
                            long replaceAddress = long.Parse(addresses[i]);
                            i++;
                            uploadRequest = await vimeoClient.UploadEntireFileAsync(binaryContent, chunkSize, replaceAddress);
                            uploadStatus = "Upload Successful";
                        }

                    }
                }   
            }
            catch (Exception er)
            {
                uploadStatus = "not uploaded:" + er.Message;
                if (er.InnerException != null)
                {
                    uploadStatus += er.InnerException.Message;
                }
            }

            ViewBag.UploadStatus = uploadStatus;

            return View();

        }


        public ActionResult About()
        {
            ViewBag.Message = "Your description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}