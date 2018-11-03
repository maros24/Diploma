using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Drawing.Imaging;
using Diplom.Models;
using Diploma.Models;
using System.Text;
using System.Security.Cryptography;
using static Diploma.MvcApplication;

namespace Diploma.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CryptoEncrypted()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CryptoEncrypted(HttpPostedFileBase upload1, string key, string algoritm)
        {
            var baseLocation = Server.MapPath("~/Files/");

            var fileName1 = GetUniqueFileName(upload1.FileName);
            var fileLocation1 = Path.Combine(baseLocation, fileName1);
            upload1.SaveAs(fileLocation1);

            StreamReader sr = new StreamReader(fileLocation1, Encoding.Default);
            string text = sr.ReadToEnd();

            if (algoritm == "AES")
            {
                text = Crypto.EncryptStringAES(text, key);
            }
            else if (algoritm == "TripleDES")
            {
                text = TripleDes.Encrypt<TripleDESCryptoServiceProvider>(text, key);
            }
            else if (algoritm == "Rijndael")
            {
                text = TripleDes.Encrypt<RijndaelManaged>(text, key);
            }

            var baseLocationResult = Server.MapPath("~/Text/");
            var fileNameRes = GetUniqueFileName("res.txt");
            var fileLocationRes = Path.Combine(baseLocationResult, fileNameRes);

            System.IO.File.AppendAllText(fileLocationRes, text, Encoding.Default);
            TempData["FileLocationRes"] = fileLocationRes;
            return RedirectToAction("PreviewCryptoEn");

        }
        [NoDirectAccess]
        public ActionResult PreviewCryptoEn() 
            {
                var fileLocation = TempData["FileLocationRes"] as string;
                ViewBag.Location = fileLocation;
                return View();
            }

        [NoDirectAccess]
        public FileResult DownloadCryptoEn(string path)
            {
                var doc = new byte[0];
                doc = System.IO.File.ReadAllBytes(path);
                return File(doc, "application / octet - stream", "encrypted.txt");
            }

        [HttpGet]
        public ActionResult CryptoDecrypted()
        {
            return View();

        }
        [HttpPost]
        public ActionResult CryptoDecrypted(HttpPostedFileBase upload, string key, string algoritm)
        {

            var baseLocation = Server.MapPath("~/Text/");
            var fileName1 = GetUniqueFileName(upload.FileName);
            var fileLocation1 = Path.Combine(baseLocation, fileName1);
            upload.SaveAs(fileLocation1);

            StreamReader sr = new StreamReader(fileLocation1, Encoding.Default);
            string extractedText = sr.ReadToEnd();

            
            var fileNameRes = GetUniqueFileName("res.txt");
            var fileLocationRes = Path.Combine(baseLocation, fileNameRes);

            try
            {
                if (algoritm == "AES")
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, key) + Environment.NewLine;
                }
                else if (algoritm == "TripleDES")
                {
                    extractedText = TripleDes.Decrypt<TripleDESCryptoServiceProvider>(extractedText, key) + Environment.NewLine;
                }
                else if (algoritm == "Rijndael")
                {
                    extractedText = TripleDes.Decrypt<RijndaelManaged>(extractedText, key) + Environment.NewLine;
                }

                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Default);
                TempData["FileLocationCrypto"] = fileLocationRes;
                return RedirectToAction("PreviewCryptoDecrypt");
            }
            catch
            {
                extractedText = "Wrong password";
                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Default);
                TempData["FileLocationCrypto"] = fileLocationRes;
                return RedirectToAction("PreviewCryptoDecrypt");
            }
        }
        [NoDirectAccess]
        public ActionResult PreviewCryptoDecrypt()
        {
            var fileLocation = TempData["FileLocationCrypto"] as string;
            ViewBag.Location = fileLocation;
            return View();
        }

        [NoDirectAccess]
        public FileResult DownloadCryptoDecrypt(string path)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(path);
            return File(doc, "application / octet - stream", "decrypted.txt");
        }


        [HttpGet]
        public ActionResult Encrypted()
        {
            return View();
        }

        [NoDirectAccess]
        private string GetUniqueFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                    + "_"
                    + Guid.NewGuid().ToString().Substring(0, 4)
                    + Path.GetExtension(fileName);
        }

        public byte[] arr;
        [HttpPost]
        public ActionResult  Encrypted(HttpPostedFileBase upload1, HttpPostedFileBase upload2, string key, string algoritm)
        {
            var baseLocation = Server.MapPath("~/Files/");

            var fileName1 = GetUniqueFileName(upload1.FileName);
            var fileLocation1 = Path.Combine(baseLocation, fileName1);
            upload1.SaveAs(fileLocation1);

            var fileName2 = GetUniqueFileName(upload2.FileName);
            var fileLocation2 = Path.Combine(baseLocation, fileName2);
            upload2.SaveAs(fileLocation2);


                Image image = Image.FromStream(upload2.InputStream);
                image.Save(fileLocation2);
                string imagePath = fileLocation2;
                Bitmap bmp = new Bitmap(imagePath);

                StreamReader sr = new StreamReader(fileLocation1, Encoding.Default);
                string text = sr.ReadToEnd();

            if (algoritm == "AES")
            {
                text = Crypto.EncryptStringAES(text, key);
            }
            else if (algoritm == "TripleDES")
            {
                text = TripleDes.Encrypt<TripleDESCryptoServiceProvider>(text, key);
            }
            else if(algoritm== "Rijndael")
            {
                text = TripleDes.Encrypt<RijndaelManaged>(text, key);
            }

            Bitmap bmp1 = SteganoHelper.embedText(text, bmp);
            arr = bmp1.ToByteArray(ImageFormat.Png);
            MemoryStream ms = new MemoryStream(arr);
            Image imageResult = Image.FromStream(ms);

            var fileNameRes = "res.png";
            var fileLocationRes = Path.Combine(baseLocation, fileNameRes);
            imageResult.Save(fileLocationRes);

            TempData["ImageLocation"] = fileNameRes;
            return RedirectToAction("Preview");
        }

        [NoDirectAccess]
        public ActionResult Preview()
        {
            var vm = new PreviewImageVm();
            var fileName = TempData["ImageLocation"] as string;
            vm.ImageName = fileName;
            return View(vm);
        }
        [NoDirectAccess]
        public FileResult Download(string path)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(Server.MapPath(path));
            return File(doc, "image/png", "encrypt.png");
        }


        [HttpGet]
        public ActionResult EncryptedOnly()
        {
            return View();

        }
        [HttpPost]
        public ActionResult EncryptedOnly(HttpPostedFileBase upload1, HttpPostedFileBase upload2)
        {
            var baseLocation = Server.MapPath("~/Files/");

            var fileName1 = GetUniqueFileName(upload1.FileName);
            var fileLocation1 = Path.Combine(baseLocation, fileName1);
            upload1.SaveAs(fileLocation1);

            var fileName2 = GetUniqueFileName(upload2.FileName);
            var fileLocation2 = Path.Combine(baseLocation, fileName2);
            upload2.SaveAs(fileLocation2);


            Image image = Image.FromStream(upload2.InputStream);
            image.Save(fileLocation2);
            string imagePath = fileLocation2;
            Bitmap bmp = new Bitmap(imagePath);

            StreamReader sr = new StreamReader(fileLocation1,Encoding.Unicode);
            string text = sr.ReadToEnd();
           
            Bitmap bmp1 = SteganoHelper.embedText(text, bmp);
            arr = bmp1.ToByteArray(ImageFormat.Png);
            MemoryStream ms = new MemoryStream(arr);
            Image imageResult = Image.FromStream(ms);

            var fileNameRes = "res.png";
            var fileLocationRes = Path.Combine(baseLocation, fileNameRes);
            imageResult.Save(fileLocationRes);

            TempData["ImageLocationOnly"] = fileNameRes;
            return RedirectToAction("PreviewOnly");
        }
        [NoDirectAccess]
        public ActionResult PreviewOnly()
        {
            var vm = new PreviewImageVm();
            var fileName = TempData["ImageLocationOnly"] as string;
            vm.ImageName = fileName;
            return View(vm);
        }
        [NoDirectAccess]
        public FileResult DownloadOnly(string path)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(Server.MapPath(path));
            return File(doc, "image/png", "encrypt.png");
        }


        [HttpGet]
        public ActionResult Decrypted()
        {
            return View();

        }

        [HttpPost]
        public  ActionResult Decrypted(HttpPostedFileBase upload, string key, string algoritm)
        {
            string fileName = GetUniqueFileName(upload.FileName);
            Image image = Image.FromStream(upload.InputStream);
            image.Save(Server.MapPath("~/Files/" + fileName));

            string imagePath = Server.MapPath("~/Files/" + fileName);
            Bitmap bmp = new Bitmap(imagePath);
            string extractedText = SteganoHelper.extractText(bmp);


            var baseLocation = Server.MapPath("~/Text/");
            var fileNameRes = GetUniqueFileName("res.txt");
            var fileLocationRes = Path.Combine(baseLocation, fileNameRes);

            try
            {
                if (algoritm == "AES")
                {
                    extractedText = Crypto.DecryptStringAES(extractedText, key) + Environment.NewLine;
                }
                else if (algoritm== "TripleDES")
                {
                    extractedText = TripleDes.Decrypt<TripleDESCryptoServiceProvider>(extractedText, key) + Environment.NewLine;
                }
                else if (algoritm== "Rijndael")
                {
                    extractedText = TripleDes.Decrypt<RijndaelManaged>(extractedText, key) + Environment.NewLine;
                }
                
                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Default);
            TempData["FileLocation"] = fileLocationRes;
            return RedirectToAction("PreviewDecrypt");
            }
            catch
            {
                extractedText = "Wrong password";
                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Default);
                TempData["FileLocation"] = fileLocationRes;
                return RedirectToAction("PreviewDecrypt");
            }
        }
        [NoDirectAccess]
        public ActionResult PreviewDecrypt()
        {
            var fileLocation = TempData["FileLocation"] as string;
            ViewBag.Location = fileLocation;
            return View();
        }
        [NoDirectAccess]
        public FileResult DownloadDecrypt(string path)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(path);
            return File(doc, "application / octet - stream", "decrypted.txt");
        }

        [HttpGet]
        public ActionResult DecryptedOnly()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DecryptedOnly(HttpPostedFileBase upload)
        {
            string extractedText;
            string fileName = GetUniqueFileName(upload.FileName);
            Image image = Image.FromStream(upload.InputStream);
            image.Save(Server.MapPath("~/Files/" + fileName));

            string imagePath = Server.MapPath("~/Files/" + fileName);
            Bitmap bmp = new Bitmap(imagePath);
            


            var baseLocation = Server.MapPath("~/Text/");
            var fileNameRes = GetUniqueFileName("res.txt");
            var fileLocationRes = Path.Combine(baseLocation, fileNameRes);

            try
            {
                extractedText = SteganoHelper.extractText(bmp);
         
                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Unicode);
                TempData["FileLocationOnly"] = fileLocationRes;
                return RedirectToAction("PreviewDecryptOnly");
            }
            catch
            {
                extractedText = "Wrong password";
                System.IO.File.AppendAllText(fileLocationRes, extractedText, Encoding.Unicode);
                TempData["FileLocationOnly"] = fileLocationRes;
                return RedirectToAction("PreviewDecryptOnly");
            }
        }
        [NoDirectAccess]
        public ActionResult PreviewDecryptOnly()
        {
            var fileLocation = TempData["FileLocationOnly"] as string;
            ViewBag.Location = fileLocation;
            return View();
        }
        [NoDirectAccess]
        public FileResult DownloadDecryptOnly(string path)
        {
            var doc = new byte[0];
            doc = System.IO.File.ReadAllBytes(path);
            return File(doc, "application / octet - stream", "decrypted.txt");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}