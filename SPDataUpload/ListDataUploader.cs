using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;
using Microsoft.VisualBasic.FileIO;
using FieldType = Microsoft.VisualBasic.FileIO.FieldType;
using File = System.IO.File;

namespace SPDataUpload
{
    public class ListDataUploader : Uploader
    {
        public void UploadToListByFileName(string csvPath, string csvDelimiter, ClientContext ctx)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException();

            string fn = Path.GetFileNameWithoutExtension(csvPath);

            IEnumerable<List> allLists = ctx.LoadQuery(ctx.Web.Lists.Include(inc => inc.RootFolder, inc => inc.RootFolder.Name));
            ctx.ExecuteQuery();

            List targetList = allLists.SingleOrDefault(l => l.RootFolder.Name == fn);

            if (targetList == null)
                throw new FileNotFoundException(String.Format("List {0} not found", fn));

            Upload(csvPath, csvDelimiter, targetList, ctx);
        }
        public void Upload(string csvPath, string csvDelimiter, List targetList, ClientContext ctx)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException();

            TextFieldParser parser = new TextFieldParser(csvPath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(csvDelimiter);
            string[] headers = parser.ReadFields();

            while (!parser.EndOfData)
            {
                string[] data = parser.ReadFields();
                // todo: some warning
                // if (headers.Length != data.Length)

                ListItem newItem = targetList.AddItem(new ListItemCreationInformation());
                for (int i = 0; i < headers.Length; i++)
                {
                    newItem[headers[i]] = data[i];
                }
                newItem.Update();
                ctx.Load(newItem);
                ctx.ExecuteQuery();
            }
        }
    }
}
