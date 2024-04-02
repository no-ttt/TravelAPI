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
        /// 指定路線的附近景點 (公尺)
        /// </summary>
        [HttpGet]
        [Route("LineNearbyPoint")]
        public IActionResult GetPathNearbySpot(string lonLatString, float distance)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"exec xp_LineNearbyPoint @LonLatString, @mid, @distance";

            var p = new DynamicParameters();
            p.Add("@LonLatString", lonLatString);
            p.Add("@mid", mid);
            p.Add("@distance", distance);

            using (var db = new AppDb())
            {
                List<Nearby> data = db.Connection.Query<Nearby>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
