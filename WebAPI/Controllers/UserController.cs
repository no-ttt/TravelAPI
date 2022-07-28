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
    public class UserController : ControllerBase
    {
        /// <summary>
        /// 取得資料庫所有 User
        /// </summary>
        [HttpGet]
        public IActionResult GetUser()
        {
            string strSql = @"select name,
                                       create_date,
                                       modify_date,
                                       type_desc as type,
                                       authentication_type_desc as authentication_type
                                from sys.database_principals
                                where type not in ('A', 'G', 'R', 'X')
                                      and sid is not null
                                      and name != 'guest'
                                order by name";

            using (var db = new AppDb())
            {
                List<User> data = db.Connection.Query<User>(strSql).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 取得 Table 使用者權限
        /// </summary>
        [HttpGet]
        [Route("Table")]
        public IActionResult GetUserTable()
        {
            string strSql = @"select GRANTEE as user_name,
                                       TABLE_NAME as table_name,
	                                   PRIVILEGE_TYPE as privilege_type,
	                                   IS_GRANTABLE as is_grantable 
                                from INFORMATION_SCHEMA.TABLE_PRIVILEGES";

            using (var db = new AppDb())
            {
                List<UserTable> data = db.Connection.Query<UserTable>(strSql).ToList();
                return Ok(new { data });
            }
        }
        /// <summary>
        /// 取得 Column 使用者權限
        /// </summary>
        [HttpGet]
        [Route("Column")]
        public IActionResult GetUserColumn()
        {
            string strSql = @"select GRANTEE as user_name,
                                       TABLE_NAME as table_name,
	                                   COLUMN_NAME as column_name,
	                                   PRIVILEGE_TYPE as privilege_type,
	                                   IS_GRANTABLE as is_grantable
                                from INFORMATION_SCHEMA.COLUMN_PRIVILEGES";

            using (var db = new AppDb())
            {
                List<UserColumn> data = db.Connection.Query<UserColumn>(strSql).ToList();
                return Ok(new { data });
            }
        }
    }
}
