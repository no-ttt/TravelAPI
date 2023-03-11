using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPI.Lib;
using WebAPI.model;
using static WebAPI.Startup;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        /// <summary>
        /// 上傳檔案 (頭貼、評論照片)
        /// </summary>
        [HttpPost]
        [Route("uploadImg")]
        public IActionResult uploadImg(IFormFile Files)
        {
            string mid = Request.Cookies["mid"];
            string strSql = $@"xp_insertArchive";

            var p = new DynamicParameters();
            p.Add("@FileName", Path.GetFileNameWithoutExtension(Files.FileName));
            p.Add("@FileExtension", Path.GetExtension(Files.FileName).Replace(".", ""));
            p.Add("@ContentLen", Files.Length);
            p.Add("@ContentType", Files.ContentType);
            p.Add("@MID", mid);
            p.Add("@NewOID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@NewUUID", dbType: DbType.String, direction: ParameterDirection.Output, size: 100);

            using (var db = new AppDb())
            {
                var result = db.Connection.Execute("xp_insertArchive", p, commandType: CommandType.StoredProcedure);
                int NewID = p.Get<int>("@NewOID");

                string FileStorage_Root = AppConfig.Config["Filestorage:Default"];
                var ext = Path.GetExtension(Files.FileName).ToLowerInvariant();
                string HexStr = Convert.ToString(Convert.ToInt32(NewID), 16).PadLeft(8, '0');
                string SubFilePath = string.Join("/", System.Text.RegularExpressions.Regex.Split(HexStr, "(?<=\\G.{2})(?!$)"));
                string FilePath = Path.Combine(FileStorage_Root, SubFilePath);

                FileInfo fi = new FileInfo(FilePath + ext);
                fi.Directory.Create();
                using (FileStream fs = fi.Create())
                {
                    Files.CopyTo(fs);
                }

                return Ok(new { data = p.Get<int>("NewOID") });
            }
        }
        /// <summary>
        /// 取得檔案
        /// </summary>
        [HttpGet]
        [Route("download/Avatar/{AID}/{UUID}")]
        public IActionResult download(int AID, string UUID)
        {
            string strSql = @"select top 1 1 from Archive where AID = @AID and UUID = @UUID";

            var p = new DynamicParameters();
            p.Add("@AID", AID);
            p.Add("@UUID", UUID);

            using (var db = new AppDb())
            {
                bool flag = db.Connection.QueryFirstOrDefault<bool>(strSql, p);

                if (flag)
                {
                    strSql = @"select FileExtension from Archive where AID = @AID and UUID = @UUID";
                    string ext = db.Connection.QueryFirstOrDefault<String>(strSql, p);

                    string FileStorage_Root = AppConfig.Config["Filestorage:Default"];
                    string HexStr = Convert.ToString(Convert.ToInt32(AID), 16).PadLeft(8, '0');
                    string SubFilePath = string.Join("/", System.Text.RegularExpressions.Regex.Split(HexStr, "(?<=\\G.{2})(?!$)"));
                    string FilePath = Path.Combine(FileStorage_Root, SubFilePath) + "." + ext;

                    List<Stream> docxs = new List<Stream>();
                    byte[] docx = System.IO.File.ReadAllBytes(FilePath);

                    return File(
                        fileContents: docx,
                        contentType: "image/" + ext
                    );
                }

                return Ok();
            }
        }
    }
}
