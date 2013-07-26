using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xilium.CefGlue;
using System.IO;
using System.Threading;
using System.Collections.Specialized;
using System.Web;

namespace Xilium.CefGlue.Client
{
    class MySchemeHandler : CefResourceHandler
    {
        byte[] responseData;
        int    pos;
        string url;

        Thread mythread;

        CefCallback callback;

        void MyThread()
        {
            NameValueCollection queryPars = HttpUtility.ParseQueryString(url);
            string remoteCallback = queryPars[0];
            string remoteData     = queryPars[1];

            responseData = Encoding.UTF8.GetBytes(remoteCallback + "(" + remoteData + ");");

            Thread.Sleep(1000); // Wait 1 sec

            this.callback.Continue();
        }

        protected override bool ProcessRequest(CefRequest request, CefCallback callback)
        {
            this.url      = request.Url;
            this.callback = callback;

            this.mythread = new Thread(new ThreadStart(this.MyThread));
            this.mythread.Start();

            return true;
        }

        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            response.MimeType   = "application/javascript";
            response.Status     = 200;
            response.StatusText = "OK, hello from handler!";

            var headers = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);
            headers.Add("Cache-Control", "private");
            response.SetHeaderMap(headers);

            responseLength = responseData.LongLength;
            redirectUrl = null;
        }

        protected override bool ReadResponse(Stream response, int bytesToRead, out int bytesRead, CefCallback callback)
        {
            if (bytesToRead == 0 || pos >= responseData.Length)
            {
                bytesRead = 0;
                return false;
            }
            else
            {
                response.Write(responseData, pos, bytesToRead);
                pos += bytesToRead;
                bytesRead = bytesToRead;
                return true;
            }
        }

        protected override bool CanGetCookie(CefCookie cookie)
        {
            return false;
        }

        protected override bool CanSetCookie(CefCookie cookie)
        {
            return false;
        }

        protected override void Cancel()
        {
        }
    }
}
