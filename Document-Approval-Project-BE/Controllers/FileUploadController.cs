using Document_Approval_Project_BE.Models;
using Newtonsoft.Json;
using Syncfusion.EJ2.PdfViewer;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.UI.WebControls;
using System.Xml.Linq;

namespace Document_Approval_Project_BE.Controllers
{
    [RoutePrefix("api/pdf-viewers")]
    public class FileUploadController : ApiController
    {
        private readonly ProjectDBContext db = new ProjectDBContext();
        private System.Web.HttpContext currentContext = System.Web.HttpContext.Current;

        [HttpPost]
        [Route("Load")]
        public IHttpActionResult Load([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            MemoryStream stream = new MemoryStream();
            object jsonResult = new object();

            if (jsonObject != null && (jsonObject.ContainsKey("document") || jsonObject.ContainsKey("documentId")))
            {
                if (jsonObject.ContainsKey("documenApprovaltId") && jsonObject.ContainsKey("fileId"))
                {
                    if (Guid.TryParse(jsonObject["documenApprovaltId"], out Guid documentApprovalId) &&
                    Guid.TryParse(jsonObject["fileId"], out Guid fileId))
                    {
                        var fileExist = db.DocumentApprovalFiles.FirstOrDefault(file =>
                            file.DocumentApprovalId == documentApprovalId && file.DocumentFileId == fileId);

                        if (fileExist != null)
                        {
                            string documentPath = GetDocumentPath(fileExist.FilePath);

                            if (!string.IsNullOrEmpty(documentPath) && File.Exists(documentPath))
                            {
                                byte[] bytes = File.ReadAllBytes(documentPath);
                                stream = new MemoryStream(bytes);
                            }
                            else
                            {
                                return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                            }
                        }
                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(jsonObject["document"]);
                        stream = new MemoryStream(bytes);
                    }
                    jsonResult = pdfviewer.Load(stream, jsonObject);
                    return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "Invalid parameters provided");
                }
            }
            else
            {
                return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
            }
        }


        [HttpPost]
        [Route("SaveDocument")]
        public IHttpActionResult SaveDocument([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();
            MemoryStream stream = new MemoryStream();

            if (jsonObject != null && jsonObject.ContainsKey("documentApprovalId") && jsonObject.ContainsKey("fileId"))
            {
                if (Guid.TryParse(jsonObject["documentApprovalId"], out Guid documentApprovalId) &&
                    Guid.TryParse(jsonObject["fileId"], out Guid fileId))
                {

                    var fileExist = db.DocumentApprovalFiles.FirstOrDefault(file =>
                        file.DocumentApprovalId == documentApprovalId && file.DocumentFileId == fileId);

                    if (fileExist != null)
                    {
                        string Filepath = Path.Combine(HostingEnvironment.MapPath("~/"),"Upload\\Files",fileExist.DocumentApprovalId.ToString());

                        if (!Directory.Exists(Filepath))
                        {
                            Directory.CreateDirectory(Filepath);
                        }

                        string documentPath = GetDocumentPath(fileExist.FilePath);


                        if (!string.IsNullOrEmpty(documentPath) && File.Exists(documentPath))
                        {
                            File.Delete(documentPath);


                            byte[] pdfBytes = Convert.FromBase64String(jsonObject["documentData"]);

                            File.WriteAllBytes(documentPath, pdfBytes);

                            if(File.Exists(documentPath))
                            {
                                using (FileStream docStream = new FileStream(documentPath, FileMode.Open, FileAccess.ReadWrite))
                                {
                                    PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                                    PdfLoadedForm form = loadedDocument.Form;
                                    PdfLoadedFormFieldCollection fieldCollection = form.Fields as PdfLoadedFormFieldCollection;

                                    foreach (PdfLoadedField field in fieldCollection)
                                    {
                                        if (field.Name != null && field is PdfLoadedTextBoxField)
                                        {
                                            string fieldName = field.Name;

                                            if (fieldName.StartsWith("[{RequestCode") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Request Code";
                                            }

                                            if (fieldName.StartsWith("[{Title") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Title";
                                            }

                                            if (fieldName.StartsWith("[{Signature") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Signature";
                                            }

                                            if (fieldName.StartsWith("[{SignedDate") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Date";
                                            }

                                            if (fieldName.StartsWith("[{SignerJobTitle") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Title";
                                            }

                                            if (fieldName.StartsWith("[{SignerName") && fieldName.EndsWith("}]"))
                                            {
                                                (field as PdfLoadedTextBoxField).Text = "Nghia";
                                            }
                                        }
                                    }

                                    loadedDocument.Save(docStream);
                                    loadedDocument.Close(true);
                                }

                            }
                        }
                        else
                        {
                            return Content(HttpStatusCode.NotFound, jsonObject["documentId"] + " is not found");
                        }
                    }

                    return Content(HttpStatusCode.OK, new
                    {
                        state = "true",
                        documentApprovalId= fileExist.DocumentApprovalId,
                        fileId = fileExist.DocumentFileId,
                    });
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "Invalid parameters provided");
                }
            }
            else
            {
                return Content(HttpStatusCode.NotFound, jsonObject["fileName"] + " is not found");
            }
        }

        public Dictionary<string, string> JsonConverter(jsonObjects results)
        {
            Dictionary<string, object> resultObjects = new Dictionary<string, object>();
            resultObjects = results.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(results, null));

            var emptyObjects = (from kv in resultObjects

                                where kv.Value != null
                                select kv).ToDictionary(kv => kv.Key, kv => kv.Value);

            Dictionary<string, string> jsonResult = emptyObjects.ToDictionary(k => k.Key, k => k.Value.ToString());

            return jsonResult;

        }

        [HttpPost]
        [Route("ExportAnnotations")]

        public IHttpActionResult ExportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            string jsonResult = pdfviewer.ExportAnnotation(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("ImportAnnotations")]
        public IHttpActionResult ImportAnnotations([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            string jsonResult = string.Empty;
            object JsonResult;

            if (jsonObject != null && jsonObject.ContainsKey("fileName"))
            {

                string documentPath = GetDocumentPath(jsonObject["filePath"]);
                if (!string.IsNullOrEmpty(documentPath))
                {
                    //Returns a string containing all the text in the file.
                    jsonResult = File.ReadAllText(documentPath);
                }
                else
                {
                    return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                }
            }
            else
            {
                string extension = Path.GetExtension(jsonObject["importedData"]);
                if (extension != ".xfdf")
                {
                    JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                    return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                }
                else
                {
                    //Gets the path of the PDF document.
                    string documentPath = GetDocumentPath(jsonObject["importedData"]);
                    if (!string.IsNullOrEmpty(documentPath))
                    {
                        byte[] bytes = File.ReadAllBytes(documentPath);
                        //Returns the byte as base64 string.
                        jsonObject["importedData"] = Convert.ToBase64String(bytes);
                        //Gets the annotation from the document.
                        JsonResult = pdfviewer.ImportAnnotation(jsonObject);
                        return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
                    }
                    else
                    {
                        return Content(HttpStatusCode.NotFound, jsonObject["document"] + " is not found");
                    }
                }
            }
            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("ImportFormFields")]
        public IHttpActionResult ImportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object jsonResult = pdfviewer.ImportFormFields(jsonObject);

            return Ok(JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("ExportFormFields")]
        public IHttpActionResult ExportFormFields([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            string jsonResult = pdfviewer.ExportFormFields(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("RenderPdfPages")]
        public IHttpActionResult RenderPdfPages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object jsonResult = pdfviewer.GetPage(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("Unload")]
        public IHttpActionResult Unload([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            pdfviewer.ClearCache(jsonObject);

            return Content(HttpStatusCode.OK, "Document cache is cleared");
        }

        [HttpPost]
        [Route("RenderThumbnailImages")]

        public IHttpActionResult RenderThumbnailImages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object result = pdfviewer.GetThumbnailImages(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(result));
        }

        [HttpPost]
        [Route("Bookmarks")]
        public IHttpActionResult Bookmarks([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object jsonResult = pdfviewer.GetBookmarks(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("RenderAnnotationComments")]

        public IHttpActionResult RenderAnnotationComments([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object jsonResult = pdfviewer.GetAnnotationComments(jsonObject);

            return Content(HttpStatusCode.OK, JsonConvert.SerializeObject(jsonResult));
        }

        [HttpPost]
        [Route("Download")]
        public IHttpActionResult Download([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            string documentBase = pdfviewer.GetDocumentAsBase64(jsonObject);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(documentBase, System.Text.Encoding.UTF8, "text/plain");

            return ResponseMessage(response);
        }


        [HttpPost]
        [Route("PrintImages")]
        public IHttpActionResult PrintImages([FromBody] Dictionary<string, string> jsonObject)
        {
            PdfRenderer pdfviewer = new PdfRenderer();

            object pageImage = pdfviewer.GetPrintImage(jsonObject);

            return Content(HttpStatusCode.OK, pageImage);
        }


        private HttpResponseMessage GetPlainText(string pageImage)
        {
            var responseText = new HttpResponseMessage(HttpStatusCode.OK);

            responseText.Content = new StringContent(pageImage, System.Text.Encoding.UTF8, "text/plain");

            return responseText;
        }

        private string GetDocumentPath(string FilePath)
        {
            string rootPath = HostingEnvironment.MapPath("~/");
            string documentPath = Path.Combine(rootPath, FilePath);

            if (System.IO.File.Exists(documentPath))
            {
                return documentPath;
            }
            return string.Empty;

        }



        //[HttpPost]
        //[Route("get/{id}")]
        //public IHttpActionResult AddFileDocumentApproval(string id)
        //{
        //    // Parse the 'id' parameter into a Guid variable
        //    if (Guid.TryParse(id, out Guid documentFileId))
        //    {
        //        var fileExist = db.DocumentApprovalFiles.FirstOrDefault(file =>
        //         file.DocumentFileId == documentFileId);

        //        if (fileExist != null)
        //        {
        //            string rootPath = HostingEnvironment.MapPath("~/");
        //            string fullPath = Path.Combine(rootPath, fileExist.FilePath);

        //            byte[] b = System.IO.File.ReadAllBytes(fullPath);
        //            string FileBase64 = "data:application/pdf;base64," + Convert.ToBase64String(b);
        //            return Ok(new
        //            {
        //                state = "true",
        //                file = fileExist,
        //                FileBase64
        //            });
        //        }

        //    }

        //    return Ok(new
        //    {
        //        state = "false",
        //    });
        //}

    }

}
