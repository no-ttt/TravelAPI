using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using OpenXmlPowerTools;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpotController : ControllerBase
    {
        /// <summary>
        /// 取得指定 縣市鄉鎮市區 的景點
        /// </summary>
        [HttpGet]
        public IActionResult GetSpot(string city, int? type, int start = 1, int counts = 30)
        {
            string strSql = @"select * from vd_Spot
                                where CONCAT(city, town) = @city and (@type is null or type = @type)
                                order by oid offset @start - 1 rows fetch next @counts rows only";

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

            string mid = Request.Cookies["mid"];
            string spotCollection = @"
                declare @CID int
                select @CID = ClassID from Member where MID = @MID
                select count(*) from CO where OID = @OID and CID = @CID + 2;
            ";
            var p2 = new DynamicParameters();
            p2.Add("@MID", mid);
            p2.Add("@OID", oid);


            using (var db = new AppDb())
            {
                var data = db.Connection.Query<SpotDetail>(strSql, p);
                int result = db.Connection.QueryFirstOrDefault<int>(spotCollection, p2);

                return Ok(new { collectionResult = (result == 1 ? true : false), data });
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
        /// <summary>
        /// 所有收藏景點
        /// </summary>
        [HttpGet]
        [Route("Favorite")]
        public IActionResult collection()
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                declare @CID int
                select @CID = ClassID from Member where MID = @MID

                select v.* from CO, vd_Spot v 
                where CO.CID = @CID + 2 and CO.OID = v.oid
            ";

            var p = new DynamicParameters();
            p.Add("@MID", mid);

            using (var db = new AppDb())
            {
                List<Spot> data = db.Connection.Query<Spot>(strSql, p).ToList();

                return Ok(new { data });
            }
        }
        /// <summary>
        /// 將景點加入收藏
        /// </summary>
        [HttpPost]
        [Route("Favorite/add")]
        public IActionResult add(int oid)
        {
            string mid = Request.Cookies["mid"];
            string getCID = @"
                declare @CID int
                select @CID = ClassID from Member where MID = @MID
            ";

            string check = getCID + @"select top 1 1 from CO where OID = @OID and CID = @CID + 2";
            string strSql = getCID + @"insert CO(CID, OID) values(@CID + 2, @OID)";
            string spotCollection = getCID + @"select count(*) from CO where OID = @OID and CID = @CID + 2;";

            var p = new DynamicParameters();
            p.Add("@MID", mid);
            p.Add("@OID", oid);

            using (var db = new AppDb())
            {
                bool flag = db.Connection.QueryFirstOrDefault<bool>(check, p);
                if (!flag)
                {
                    db.Connection.Execute(strSql, p);
                    int result = db.Connection.QueryFirstOrDefault<int>(spotCollection, p);

                    return Ok(new { success = (result == 1 ? true : false) });
                }
                return StatusCode(500);
            }
        }
        /// <summary>
        /// 將景點移出收藏
        /// </summary>
        [HttpDelete]
        [Route("Favorite/delete")]
        public IActionResult delete(int oid)
        {
            string mid = Request.Cookies["mid"];
            string getCID = @"
                declare @CID int
                select @CID = ClassID from Member where MID = @MID
            ";

            string check = getCID + @"select top 1 1 from CO where OID = @OID and CID = @CID + 2";
            string strSql = getCID + @"delete CO where OID = @oid and CID = @cid + 2";
            string spotCollection = getCID + @"select count(*) from CO where OID = @OID and CID = @CID + 2;";

            var p = new DynamicParameters();
            p.Add("@MID", mid);
            p.Add("@OID", oid);

            using (var db = new AppDb())
            {
                bool flag = db.Connection.QueryFirstOrDefault<bool>(check, p);
                if (flag)
                {
                    db.Connection.Execute(strSql, p);
                    int result = db.Connection.QueryFirstOrDefault<int>(spotCollection, p);

                    return Ok(new { success = (result == 0 ? true : false) });
                }
                return StatusCode(500);
            }   
        }
    }
}
