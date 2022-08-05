using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;
using DocxProcessor;
using System.IO;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDocx()
        {
            List<DocTable> table = new List<DocTable>();
            List<DocColumn> column = new List<DocColumn>();
            List<Stream> docxs = new List<Stream>();

            string tableSql = @"select t.name as tableName from sys.tables t order by name";

            string columnSql = @"select tab.name as tableName,
                                        col.name,
                                        t.name as data_type,
	                                    ISNULL(p.value, '') as des
                                    from sys.tables as tab
                                        inner join sys.columns as col
                                            on tab.object_id = col.object_id
                                        left join sys.types as t
		                                    on col.user_type_id = t.user_type_id
	                                    left join sys.extended_properties as p
		                                    on tab.object_id = p.major_id and col.column_id = p.minor_id
                                    order by tab.name, column_id";

            using (var db = new AppDb())
            {
                table = db.Connection.Query<DocTable>(tableSql).ToList();
                column = db.Connection.Query<DocColumn>(columnSql).ToList();
            }

            // 從實體路徑讀檔案
            byte[] docx = System.IO.File.ReadAllBytes(".\\Templates\\test.docx");

            // 取代
            var wordProcessor = new ReplaceWordTemplate();
            foreach (var tb in table)
            {
                byte[] tmp;
                var col = column.Where(c => c.tableName == tb.tableName)
                    .Select(c => new { c.name, c.data_type, c.des })
                    .ToList();
                
                tmp = wordProcessor.Replace(docx, col);
                tmp = wordProcessor.Replace(tmp, tb);

                docxs.Add(new MemoryStream(tmp));
            }

            // 取套件
            MergeWordTemplate WordMerger = new MergeWordTemplate();

            // 合併文件
            byte[] final = WordMerger.MergeDocxsIntoOne(docxs);

            // 設定檔名
            string fileName = "finish";

            // 回傳合併檔案
            return File(
                fileContents: final,
                contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                fileDownloadName: $"{fileName}.docx"
            );
        }
    }
}
