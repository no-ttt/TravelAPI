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
    public class MemberController : ControllerBase
    {
        /// <summary>
        /// 更新會員資料
        /// </summary>
        [HttpPut]
        [Route("update")]
        public IActionResult update(UpdateMemberInfo Info)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                update Object
                set CName = @cName, CDes = @cDes
                where OID = @mid

                update Member 
                set EMail = @email, Address = @city, Birthday = @birthday
                where MID = @mid
            ";

            var p = new DynamicParameters();
            p.Add("@mid", mid);
            p.Add("@cName", Info.cName);
            p.Add("@cDes", Info.cDes);
            p.Add("@email", Info.email);
            p.Add("@city", Info.city);
            p.Add("@birthday", Info.birthday);
            p.Add("@aid", Info.aid);

            if (Info.changeAvatar)
            {
                strSql += @"
                    insert ORel(OID1, OID2, Des) values(@mid, @aid, 'avatar')
                    update Member set ChangeAvatar = 1 where MID = @mid
                ";
            }

            using (var db = new AppDb())
            {
                db.Connection.Execute(strSql, p);
                return Ok(new { success = true });
            }
        }
    }
}
