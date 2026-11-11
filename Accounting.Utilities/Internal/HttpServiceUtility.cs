using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Accounting.Entities.DB.Transactions;
namespace Accounting.Utilities.Internal
{
    public class HttpServiceUtility
    {
        // Flag: Has Dispose already been called?
        bool disposed = false;
        // Instantiate a SafeHandle instance.
        SafeHandle handle = new SafeFileHandle(nint.Zero, true);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                handle.Dispose();
                // Free any other managed objects here.
                //
            }
            disposed = true;
        }

        public static object ExecuteSOAPPostRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, string SOAPAction, object Req_Object, Type TargetType, bool EnableSSL = false, bool EnableHtmlDecode = false, Dictionary<string, string> ResponseReplacePair = null, Dictionary<string, string> HeaderPairs = null, string RequestContentType = null, string Dynamic_Namespace = null)
        {
            if (_serviceLogInfo == null || string.IsNullOrEmpty(Service_Url))
            {
                if (_serviceLogInfo == null) _serviceLogInfo = new ServiceLogInfo();
                _serviceLogInfo.Message = "Invalid Data.";
                return _serviceLogInfo;
            }
            HttpServiceUtility objHttpServiceUtility = new HttpServiceUtility();
            try
            {
                return objHttpServiceUtility.InvokeSOAPPostRequest(_serviceLogInfo, Service_Url, SOAPAction, Req_Object, TargetType, EnableSSL, EnableHtmlDecode, ResponseReplacePair, HeaderPairs, RequestContentType);
            }
            catch (Exception ex)
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
                throw ex;
            }
            finally
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
            }
        }
        public static object ExecuteJsonPostRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, object Req_Object, Type TargetType, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, bool unescapedJsonString = false, string RequestContentType = null, string RequestMethod = null)
        {
            if (_serviceLogInfo == null || string.IsNullOrEmpty(Service_Url))
            {
                if (_serviceLogInfo == null) _serviceLogInfo = new ServiceLogInfo();
                _serviceLogInfo.Message = "Invalid Data.";
                return _serviceLogInfo;
            }
            HttpServiceUtility objHttpServiceUtility = new HttpServiceUtility();
            try
            {
                return objHttpServiceUtility.InvokeJsonPostRequest(_serviceLogInfo, Service_Url, Req_Object, TargetType, EnableSSL, HeaderPairs, unescapedJsonString, RequestContentType, RequestMethod);
            }
            catch (Exception ex)
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
                throw ex;
            }
            finally
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
            }
        }

        public static object ExecuteJsonGetRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, Type TargetType, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, Dictionary<string, string> ResponseReplacePair = null, string ContentType = null)
        {
            if (_serviceLogInfo == null || string.IsNullOrEmpty(Service_Url))
            {
                if (_serviceLogInfo == null) _serviceLogInfo = new ServiceLogInfo();
                _serviceLogInfo.Message = "Invalid Data.";
                return _serviceLogInfo;
            }
            HttpServiceUtility objHttpServiceUtility = new HttpServiceUtility();
            try
            {
                return objHttpServiceUtility.InvokeJsonGETRequest(_serviceLogInfo, Service_Url, TargetType, EnableSSL, HeaderPairs, ResponseReplacePair, ContentType);
            }
            catch (Exception ex)
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
                throw ex;
            }
            finally
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
            }
        }
        internal object InvokeJsonPostRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, object Req_Object, Type TargetType, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, bool unescapedJsonString = false, string RequestContentType = null, string RequestMethod = null)
        {
            try
            {
                if (Req_Object != null && Req_Object.GetType() != typeof(string))
                {
                    _serviceLogInfo.Request = JsonConvert.SerializeObject(Req_Object, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                }
                else if (Req_Object != null && Req_Object.GetType() == typeof(string))
                {
                    _serviceLogInfo.Request = Convert.ToString(Req_Object);
                }
                Uri uri = new Uri(Service_Url);
                HttpWebRequest httpwebrequest = (HttpWebRequest)WebRequest.Create(uri);
                if (HeaderPairs != null && HeaderPairs.Count > 0)
                {
                    foreach (var pair in HeaderPairs)
                    {
                        httpwebrequest.Headers.Add(pair.Key, pair.Value);
                    }
                }
                if (!string.IsNullOrEmpty(RequestMethod))
                {
                    httpwebrequest.Method = RequestMethod;
                }
                else
                {
                    httpwebrequest.Method = "POST";
                }
                if (!string.IsNullOrEmpty(RequestContentType))
                {
                    httpwebrequest.ContentType = RequestContentType;
                }
                else
                {
                    httpwebrequest.ContentType = "application/json; charset=utf-8";
                    httpwebrequest.Accept = "application/json";//newly added...
                }
                httpwebrequest.Timeout = Timeout.Infinite;
                httpwebrequest.KeepAlive = true;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                using (StreamWriter writer = new StreamWriter(httpwebrequest.GetRequestStream()))
                {
                    writer.Write(_serviceLogInfo.Request);
                }
                WebResponse response = null;
                try
                {
                    if (EnableSSL)
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.MaxServicePointIdleTime = 120000;
                        ServicePointManager.ServerCertificateValidationCallback = new
    RemoteCertificateValidationCallback
    (
    delegate { return true; }
    );
                    }
                    response = httpwebrequest.GetResponse();

                }
                catch (WebException ex)
                {
                    try
                    {
                        using (WebResponse webresponse = ex.Response)
                        {
                            if (webresponse != null)
                            {
                                HttpWebResponse httpResponse = (HttpWebResponse)webresponse;
                                using (Stream data = webresponse.GetResponseStream())
                                using (var strreader = new StreamReader(data))
                                {
                                    _serviceLogInfo.Response = strreader.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (Exception iex)
                    {
                        throw new InvalidDataException(iex.Message);
                    }
                    _serviceLogInfo.Message = ex.Message;
                }
                catch (Exception ex)
                {
                    _serviceLogInfo.Is_Success = false;
                    _serviceLogInfo.Message = ex.Message;
                }
                if (response != null)
                {
                    Stream reader = response.GetResponseStream();
                    StreamReader sReader = new StreamReader(reader);
                    _serviceLogInfo.Response = sReader.ReadToEnd();
                    sReader.Close();
                }
                timer.Stop();
                _serviceLogInfo.ResponseTime_In_Sec = Convert.ToInt32(timer.ElapsedMilliseconds / 1000);
                if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && IsValidJson(_serviceLogInfo.Response) && TargetType != null)
                {
                    if (unescapedJsonString)
                    {
                        _serviceLogInfo.Response = JsonConvert.DeserializeObject<string>(_serviceLogInfo.Response);
                    }
                    _serviceLogInfo.Is_Success = true;
                    _serviceLogInfo.Message = "Success";
                    return JsonConvert.DeserializeObject(_serviceLogInfo.Response, TargetType);// JsonConvert.DeserializeObject(_serviceLogInfo.Response);
                                                                                               // return jsonSerializer.Deserialize(_serviceLogInfo.Res_Data, TargetType);
                }
            }
            catch (Exception ex)
            {
                _serviceLogInfo.Message = ex.Message;
            }
            return null;
        }
        internal object InvokeJsonGETRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, Type TargetType, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, Dictionary<string, string> ResponseReplacePair = null, string ContentType = null)
        {
            try
            {
                Uri uri = new Uri(Service_Url);
                HttpWebRequest httpwebrequest = (HttpWebRequest)WebRequest.Create(uri);
                if (HeaderPairs != null && HeaderPairs.Count > 0)
                {
                    foreach (var pair in HeaderPairs)
                    {
                        httpwebrequest.Headers.Add(pair.Key, pair.Value);
                    }
                }
                httpwebrequest.Method = "GET";
                if (!string.IsNullOrEmpty(ContentType))
                    httpwebrequest.ContentType = ContentType;
                else if (ContentType == null)
                    httpwebrequest.ContentType = "application/json; charset=utf-8";
                httpwebrequest.Accept = "application/json";//newly added...
                httpwebrequest.Timeout = Timeout.Infinite;
                httpwebrequest.KeepAlive = true;
                WebResponse response = null;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                try
                {
                    if (EnableSSL)
                    {
                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.MaxServicePointIdleTime = 120000;
                        ServicePointManager.ServerCertificateValidationCallback = new
    RemoteCertificateValidationCallback
    (
    delegate { return true; }
    );
                    }
                    response = httpwebrequest.GetResponse();
                }
                catch (WebException ex)
                {
                    try
                    {
                        using (WebResponse webresponse = ex.Response)
                        {
                            if (webresponse != null)
                            {
                                HttpWebResponse httpResponse = (HttpWebResponse)webresponse;
                                using (Stream data = webresponse.GetResponseStream())
                                using (var strreader = new StreamReader(data))
                                {
                                    _serviceLogInfo.Response = strreader.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (Exception iex)
                    {
                        throw new InvalidDataException(iex.Message);
                    }
                    _serviceLogInfo.Message = ex.Message;
                }

                catch (Exception ex)
                {
                    _serviceLogInfo.Is_Success = false;
                    _serviceLogInfo.Message = ex.Message;
                }
                timer.Stop();
                _serviceLogInfo.ResponseTime_In_Sec = Convert.ToInt32(timer.ElapsedMilliseconds / 1000);
                if (response != null)
                {
                    Stream reader = response.GetResponseStream();
                    StreamReader sReader = new StreamReader(reader);
                    _serviceLogInfo.Response = sReader.ReadToEnd();
                    sReader.Close();
                }
                if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && ResponseReplacePair != null && ResponseReplacePair.Count > 0)
                {
                    foreach (var pair in ResponseReplacePair)
                    {
                        _serviceLogInfo.Response = _serviceLogInfo.Response.Replace(pair.Key, pair.Value);
                    }
                }
                if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && TargetType != null)
                {
                    _serviceLogInfo.Is_Success = true;
                    _serviceLogInfo.Message = "Success";
                    return JsonConvert.DeserializeObject(_serviceLogInfo.Response, TargetType);
                }
            }
            catch (Exception ex)
            {
                _serviceLogInfo.Message = ex.Message;
            }
            return null;
        }
        internal bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if (strInput.StartsWith("{") && strInput.EndsWith("}") || //For object
                strInput.StartsWith("[") && strInput.EndsWith("]")) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public string ExecuteGETService(string url, bool EnableSSL = false, bool htmlDecode = false, string contentType = null, Dictionary<string, string> HeaderPairs = null)
        {
            string result = "";
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                if (!string.IsNullOrEmpty(contentType))
                    request.ContentType = contentType;
                else
                    request.ContentType = "application/xml";
                request.Method = "GET";
                request.Timeout = Timeout.Infinite;
                request.KeepAlive = true;
                request.ReadWriteTimeout = Timeout.Infinite;
                if (HeaderPairs != null && HeaderPairs.Count > 0)
                {
                    foreach (var pair in HeaderPairs)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }
                DateTime startTime = DateTime.Now;
                if (EnableSSL)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;// | SecurityProtocolType.Ssl3;
                    ServicePointManager.MaxServicePointIdleTime = 120000;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.ServerCertificateValidationCallback = new
 RemoteCertificateValidationCallback
 (
 delegate { return true; }
 );
                }
                using (WebResponse webResponse = request.GetResponse())
                {
                    Stream s = webResponse.GetResponseStream();
                    Encoding enc = Encoding.GetEncoding("utf-8");
                    StreamReader readStream = new StreamReader(s, enc);
                    result = readStream.ReadToEnd();
                }
                if (htmlDecode && !string.IsNullOrEmpty(result))
                {
                    result = HttpUtility.HtmlDecode(result);
                }
            }
            catch (Exception ex) { }

            return result;
        }

        internal object InvokeSOAPPostRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, string SOAPAction, object Req_Object, Type TargetType, bool EnableSSL = false, bool EnableHtmlDecode = false, Dictionary<string, string> ResponseReplacePair = null, Dictionary<string, string> HeaderPairs = null, string RequestContentType = null)
        {
            try
            {
                if (Req_Object != null)
                    _serviceLogInfo.Request = XMLUtility.Serialize(Req_Object.GetType(), Req_Object);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Service_Url);
                if (!string.IsNullOrEmpty(RequestContentType))
                {
                    request.ContentType = RequestContentType;
                }
                else
                {
                    request.ContentType = "application/xml";
                    request.Accept = "application/xml";
                }

                request.Method = "POST";
                request.Timeout = 600000;
                if (SOAPAction != null)
                    request.Headers.Add("SOAPAction", SOAPAction);
                if (HeaderPairs != null && HeaderPairs.Count > 0)
                {
                    foreach (var pair in HeaderPairs)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }


                DateTime startTime = DateTime.Now;
                try
                {


                    if (EnableSSL)
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                        ServicePointManager.MaxServicePointIdleTime = 120000;
                        ServicePointManager.Expect100Continue = false;
                        ServicePointManager.ServerCertificateValidationCallback = new
RemoteCertificateValidationCallback
(
delegate { return true; }
);
                    }
                    using (Stream stm = request.GetRequestStream())
                    {
                        using (StreamWriter stmw = new StreamWriter(stm))
                        {
                            stmw.Write(_serviceLogInfo.Request);
                        }
                    }

                    using (WebResponse webResponse = request.GetResponse())
                    {
                        Stream s = webResponse.GetResponseStream();
                        Encoding enc = Encoding.GetEncoding("utf-8");
                        StreamReader readStream = new StreamReader(s, enc);
                        _serviceLogInfo.Response = readStream.ReadToEnd();
                    }

                    if (EnableHtmlDecode && !string.IsNullOrEmpty(_serviceLogInfo.Response))
                        _serviceLogInfo.Response = HttpUtility.HtmlDecode(_serviceLogInfo.Response);
                    if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && ResponseReplacePair != null && ResponseReplacePair.Count > 0)
                    {
                        foreach (var pair in ResponseReplacePair)
                        {
                            _serviceLogInfo.Response = _serviceLogInfo.Response.Replace(pair.Key, pair.Value);
                        }
                    }
                }
                catch (WebException ex)
                {
                    try
                    {
                        using (WebResponse webresponse = ex.Response)
                        {
                            if (webresponse != null)
                            {
                                HttpWebResponse httpResponse = (HttpWebResponse)webresponse;
                                using (Stream data = webresponse.GetResponseStream())
                                using (var strreader = new StreamReader(data))
                                {
                                    _serviceLogInfo.Response = strreader.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (Exception iex)
                    {
                        throw new InvalidDataException(iex.Message);
                    }
                    _serviceLogInfo.Message = ex.Message;
                }
                catch (Exception ex)
                {
                    _serviceLogInfo.Message = ex.Message;
                }
                DateTime endTime = DateTime.Now;
                _serviceLogInfo.ResponseTime_In_Sec = Convert.ToInt32((endTime - startTime).TotalSeconds);
                if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && TargetType != null)
                {
                    try
                    {
                        if (!XMLUtility.IsValidXML(_serviceLogInfo.Response))
                        {
                            _serviceLogInfo.Response = _serviceLogInfo.Response.Replace("&", "'&amp;");
                        }
                        if (XMLUtility.IsValidXML(_serviceLogInfo.Response))
                            return XMLUtility.DeSerialize(TargetType, _serviceLogInfo.Response);
                    }
                    catch (Exception ex) { }
                }
            }
            catch (Exception ex)
            {
                _serviceLogInfo.Message = ex.Message;
            }
            return null;
        }
        public static byte[] DownloadPdfBytes(string Service_Url, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, string RequestContentType = null)
        {
            HttpServiceUtility objHttpServiceUtility = new HttpServiceUtility();
            try
            {
                return objHttpServiceUtility.DownloadPdf(Service_Url, EnableSSL, HeaderPairs, RequestContentType);
            }
            catch (Exception ex)
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
                throw ex;
            }
            finally
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
            }
        }
        internal byte[] DownloadPdf(string Service_Url, bool EnableSSL = false, Dictionary<string, string> HeaderPairs = null, string RequestContentType = null)
        {
            byte[] pdfBytes = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Service_Url);
                request.Method = "POST";
                if (!string.IsNullOrEmpty(RequestContentType))
                {
                    request.ContentType = RequestContentType;
                }
                else
                {
                    request.ContentType = "application/pdf";
                }
                request.Timeout = Timeout.Infinite;
                request.KeepAlive = true;
                request.ReadWriteTimeout = Timeout.Infinite;
                if (HeaderPairs != null && HeaderPairs.Count > 0)
                {
                    foreach (var pair in HeaderPairs)
                    {
                        request.Headers.Add(pair.Key, pair.Value);
                    }
                }
                if (EnableSSL)
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;//| SecurityProtocolType.Ssl3;
                    ServicePointManager.MaxServicePointIdleTime = 120000;
                    ServicePointManager.Expect100Continue = false;
                    ServicePointManager.ServerCertificateValidationCallback = new
RemoteCertificateValidationCallback
(
delegate { return true; }
);
                }
                WebResponse pdfdownloadresponse = (HttpWebResponse)request.GetResponse();
                if (string.IsNullOrEmpty(RequestContentType))
                {
                    byte[] buffer = new byte[1024]; MemoryStream objMemoryStream = new MemoryStream();
                    Stream responsestream = pdfdownloadresponse.GetResponseStream();
                    while (true)
                    {
                        int bytesRead = responsestream.Read(buffer, 0, buffer.Length);
                        if (bytesRead < 5) { break; }
                        else
                        {
                            objMemoryStream.Write(buffer, 0, bytesRead);
                            //boolPolicyGenerationFlag = true;
                        }
                    }
                    pdfBytes = objMemoryStream.ToArray();
                    if (pdfBytes.Length < 1000)
                    {
                        pdfBytes = null;
                    }
                    responsestream.Close();
                    objMemoryStream.Close();
                }
                else
                {

                    Stream reader = pdfdownloadresponse.GetResponseStream();
                    StreamReader sReader = new StreamReader(reader);
                    string base64 = sReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(base64))
                    {
                        base64 = base64.Replace("\"", "");
                        pdfBytes = Convert.FromBase64String(base64);
                    }
                    sReader.Close();
                }
                pdfdownloadresponse.Close();
            }
            catch (Exception ex) { }
            return pdfBytes;
        }



        public static T DeepClone<T>(T self)
        {
            var serialized = JsonConvert.SerializeObject(self);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
        public static object ExecuteXMLPostRequest(ServiceLogInfo _serviceLogInfo, string Service_Url, object Req_Object, Type TargetType, string CertificatePath = null, string CertificatePassword = null)
        {
            if (_serviceLogInfo == null || string.IsNullOrEmpty(Service_Url))
            {
                if (_serviceLogInfo == null) _serviceLogInfo = new ServiceLogInfo();
                _serviceLogInfo.Message = "Invalid Data.";
                return _serviceLogInfo;
            }
            HttpServiceUtility objHttpServiceUtility = new HttpServiceUtility();
            try
            {
                return objHttpServiceUtility.InvokeXMLPostRequest(_serviceLogInfo, Service_Url, Req_Object, TargetType, CertificatePath, CertificatePassword);
            }
            catch (Exception ex)
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
                throw ex;
            }
            finally
            {
                if (objHttpServiceUtility != null)
                    objHttpServiceUtility.Dispose();
            }
        }

        internal object InvokeXMLPostRequest(ServiceLogInfo _serviceLogInfo, string ServiceURL, object Req_Object, Type TargetType, string CertificatePath = null, string CertificatePassword = null)
        {
            _serviceLogInfo.Request = XMLUtility.Serialize(Req_Object.GetType(), Req_Object);


            HttpWebRequest getrequest = (HttpWebRequest)WebRequest.Create(ServiceURL);
            if (CertificatePath is not null && CertificatePassword is not null)
            {
                X509Certificate2 certificate = new X509Certificate2(CertificatePath, CertificatePassword, X509KeyStorageFlags.MachineKeySet);
                getrequest.ClientCertificates.Add(certificate);
            }
            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback(delegate { return true; });
            ServicePointManager.MaxServicePointIdleTime = 120000;
            ServicePointManager.Expect100Continue = false;


            getrequest.Method = "POST";
            getrequest.ContentType = "text/xml";
            getrequest.Accept = "text/xml";
            Stopwatch timer = new Stopwatch();
            timer.Start();
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(_serviceLogInfo.Request);
                getrequest.ContentLength = bytes.Length;

                Stream requestStream = getrequest.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);

                WebResponse response = getrequest.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream);

                _serviceLogInfo.Response = reader.ReadToEnd();
                timer.Stop();
                _serviceLogInfo.ResponseTime_In_Sec = Convert.ToInt32(timer.ElapsedMilliseconds / 1000);
                if (!string.IsNullOrEmpty(_serviceLogInfo.Response) && TargetType != null)
                {
                    try
                    {
                        if (!XMLUtility.IsValidXML(_serviceLogInfo.Response))
                        {
                            _serviceLogInfo.Response = _serviceLogInfo.Response.Replace("&", "'&amp;");
                        }
                        if (XMLUtility.IsValidXML(_serviceLogInfo.Response))
                        {
                            _serviceLogInfo.Is_Success = true;
                            _serviceLogInfo.Message = "Success";
                            return XMLUtility.DeSerialize(TargetType, _serviceLogInfo.Response);
                        }
                    }
                    catch (Exception ex) { }
                }

                stream.Dispose();
                reader.Dispose();
            }
            catch (WebException webEx)
            {
                if (webEx != null && webEx.Response != null)
                {
                    using (WebResponse response = webEx.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;

                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            _serviceLogInfo.Response = reader.ReadToEnd().ToString();
                        }
                    }
                }
            }
            return null;
        }
        public static string ExecuteXMLPostRequest(
    ServiceLogInfo serviceLogInfo,
    string serviceUrl,
    string xmlRequest,
    string certificatePath = null,
    string certificatePassword = null)
        {
            var http = new HttpServiceUtility();

            try
            {
                return http.InvokeXMLPostRequest(
                    serviceLogInfo,
                    serviceUrl,
                    xmlRequest,
                    certificatePath,
                    certificatePassword);
            }
            finally
            {
                http.Dispose();
            }
        }
        internal string InvokeXMLPostRequest(
        ServiceLogInfo serviceLogInfo,
        string serviceURL,
        string xmlRequest,
        string certificatePath = null,
        string certificatePassword = null)
        {
            serviceLogInfo.Request = xmlRequest;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serviceURL);

            // Certificate (optional)
            if (!string.IsNullOrEmpty(certificatePath) && !string.IsNullOrEmpty(certificatePassword))
            {
                var certificate = new X509Certificate2(
                    certificatePath,
                    certificatePassword,
                    X509KeyStorageFlags.MachineKeySet);

                request.ClientCertificates.Add(certificate);
            }

            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => true);

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.MaxServicePointIdleTime = 120000;

            request.Method = "POST";
            request.ContentType = "application/xml";
            request.Accept = "application/xml";

            var timer = Stopwatch.StartNew();

            try
            {
                // Write request
                byte[] bytes = Encoding.UTF8.GetBytes(xmlRequest);
                request.ContentLength = bytes.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                // Get response
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    serviceLogInfo.Response = reader.ReadToEnd();
                }

                timer.Stop();
                serviceLogInfo.ResponseTime_In_Sec =
                    Convert.ToInt32(timer.ElapsedMilliseconds / 1000);

                serviceLogInfo.Is_Success = true;
                serviceLogInfo.Message = "Success";

                return serviceLogInfo.Response; // ✅ RETURN RAW XML
            }
            catch (WebException ex)
            {
                using (var response = ex.Response)
                using (var stream = response?.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    serviceLogInfo.Response = reader.ReadToEnd();
                }

                serviceLogInfo.Is_Success = false;
                serviceLogInfo.Message = ex.Message;

                return serviceLogInfo.Response;
            }
        }
    }
}
