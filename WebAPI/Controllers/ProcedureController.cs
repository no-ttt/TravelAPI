﻿using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcedureController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 Procedure
        /// </summary>
        [HttpGet]
        public IActionResult GetFunction()
        {
            string strSql = @"select obj.name as name,
	                                    obj.create_date as created,
	                                    obj.modify_date as last_modified,
		                                ISNULL(p.value, '') as remark
                                from sys.objects obj
		                                left join sys.extended_properties as p
                                            on obj.object_id = p.major_id and p.minor_id = 0
                                where obj.type in ('P', 'X')
                                order by name";

            using (var db = new AppDb())
            {
                List<Table> data = db.Connection.Query<Table>(strSql).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 指定 Procedure 使用的 Object (uses)
        /// </summary>
        [HttpGet]
        [Route("Uses")]
        public IActionResult GeProcedureUses(string proc)
        {
            string strSql = @"select dep_obj.name as object_name,
                                       dep_obj.type_desc as object_type
                                from sys.objects obj
                                left join sys.sql_expression_dependencies dep
                                          on dep.referencing_id = obj.object_id
                                left join sys.objects dep_obj
                                          on dep_obj.object_id = dep.referenced_id
                                where obj.type in ('P', 'X', 'PC', 'RF')
                                    and obj.name = @proc 
                                order by object_name";

            var p = new DynamicParameters();
            p.Add("@proc", proc);

            using (var db = new AppDb())
            {
                List<objUse> data = db.Connection.Query<objUse>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 有使用到指定 Procedure 的 Object (used)
        /// </summary>
        [HttpGet]
        [Route("Used")]
        public IActionResult GetProcedureUsed(string proc)
        {
            string strSql = @"select ref_o.name as object_name,
                                       ref_o.type_desc as object_type
                                from sys.objects o
                                join sys.sql_expression_dependencies dep
                                     on o.object_id = dep.referenced_id
                                join sys.objects ref_o
                                     on dep.referencing_id = ref_o.object_id
                                where o.type in ('P', 'X') and o.name = @proc
                                order by object_name";

            var p = new DynamicParameters();
            p.Add("@proc", proc);

            using (var db = new AppDb())
            {
                List<objUse> data = db.Connection.Query<objUse>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 指定 Procedure 的 Type 和 Script
        /// </summary>
        [HttpGet]
        [Route("Info")]
        public IActionResult GetProcedureInfo(string proc)
        {
            string strSql = @"select case type
                                            when 'P' then 'SQL Stored Procedure'
                                            when 'X' then 'Extended stored procedure'
                                        end as type,
                                        mod.definition as script
                                from sys.objects obj
                                join sys.sql_modules mod
                                        on mod.object_id = obj.object_id
                                cross apply (select p.name + ' ' + TYPE_NAME(p.user_type_id) + ', ' 
                                                from sys.parameters p
                                                where p.object_id = obj.object_id 
                                                    and p.parameter_id != 0 
                                                for xml path ('') ) par (parameters)
                                where obj.type in ('P', 'X') and obj.name = @proc";

            var p = new DynamicParameters();
            p.Add("@proc", proc);

            using (var db = new AppDb())
            {
                List<Info> data = db.Connection.Query<Info>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
