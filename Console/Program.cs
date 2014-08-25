using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using SPDataUpload;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ClientContext ctx = new ClientContext(""))
            {
                ctx.ExecutingWebRequest += (sender, eventArgs) =>
                {
                    eventArgs.WebRequestExecutor.RequestHeaders.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                };
                ctx.Credentials = new NetworkCredential("", "", "");

                IEnumerable<List> allLists = ctx.LoadQuery(ctx.Web.Lists.Include(inc => inc.RootFolder, inc => inc.RootFolder.Name));
                ctx.ExecuteQuery();
                
                List targetList = allLists.SingleOrDefault(l => l.RootFolder.Name == "testImportList");
                if (targetList != null)
                {
                    ListDataUploader uploader = new ListDataUploader();    
                    uploader.Upload("TestList3.txt", ",", targetList, ctx);
                }
            }
        }
    }
}
