using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FKController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有或指定 table 的 foreign key 及其相關的 primary key / table / column
        /// </summary>
        [HttpGet]
        public IActionResult GetFK(string Tab)
        {
            string strSql = @"select fk.name as fk_constraint_name,
	                            fk_cols.constraint_column_id as no,
	                            fk_tab.name as foreign_table,
	                            fk_col.name as fk_column_name, 
                                pk_tab.name as primary_table,
                                pk_col.name as pk_column_name
                            from sys.foreign_keys fk
                                inner join sys.tables fk_tab
                                    on fk_tab.object_id = fk.parent_object_id
                                inner join sys.tables pk_tab
                                    on pk_tab.object_id = fk.referenced_object_id
                                inner join sys.foreign_key_columns fk_cols
                                    on fk_cols.constraint_object_id = fk.object_id
                                inner join sys.columns fk_col
                                    on fk_col.column_id = fk_cols.parent_column_id
                                    and fk_col.object_id = fk_tab.object_id
                                inner join sys.columns pk_col
                                    on pk_col.column_id = fk_cols.referenced_column_id
                                    and pk_col.object_id = pk_tab.object_id
                            where fk_tab.name = @Tab or @Tab is null
                            order by fk_tab.name, pk_tab.name, fk_cols.constraint_column_id";

            var p = new DynamicParameters();
            p.Add("@Tab", Tab);

            using (var db = new AppDb())
            {
                List<FK> data = db.Connection.Query<FK>(strSql, p).ToList();
                return Ok(new { data });
            }
        }

        /// <summary>
        /// 取得資料庫指定 table 的相關 foreign key / table / column
        /// </summary>
        [HttpGet]
        [Route("{Tab}")]
        public IActionResult GetFKTab(string Tab)
        {
            string strSql = @"select fk.name as fk_constraint_name,
	                            fk_cols.constraint_column_id as no,
	                            fk_tab.name as foreign_table,
	                            fk_col.name as fk_column_name, 
                                pk_tab.name as primary_table,
                                pk_col.name as pk_column_name
                            from sys.foreign_keys fk
                                inner join sys.tables fk_tab
                                    on fk_tab.object_id = fk.parent_object_id
                                inner join sys.tables pk_tab
                                    on pk_tab.object_id = fk.referenced_object_id
                                inner join sys.foreign_key_columns fk_cols
                                    on fk_cols.constraint_object_id = fk.object_id
                                inner join sys.columns fk_col
                                    on fk_col.column_id = fk_cols.parent_column_id
                                    and fk_col.object_id = fk_tab.object_id
                                inner join sys.columns pk_col
                                    on pk_col.column_id = fk_cols.referenced_column_id
                                    and pk_col.object_id = pk_tab.object_id
                            where pk_tab.name = @Tab or @Tab is null
                            order by fk_tab.name, pk_tab.name, fk_cols.constraint_column_id";

            var p = new DynamicParameters();
            p.Add("@Tab", Tab);

            using (var db = new AppDb())
            {
                List<FK> data = db.Connection.Query<FK>(strSql, p).ToList();
                return Ok(new { data });
            }
        }
    }
}
