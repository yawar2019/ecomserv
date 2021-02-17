using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Drawing;
using System.Data.SqlClient; 
using System.Net.Mail;
using System.Net;
using System.Drawing.Drawing2D;
using System.Configuration;
using System.Web.Configuration; 
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using nQuant;
using System.Text; 

public class clsCommon
{
    public string version = "3.6"; 
    public dynamic getJsonObject(string json_str)
    {
        dynamic obj = null;
        try
        {
            if (json_str == null)
                return obj;
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer()
            {
                MaxJsonLength = Int32.MaxValue // specify length as per your business requirement
            };
            serializer.RegisterConverters(new[] { new DynamicJsonConverter() });
            obj = serializer.Deserialize(json_str, typeof(object));
        }
        catch (Exception exp)
        {
            Log("getJsonObject", exp.Message, true, exp);
        }
        return obj;
    }
    public string get_value(dynamic val)
    {
        if (val == null)
            return "";
        return val.ToString().Trim();
    } 
   
    private int get_size_in_kb(string path)
    {
        long len = (new System.IO.FileInfo(path)).Length / 1000;
        return (int)len;
    }
    private Bitmap SaveByReducingSize(Bitmap img1)
    {
        Bitmap res = null;
        try
        {
            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Png);
            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
            EncoderParameters myEncoderParameters = new EncoderParameters(1);
            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 20L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            var quantizer = new WuQuantizer();
            using (var quantized = quantizer.QuantizeImage(img1))
            {
                Stream stream = new MemoryStream();
                quantized.Save(stream, jgpEncoder, myEncoderParameters);
                res = (Bitmap)Image.FromStream(stream);
            }
            img1.Dispose();
            img1 = null;
        }
        catch (Exception exp)
        {
            Log("SaveByReducingSize", exp.Message, true, exp);
        }
        return res;
    }
    private ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }
    public Bitmap SetTransparantImage(Image OriginalImage)
    {
        try
        {
            if (OriginalImage == null) return null;

            // Get the cutoff.
            int cutoff = 230;
            // Prepare the ImageAttributes.
            Color low_color = Color.FromArgb(cutoff, cutoff, cutoff);
            Color high_color = Color.FromArgb(255, 255, 255);
            ImageAttributes image_attr = new ImageAttributes();
            image_attr.SetColorKey(low_color, high_color);

            // Make the result image.
            int wid = OriginalImage.Width;
            int hgt = OriginalImage.Height;
            Bitmap bm = new Bitmap(wid, hgt);

            // Process the image.
            using (Graphics gr = Graphics.FromImage(bm))
            {
                // Fill with magenta.
                gr.Clear(Color.Transparent);

                // Copy the original image onto the result
                // image while using the ImageAttributes.
                Rectangle dest_rect = new Rectangle(0, 0, wid, hgt);
                gr.DrawImage(OriginalImage, dest_rect,
                    0, 0, wid, hgt, GraphicsUnit.Pixel, image_attr);
            }
            return bm;
            // Display the image.
            // string out_path = @"C:\VINOD\TEST\STICKERS\OUT\txt.png";
            // string path2 = get_out_file(path);
            // bm.Save(path2, ImageFormat.Png);
            // bm.Dispose();
            //bm = null;
        }
        catch (Exception ex)
        {
            Log("SetTransparantImage", ex.Message, true, ex);
            //MessageBox.Show(ex.Message);
        }
        return null;
    }
    public Bitmap ResizeImage(Image imgPhoto, int size)
    {
        // Image imgPhoto = Image.FromFile(stPhotoPath);

        int sourceWidth = imgPhoto.Width;
        int sourceHeight = imgPhoto.Height;
        int newWidth = size;
        int newHeight = size;
        //Consider vertical pics
        if (sourceWidth < sourceHeight)
        {
            int buff = newWidth;

            newWidth = newHeight;
            newHeight = buff;
        }

        int sourceX = 0, sourceY = 0, destX = 0, destY = 0;
        float nPercent = 0, nPercentW = 0, nPercentH = 0;

        nPercentW = ((float)newWidth / (float)sourceWidth);
        nPercentH = ((float)newHeight / (float)sourceHeight);
        if (nPercentH < nPercentW)
        {
            nPercent = nPercentH;
            destX = System.Convert.ToInt16((newWidth -
                      (sourceWidth * nPercent)) / 2);
        }
        else
        {
            nPercent = nPercentW;
            destY = System.Convert.ToInt16((newHeight -
                      (sourceHeight * nPercent)) / 2);
        }

        int destWidth = (int)(sourceWidth * nPercent);
        int destHeight = (int)(sourceHeight * nPercent);


        Bitmap bmPhoto = new Bitmap(newWidth, newHeight,
                      PixelFormat.Format24bppRgb);

        bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                     imgPhoto.VerticalResolution);

        Graphics grPhoto = Graphics.FromImage(bmPhoto);
        grPhoto.Clear(Color.White);
        grPhoto.InterpolationMode =
            System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

        grPhoto.DrawImage(imgPhoto,
            new Rectangle(destX, destY, destWidth, destHeight),
            new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
            GraphicsUnit.Pixel);

        grPhoto.Dispose();
        imgPhoto.Dispose();
        imgPhoto = null;
        grPhoto = null;
        return bmPhoto;
    }
    private Bitmap TrimImage(Bitmap img, int margin)
    {
        try
        {
            // Bitmap img = (Bitmap)Image.FromFile(path);
            //get image data
            System.Drawing.Imaging.BitmapData bd = img.LockBits(new Rectangle(Point.Empty, img.Size),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int[] rgbValues = new int[img.Height * img.Width];
            Marshal.Copy(bd.Scan0, rgbValues, 0, rgbValues.Length);
            img.UnlockBits(bd);


            #region determine bounds
            int left = bd.Width - margin;
            int top = bd.Height - margin;
            int right = margin;
            int bottom = margin;

            //determine top
            for (int i = 0; i < rgbValues.Length; i++)
            {
                int color = rgbValues[i] & 0xffffff;
                if (color != 0xffffff)
                {
                    int r = i / bd.Width;
                    int c = i % bd.Width;

                    if (left > c)
                    {
                        left = c;
                    }
                    if (right < c)
                    {
                        right = c;
                    }
                    bottom = r;
                    top = r;
                    break;
                }
            }

            //determine bottom
            for (int i = rgbValues.Length - 1; i >= 0; i--)
            {
                int color = rgbValues[i] & 0xffffff;
                if (color != 0xffffff)
                {
                    int r = i / bd.Width;
                    int c = i % bd.Width;

                    if (left > c)
                    {
                        left = c;
                    }
                    if (right < c)
                    {
                        right = c;
                    }
                    bottom = r;
                    break;
                }
            }

            if (bottom > top)
            {
                for (int r = top + 1; r < bottom; r++)
                {
                    //determine left
                    for (int c = 0; c < left; c++)
                    {
                        int color = rgbValues[r * bd.Width + c] & 0xffffff;
                        if (color != 0xffffff)
                        {
                            if (left > c)
                            {
                                left = c;
                                break;
                            }
                        }
                    }

                    //determine right
                    for (int c = bd.Width - 1; c > right; c--)
                    {
                        int color = rgbValues[r * bd.Width + c] & 0xffffff;
                        if (color != 0xffffff)
                        {
                            if (right < c)
                            {
                                right = c;
                                break;
                            }
                        }
                    }
                }
            }

            int width = right - left + 1;
            int height = bottom - top + 1;
            #endregion

            //copy image data
            int[] imgData = new int[width * height];
            for (int r = top; r <= bottom; r++)
            {
                Array.Copy(rgbValues, r * bd.Width + left, imgData, (r - top) * width, width);
            }

            //create new image
            Bitmap newImage = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData nbd
                = newImage.LockBits(new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(imgData, 0, nbd.Scan0, imgData.Length);
            newImage.UnlockBits(nbd);
            img.Dispose();
            img = null;
            return newImage;
        }
        catch (Exception ex)
        {
            Log("TrimImage", ex.Message, true, ex);
        }
        return null;
    }
    public string BitmapToBase64(Bitmap img)
    {
        string res = "";
        try
        {
            System.IO.MemoryStream ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            byte[] byteImage = ms.ToArray();
            res = Convert.ToBase64String(byteImage); // Get Base64
        }
        catch (Exception exp)
        {
            Log("BitmapToBase64", exp.Message, true, exp);
        }
        return res;
    }
    public System.Drawing.Bitmap Base64StringToBitmap(string base64String)
    {
        Bitmap bmpReturn = null;
        try
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            return (Bitmap)image;
        }
        catch (Exception exp)
        {
            Log("Base64StringToBitmap", exp.Message, true, exp);
        }
        return bmpReturn;
    }
    private Bitmap ChangePixelFormat(Bitmap inputImage, PixelFormat newFormat)
    {
        Bitmap bmp = new Bitmap(inputImage.Width, inputImage.Height, newFormat);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.DrawImage(inputImage, 0, 0);
        }
        return bmp;
    }
    
    public string get_temp_folder_path()
    {
        try
        {
            return System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath + "temp";
        }
        catch (Exception exp)
        {
            Log("get_temp_folder_path", exp.Message, true, exp);
        }
        return "";
    }
    public bool HasSpecialChars(string val)
    {
        try
        {
            var regexItem = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9]*$");
            if (!regexItem.IsMatch(val)) { return true; }
        }
        catch (Exception exp)
        {
            Log("CheckForSpecialChars", exp.Message, true, exp);
        }
        return false;
    }
    public string get_log_time_diff(DateTime date)
    {
        TimeSpan ts = (TimeSpan)(DateTime.Now - date);
        int res = (int)ts.TotalSeconds;
        return res.ToString().Trim();
    }

    public string GetDateInFormat(string date, string format)
    {
        if (date.Trim().Length == 0)
            return date;
        DateTime dateNew = DateTime.Now;
        if (DateTime.TryParse(date, out dateNew))
        {
            return dateNew.ToString(format);
        }
        else
        {
            Log("GetDateInFormat", "failed to convert date(" + date + ")", true, null);
            return date;
        }
    }

    public string ImageToBase64(string jpg_filepath)
    {
        string res = "";
        try
        {
            if (!File.Exists(jpg_filepath))
            {
                Log("ImageToBase64", "File does not exist(" + jpg_filepath + ")", true, null);
                return "";
            }
            return ImageToBase64(Image.FromFile(jpg_filepath));
        }
        catch (Exception exp)
        {
            Log("ImageToBase64", exp.Message, true, exp);
        }
        return res;
    }
    public string ImageToBase64(Image img)
    {
        string res = "";
        try
        {
            return "data:image/jpg;base64," + Convert.ToBase64String(ImageToByteArraybyImageConverter(img));
        }
        catch (Exception exp)
        {
            Log("ImageToBase643", exp.Message, true, exp);
        }
        return res;
    }
    private byte[] ImageToByteArraybyImageConverter(System.Drawing.Image image)
    {
        ImageConverter imageConverter = new ImageConverter();
        byte[] imageByte = (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
        return imageByte;
    }
    public bool IsEmpty(string val)
    {
        bool res = true;
        try
        {
            if (val == null)
                return res;
            if (val.ToString().Trim().Length == 0)
                return res;
            return false;
        }
        catch (Exception exp)
        {
            Log("IsEmpty", exp.Message, true, exp);
        }
        return res;
    }
    public string[] split(string val, string patt)
    {
        return System.Text.RegularExpressions.Regex.Split(val, patt);
    }
    public string GenerateRandomNo()
    {
        int _min = 1000;
        int _max = 9999;
        Random _rdm = new Random();
        string num = _rdm.Next(_min, _max).ToString() + DateTime.Now.ToString("MMddyyyyHHmmss");
        return num;
    }
    public string GetDecURL(string binary_val)
    {
        string[] splt = split(binary_val, "a");
        string res = "";
        for (int i = 0; i < splt.Length; i++)
            res += GetDecChar(splt[i]);
        return res;
    }
    public string GetDecChar(string inpString)
    {
        return Convert.ToChar(Convert.ToInt64(inpString)).ToString();
    }
   
    public string GetUniqNo()
    {
        string res = "";
        try
        {
            res = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            int _min = 10000;
            int _max = 99999;
            Random _rdm = new Random();
            res += "_" + _rdm.Next(_min, _max).ToString();
        }
        catch (Exception exp)
        {
            Log("GetUniqNo", exp.Message, true, exp);
        }
        return res;
    }
    public string GetConfig(string key)
    {
        string res = "";
        try
        {
            res = System.Configuration.ConfigurationManager.AppSettings.Get(key);
        }
        catch (Exception exp)
        {
            Log("GetConfig", exp.Message, true, exp);
        }
        return res;
    }
    public string get_all_pars()
    {
        string res = "";
        try
        {
            string[] keys = HttpContext.Current.Request.Form.AllKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                res += keys[i] + ": " + HttpContext.Current.Request[keys[i]] + ",";
            }
        }
        catch (Exception exp)
        {
            Log("get_all_pars", exp.Message);
        }
        return res;
    }
    
    public void Log(string logDesc)
    {
        Log("LogInfo", logDesc, false, null);
    }
    public void Log(string logDesc, string desc)
    {
        Log(logDesc, desc, false, null);
    }
    public void Log(string action, string logDesc, bool isError, Exception expp)
    {
        ecomserv.Common.clsLog clslog = new ecomserv.Common.clsLog();
        clslog.Log(action, logDesc, isError, expp);
    }
    public string GetTodayToSave()
    {
        return DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
    }
    public string GetTimeStamp()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }
    public bool GetDate(string date, string formate, out DateTime dateNew)
    {
        dateNew = DateTime.Now;
        if (DateTime.TryParseExact(date, formate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateNew))
            return true;
        return false;
    }
    public string GetDate(string date_str_input, string formate)
    {
        DateTime dateNew = DateTime.Now;
        if (DateTime.TryParse(date_str_input, out dateNew))
            return dateNew.ToString(formate);
        return date_str_input;
    }

    public string GetForSQL(string val)
    {
        string res = "";
        try
        {
            res = val.Replace("'", "");
            res = res.Replace("&", "and");
            res = res.Replace("\"", "");
        }
        catch (Exception exp)
        {
            Log("GetForSQL", exp.Message, true, exp);
        }
        return res;
    } 
     
    public bool send_email(bool is_html, List<string> tos, string sub, string mail_body, AttachmentCollection atts,  string email_from, string email_from_pass, string from_name,string smtp,int port)
    {
        try
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(smtp, port);
            client.Timeout = 100000000;
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.From = new System.Net.Mail.MailAddress(email_from, from_name);
            for (int i = 0; i < tos.Count; i++)
            {
                if (tos[i] != null)
                {
                    if (tos[i].Trim().Length > 0)
                        message.To.Add(tos[i]);
                }
            }
            if (message.To.Count == 0)
                return false;
            message.Subject = sub;
            message.Body = mail_body;
            message.IsBodyHtml = is_html;
            if (atts != null)
            {
                for (int i = 0; i < atts.Count; i++)
                    message.Attachments.Add(atts[i]);
            }
            System.Net.NetworkCredential myCreds = new System.Net.NetworkCredential(email_from, email_from_pass);
            client.Credentials = myCreds;
            client.EnableSsl = false;
            client.Send(message);
            return true;
        }
        catch (Exception exp)
        {
            ecomserv.Ecom.clsEcom.err += ", " +exp.ToString();
        }
        return false;
    }
}