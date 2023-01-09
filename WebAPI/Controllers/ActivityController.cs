using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        /// <summary>
        /// 所有/指定區域 範圍時間內的活動資訊
        /// </summary>
        [HttpGet]
        public IActionResult GetActivity(string area, int month)
        {
            // 日期定在 2022-11-01，並搜尋有圖片連結的 data
            string strSql = @"select * from vd_Activity
                                where startTime <= DATEADD(month, @month, '2022-11-01') and startTime >= '2022-11-01' and pictureUrl is not null";

            var p = new DynamicParameters();
            p.Add("@month", month);

            if (area != null)
            {
                strSql += " and area = @area";
                p.Add("@area", area);

            }

            using (var db = new AppDb())
            {
                List<Activity> data = db.Connection.Query<Activity>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 取得活動資訊詳細資料
        /// </summary>
        [HttpGet]
        [Route("Detail")]
        public IActionResult GetActivityDetail(int oid)
        {
            string strSql = @"select * from vd_ActivityDetail where oid = @oid";

            var p = new DynamicParameters();
            p.Add("@oid", oid);

            using (var db = new AppDb())
            {
                List<ActivityDetail> data = db.Connection.Query<ActivityDetail>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
