using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using NLog.Fluent;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LBSEController : ControllerBase
    {
        /// <summary>
        /// 指定經緯度的附近景點 (公里)
        /// </summary>
        [HttpGet]
        [Route("Nearby")]
        public IActionResult GetNearbySpot(double lat, double lon, float distance, string type)
        {
            string strSql = @"declare @Point geometry = LBSE.dbo.fn_GetPointGeo(@lat, @lon)

                                select O.OID, O.CName, O.Type, P.PictureUrl, P.PositionLon, P.PositionLat
                                from Object O, POI P
                                where P.PID = O.OID and P.PositionLat != @lat and P.PositionLon != @lon
                                    and P.GeoIndex.STDistance(@Point) * 100 < @distance";

            if (type != null)
            {
                string cond = " and (type = " + Regex.Replace(type, ".{1}", "$0" + " or type = ");
                strSql = strSql + cond.Substring(0, cond.Length - 11) + ')';
            }

            strSql += " order by P.GeoIndex.STDistance(@Point)";

            var p = new DynamicParameters();
            p.Add("@lat", lat);
            p.Add("@lon", lon);
            p.Add("@distance", distance);
            p.Add("@type", type);

            using (var db = new AppDb())
            {
                List<Nearby> data = db.Connection.Query<Nearby>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 指定路線的附近景點 (公里)
        /// </summary>
        [HttpGet]
        [Route("Test")]
        public IActionResult GetPathNearbySpot(double startLat, double startLon, double endLat, double endLon, float distance)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                declare @lat_1 varchar(max) = Convert(varchar(max), @startLat, 3)
                declare @lon_1 varchar(max) = Convert(varchar(max), @startLon, 3)
                declare @lat_2 varchar(max) = Convert(varchar(max), @endLat, 3)
                declare @lon_2 varchar(max) = Convert(varchar(max), @endLon, 3)
                declare @g geometry
                set @g = geometry::STMLineFromText('MULTILINESTRING (('+ @lon_1+' '+@lat_1+', '+@lon_2+' '+@lat_2+'))', 4326);

                declare @CID int
                select @CID = ClassID from Member where MID = @mid
                select O.OID, O.CName, O.Type, P.PictureUrl, P.PositionLon, P.PositionLat
                from Object O, (select O.CName, P.* from CO, Object O, POI P where CO.CID = @CID + 2 and CO.OID = O.OID and O.OID = P.PID) as P
                where P.PID = O.OID and P.GeoIndex.STWithin(@g.STBuffer(0.03)) = 1

            ";

            var p = new DynamicParameters();
            p.Add("@startLat", startLat);
            p.Add("@startLon", startLon);
            p.Add("@endLat", endLat);
            p.Add("@endLon", endLon);
            p.Add("@distance", distance);
            p.Add("@mid", mid);

            using (var db = new AppDb())
            {
                List<Nearby> data = db.Connection.Query<Nearby>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
