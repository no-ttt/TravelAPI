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
    public class TripController : ControllerBase
    {
        /// <summary>
        /// 取得會員建立的所有行程
        /// </summary>
        [HttpGet]
        public IActionResult allTrip()
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                select O.OID as oid, O.CName as cName, O.CDes as cDes, convert(varchar, T.StartDate, 111) as startDate, T.DayNum as dayNum, convert(varchar, O.Since, 111) as since
                from Object O, Trip T
                where O.OwnerMID = @mid and O.type = 12 and O.OID = T.TID
            ";

            var p = new DynamicParameters();
            p.Add("@mid", mid);

            using (var db = new AppDb())
            {
                List<Trip> data = db.Connection.Query<Trip>(strSql, p).ToList();

                return Ok(new { data });
            }
        }
        /// <summary>
        /// 取得指定行程的詳細資料
        /// </summary>
        [HttpGet]
        [Route("Detail")]
        public IActionResult tripDetail(int oid)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                select O.CName as cName, O.CDes as cDes, convert(varchar, O.Since, 111) as since, T.StartPos as startPos, T.EndPos as endPos, convert(varchar, T.StartDate, 111) as startDate, T.DayNum as dayNum, T.Traffic as traffic, T.TripID as tripID,
                    (select M.CName from Object M where M.OID = @mid) as ownerName
                from Object O, Trip T 
                where O.OID = @oid and O.OwnerMID = @mid and T.TID = @OID
            ";

            var p = new DynamicParameters();
            p.Add("@mid", mid);
            p.Add("@oid", oid);

            using (var db = new AppDb())
            {
                List<TripDatail> data = db.Connection.Query<TripDatail>(strSql, p).ToList();

                return Ok(new { data });
            }
        }
        /// <summary>
        /// 新增行程
        /// </summary>
        [HttpPost]
        [Route("Add")]
        public IActionResult addTrip(AddTrip Info)
        {
            string mid = Request.Cookies["mid"];
            string strSql = @"
                declare @NewUUID varchar(50) = NEWID()
                insert Object(CName, Type, CDes, OwnerMID) values(@tripName, 12, @remark, @mid)
                declare @_NewOID int = @@Identity
                insert Trip(TID, StartPos, EndPos, StartDate, Traffic, TripID) values(@_NewOID, @startPos, @endPos, @startDate, @Traffic, @NewUUID)

                select @_NewOID as NewOID
            ";

            var p = new DynamicParameters();
            p.Add("@mid", mid);
            p.Add("@tripName", Info.tripName);
            p.Add("@remark", Info.remark);
            p.Add("@startPos", Info.startPos);
            p.Add("@endPos", Info.endPos);
            p.Add("@startDate", Info.startDate);
            p.Add("@Traffic", Info.traffic);

            using (var db = new AppDb())
            {
                int NewOID = db.Connection.QueryFirstOrDefault<int>(strSql, p);

                return Ok(new { data = NewOID });
            }
        }
    }
}
