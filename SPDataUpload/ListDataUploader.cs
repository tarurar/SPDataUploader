using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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
        public void UploadToListByFileName(string csvPath, string csvDelimiter, ClientContext ctx, bool deleteData = false)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException();

            string fn = Path.GetFileNameWithoutExtension(csvPath);

            IEnumerable<List> allLists = ctx.LoadQuery(ctx.Web.Lists.Include(inc => inc.RootFolder, inc => inc.RootFolder.Name, inc => inc.ItemCount));
            ctx.ExecuteQuery();

            List targetList = allLists.SingleOrDefault(l => l.RootFolder.Name.Equals(fn, StringComparison.CurrentCultureIgnoreCase));

            if (targetList == null)
                throw new FileNotFoundException(String.Format("List {0} not found", fn));

            if (targetList.ItemCount > 0)
            {
                if (deleteData)
                {
                    ListItemCollection items = targetList.GetItems(CamlQuery.CreateAllItemsQuery());
                    ctx.Load(items);
                    ctx.ExecuteQuery();

                    for (int i = targetList.ItemCount - 1; i > -1; i--)
                    {
                        items[i].DeleteObject();
                        ctx.ExecuteQuery();
                    }
                }
                else
                    throw new Exception(
                        String.Format("List {0} has items. Specify clearly that the existant data has to be deleted.", fn));
            }

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
