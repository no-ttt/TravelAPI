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
    public class SpotController : ControllerBase
    {
        /// <summary>
        /// 取得指定 縣市/鄉鎮市區 的景點
        /// </summary>
        [HttpGet]
        public IActionResult GetSpot(string city, int? type, int start = 1, int counts = 30)
        {
            string strSql = @"select * from vd_Spot
                                where CONCAT(city, town) = @city and (@type is null or type = @type)
                                order by oid offset @start - 1 row fetch next @counts rows only";

            string totalSql = @"select count(*) as total from vd_Spot
                                where CONCAT(city, town) = @city and (@type is null or type = @type)";

            var p = new DynamicParameters();
            p.Add("@city", city);
            p.Add("@type", type);
            p.Add("@start", start);
            p.Add("@counts", counts);

            using (var db = new AppDb())
            {
                var total = db.Connection.QueryFirstOrDefault<int>(totalSql, p);
                List<Spot> data = db.Connection.Query<Spot>(strSql, p).ToList();

                return Ok(new { total = total, data });
            }
        }
        /// <summary>
        /// 搜尋景點 (Autocomplete)
        /// </summary>
        [HttpGet]
        [Route("AutoComplete")]
        public IActionResult GetAuto(string city)
        {
            string strSql = @"select TOP 5 CONCAT(city, town) as result from vd_City where CONCAT(city, town) like '%' + @city + '%'";
            if(city != null) city = '%' + Regex.Replace(city, ".{1}", "$0" + '%');

            var p = new DynamicParameters();
            p.Add("@city", city);

            using (var db = new AppDb())
            {
                List<AutoComplete> data = db.Connection.Query<AutoComplete>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 景點詳細資訊
        /// </summary>
        [HttpGet]
        [Route("Detail")]
        public IActionResult GetSpotDetail(int oid)
        {
            string strSql = @"select * from vd_SpotDetail where oid = @oid";

            var p = new DynamicParameters();
            p.Add("@oid", oid);

            using (var db = new AppDb())
            {
                var data = db.Connection.Query<SpotDetail>(strSql, p);
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 指定城市/區域 的熱門景點 (TOP 20)
        /// </summary>
        [HttpGet]
        [Route("HotSpot")]
        public IActionResult GetHotSpot(string area, string city, int? type, int top = 20)
        {
            string strSql = "";
            var p = new DynamicParameters();

            if (area == null)
            {
                // 搜尋有圖片的 data
                strSql = @"select TOP (@top) * from vd_Spot where CONCAT(city, town) = @city and pictureUrl is not null";
                p.Add("@top", top);
                p.Add("@city", city);
            }
            else if (city == null)
            {
                // 搜尋有圖片的 data
                strSql = @"select TOP (@top) * from vd_Spot where area = @area and (@type is null or type = @type) and pictureUrl is not null";
                p.Add("@top", top);
                p.Add("@area", area);
                p.Add("@type", type);
            }

            using (var db = new AppDb())
            {
                List<Spot> data = db.Connection.Query<Spot>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
