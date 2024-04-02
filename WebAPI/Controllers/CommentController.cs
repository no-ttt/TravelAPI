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

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        /// <summary>
        /// 取得景點的所有評論
        /// </summary>
        [HttpGet]
        public IActionResult addComment(int oid)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                select cid, mid, cName, changeAvatar, avatarURL, avatarPath, cDes, star5, thumbUp, convert(varchar, since, 111) as create_date, img,
	                ISNULL((select top 1 1 from ORel where Des = 'like' and OID1 = cid and OID2 = @mid), 0) as [like], 
	                (
		                case when @mid = (select OwnerMID from Object where OID = cid) then 1
		                else 0 
	                end ) as bDel
                from vd_Comment where PID = @oid
                order by since desc
            ";

            var p = new DynamicParameters();
            p.Add("@oid", oid);
            p.Add("@mid", mid);

            using (var db = new AppDb())
            {
                List<SpotComment> data = db.Connection.Query<SpotComment>(strSql, p).ToList();

                return Ok(new { data });
            }
        }
        /// <summary>
        /// 添加景點評論
        /// </summary>
        [HttpPost]
        [Route("Add")]
        public IActionResult addComment(AddComment Info)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                insert Object(Type, CDes, OwnerMID) values(11, @CDes, @MID)
                declare @_NewOID int = @@Identity

                insert ORel(OID1, OID2, Des) values(@_NewOID, @OID, 'comment')
                insert Comment(CID, Star5) values(@_NewOID, @star)

                select @_NewOID as NewOID
            ";

            var p = new DynamicParameters();
            p.Add("@MID", mid);
            p.Add("@OID", Info.oid);
            p.Add("@cDes", Info.cDes);
            p.Add("@star", Info.star5);

            using (var db = new AppDb())
            {
                int NewOID = db.Connection.QueryFirstOrDefault<int>(strSql, p);

                if (Info.imgs.Length != 0)
                {
                    string imgORel = @"insert ORel(OID1, OID2, Des) values(@_NewOID, @AID, 'comment_img')";
                    var p2 = new DynamicParameters();
                    p2.Add("@_NewOID", NewOID);

                    for (int i = 0; i < Info.imgs.Length; i++)
                    {
                        p2.Add("@AID", Info.imgs[i]);
                        db.Connection.Query(imgORel, p2);
                    }
                }
                
                
                return Ok(new { data = NewOID });
            }
        }
        /// <summary>
        /// 刪除景點評論
        /// </summary>
        [HttpDelete]
        [Route("Delete")]
        public IActionResult deleteComment(int cid)
        {
            string strSql = @"
                delete ORel where OID1 = @cid
                delete Object where OID = @cid
                delete Comment where CID = @cid
            ";

            var p = new DynamicParameters();
            p.Add("@cid", cid);

            using (var db = new AppDb())
            {
                db.Connection.Execute(strSql, p);
                return Ok(new { success = true });
            }
        }
        /// <summary>
        /// 對景點按讚/取消讚
        /// </summary>
        [HttpPut]
        [Route("Like")]
        public IActionResult ThumbUp(int cid)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                if exists(select top 1 * from ORel where OID1 = @cid and OID2 = @mid)
                begin
	                delete ORel where OID1 = @cid and OID2 = @mid
	                update Object set nClick = nClick - 1 where OID = @cid
                end
                else
                begin
	                insert ORel(OID1, OID2, Des) values(@cid, @mid, 'like')
	                update Object set nClick = nClick + 1 where OID = @cid
                end
            ";

            var p = new DynamicParameters();
            p.Add("@mid", mid);
            p.Add("@cid", cid);

            Console.WriteLine(mid);
            Console.WriteLine(cid);

            using (var db = new AppDb())
            {
                db.Connection.Execute(strSql, p);

                return Ok(new { success = true });
            }
        }
    }
}
