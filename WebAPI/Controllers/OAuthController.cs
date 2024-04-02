using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Dapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Lib;
using WebAPI.model;
using System.Text;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using System.IdentityModel.Tokens.Jwt;
using static WebAPI.Startup;
using System;
using System.IO;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OAuthController : ControllerBase
    {
        /// <summary>
        /// 取得會員資料
        /// </summary>
        [HttpGet]
        [Route("Me")]
        public IActionResult me()
        {
            string mid = Request.Cookies["mid"];
            string passportCode = Request.Cookies["passportCode"];
            if (mid == null)
            {
                return Ok(new { data = "null" });
            }
            else
            {
                string avatarUpdate = @"
                    declare @AID int
                    select @AID = max(OID2) from ORel where OID1 = @mid and ORel.Des = 'avatar'
                    select AID, UUID from Archive where AID = @AID
                ";

                var p2 = new DynamicParameters();
                p2.Add("@mid", mid);

                string strSql = @"
                    if exists(select top 1 MID from MSession where PassportCode = @passportCode and MID = @mid)
                    begin
                        select OID, CName, CDes, convert(varchar, Since, 111) as Since, EMail, Address, convert(varchar, Birthday, 111) as Birthday, AvatarURL from vs_Member where OID = @mid
                    end
                ";

                var p = new DynamicParameters();
                p.Add("@mid", mid);
                p.Add("@passportCode", passportCode);

                using (var db = new AppDb())
                {
                    List<MemberInfo> data = db.Connection.Query<MemberInfo>(strSql, p).ToList();

                    string avatarAlter = @"select top 1 1 OID2 from ORel where OID1 = @mid and ORel.Des = 'avatar'";
                    bool flag = db.Connection.QueryFirstOrDefault<bool>(avatarAlter, p2);
                    if (flag)
                    {
                        List<MemberAvatar> avatar = db.Connection.Query<MemberAvatar>(avatarUpdate, p2).ToList();
                        return Ok(new { data, avatarAlter = $"http://localhost:3000/api/File/download/Avatar/{avatar[0].AID}/{avatar[0].UUID}" });
                    }
                    else
                    {
                        return Ok(new { data, avatarAlter = "null" });
                    }
                }
            }
        }
        /// <summary>
        /// 系統登出
        /// </summary>
        [HttpGet]
        [Route("logout")]
        public IActionResult logout()
        {
            Response.Cookies.Delete("mid");
            Response.Cookies.Delete("passportCode");

            return Redirect("http://localhost:8888");
        }
        /// <summary>
        /// Google 驗證登入授權
        /// </summary>
        [IgnoreLogInActionFilter]
        [HttpGet]
        [Route("google")]
        public IActionResult GetGoogleLogin()
        {
            string strUrl = "https://accounts.google.com/o/oauth2/auth";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("client_id=726039261276-o4317cal25od66cqojl414lg4k5gdlv3.apps.googleusercontent.com&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/google/callback&");
            StrParam.Append("response_type=code&");
            StrParam.Append("scope=profile email&");
            StrParam.Append("access_type=offline&");
            StrParam.Append("state=&");

            return Redirect(strUrl + "?" + StrParam.ToString());
        }
        /// <summary>
        /// 接收 Google CallBack 回來的參數及取得使用者資訊
        /// </summary>
        [IgnoreLogInActionFilter]
        [HttpGet]
        [Route("google/callback")]
        public IActionResult PageLoad()
        {
            string AuthError = Request.Query["error"];
            if (AuthError != null) return NotFound();

            string AuthCode = Request.Query["code"];
            if (AuthCode == null) return NotFound();

            string StrUrl = "https://oauth2.googleapis.com/token";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("code=" + AuthCode + "&");
            StrParam.Append("client_id=[client_id]&");
            StrParam.Append("client_secret=[client_secret]&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/google/callback&");
            StrParam.Append("grant_type=authorization_code&");

            string StrReJson = "";
            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("content-type", "application/x-www-form-urlencoded");
                try
                {
                    StrReJson = WClient.UploadString(StrUrl, "POST", StrParam.ToString());
                }
                catch
                {
                    return NotFound();
                }
            }

            JObject JobjAccToken = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string AccToken = "";
            try
            {
                AccToken = JobjAccToken["access_token"].ToString();
            }
            catch
            {
                return NotFound();
            }

            string StrInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";
            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("Authorization", "Bearer " + AccToken);
                try
                {
                    StrReJson = WClient.DownloadString(StrInfoUrl);
                }
                catch
                {
                    return NotFound();
                }
            }
            
            JObject JobjInfo = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string id = JobjInfo["id"].ToString();
            string name = JobjInfo["name"].ToString();
            string picture = JobjInfo["picture"].ToString();
            string email = JobjInfo["email"].ToString();

            string strSql = @"
                declare @MID int
                declare @PassportCode nvarchar(300)

                exec xps_SignIn @ClientID, @LoginMethod, @CName, @AvatarURL, @EMail, @MID output, @PassportCode output
                select @MID as MID, @PassportCode as PassportCode
            ";

            var p = new DynamicParameters();
            p.Add("@ClientID", id);
            p.Add("@CName", name);
            p.Add("@EMail", email);
            p.Add("@AvatarURL", picture);
            p.Add("@LoginMethod", "google");

            using (var db = new AppDb())
            {
                List<Oauth> userInfo = db.Connection.Query<Oauth>(strSql, p).ToList();

                Response.Cookies.Append("mid", userInfo[0].MID.ToString());
                Response.Cookies.Append("passportCode", userInfo[0].PassportCode);

                return Redirect("http://localhost:8888/home");
            }
        }
        // <summary>
        /// Facebook 驗證登入授權
        /// </summary>
        [HttpGet]
        [Route("facebook/test")]
        public RedirectResult GetFacebookLogin2()
        {
            string strUrl = "https://www.facebook.com/dialog/oauth";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("client_id=171322955816361&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/facebook/callback&");
            StrParam.Append("response_type=code&");
            StrParam.Append("scope=public_profile&");

            return RedirectPermanent(strUrl + "?" + StrParam.ToString());
        }
        /// <summary>
        /// 接收 Facebook CallBack 回來的參數及取得使用者資訊
        /// </summary>
        [HttpGet]
        [Route("facebook/test/callback")]
        public IActionResult FBPageLoad2()
        {
            // 登入回傳錯誤
            string AuthError = Request.Query["error"];
            if (AuthError != null) return RedirectPermanent("");

            string AuthCode = Request.Query["code"];
            // 沒有獲取授權碼
            if (AuthCode == null) return RedirectPermanent("");

            string StrUrl = "https://graph.facebook.com/v2.3/oauth/access_token";

            StringBuilder StrParam = new StringBuilder();
            StrParam.Append("code=" + AuthCode + "&");
            StrParam.Append("client_id=[client_id]&");
            StrParam.Append("client_secret=[client_secret]&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/facebook/callback&");
            StrParam.Append("grant_type=authorization_code&");

            string StrReJson = "";

            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("content-type", "application/x-www-form-urlencoded");
                try
                {
                    StrReJson = WClient.UploadString(StrUrl, "POST", StrParam.ToString());
                }
                catch
                {
                    return RedirectPermanent("");
                }
            }

            JObject JobjAccToken = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string AccToken = "";

            try
            {
                AccToken = JobjAccToken["access_token"].ToString();
            }
            catch
            {
                return RedirectPermanent("");
            }

            string StrInfoUrl = "https://graph.facebook.com/v11.0/me?access_token=" + AccToken + "&debug=all&fields=id,name,picture,email";
            using (WebClient WClient = new WebClient())
            {
                try
                {
                    StrReJson = WClient.DownloadString(StrInfoUrl);
                }
                catch
                {
                    return RedirectPermanent("");
                }

            }

            JObject JobjInfo = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string name = "";
            string picture = "";
            string msg = "";

            try
            {
                name = JobjInfo["name"].ToString();
                picture = JobjInfo["picture"]["data"]["url"].ToString();
                msg = JobjInfo["__debug__"]["messages"][1]["message"].ToString();
            }
            catch
            {
                return RedirectPermanent("");
            }

            //return RedirectPermanent("http://localhost:8888/home");

            return Ok(new { name = name, picture = picture, url = StrInfoUrl, msg = msg });
        }
        /// <summary>
        /// Facebook 驗證登入授權
        /// </summary>
        [HttpGet]
        [Route("facebook")]
        public RedirectResult GetFacebookLogin()
        {
            string strUrl = "https://www.facebook.com/dialog/oauth";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("client_id=1418268912328690&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/facebook/callback&");
            StrParam.Append("response_type=code&");
            StrParam.Append("scope=public_profile&");

            return RedirectPermanent(strUrl + "?" + StrParam.ToString());
        }
        /// <summary>
        /// 接收 Facebook CallBack 回來的參數及取得使用者資訊
        /// </summary>
        [HttpGet]
        [Route("facebook/callback")]
        public IActionResult FBPageLoad()
        {
            // 登入回傳錯誤
            string AuthError = Request.Query["error"];
            if (AuthError != null) return RedirectPermanent("");

            string AuthCode = Request.Query["code"];
            // 沒有獲取授權碼
            if (AuthCode == null) return RedirectPermanent("");

            string StrUrl = "https://graph.facebook.com/v2.3/oauth/access_token";

            StringBuilder StrParam = new StringBuilder();
            StrParam.Append("code=" + AuthCode + "&");
            StrParam.Append("client_id=[client_id]&");
            StrParam.Append("client_secret=[client_secret]&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/facebook/callback&");
            StrParam.Append("grant_type=authorization_code&");

            string StrReJson = "";

            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("content-type", "application/x-www-form-urlencoded");
                try
                {
                    StrReJson = WClient.UploadString(StrUrl, "POST", StrParam.ToString());
                }
                catch
                {
                    return RedirectPermanent("");
                }
            }

            JObject JobjAccToken = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string AccToken = "";

            try
            {
                AccToken = JobjAccToken["access_token"].ToString();
            }
            catch
            {
                return RedirectPermanent("");
            }

            string StrInfoUrl = "https://graph.facebook.com/v11.0/me?access_token=" + AccToken + "&debug=all&fields=id,name,picture,email";
            using (WebClient WClient = new WebClient())
            {
                try
                {
                    StrReJson = WClient.DownloadString(StrInfoUrl);
                }
                catch
                {
                    return RedirectPermanent("");
                }

            }

            JObject JobjInfo = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string name = "";
            string picture = "";
            string msg = "";

            try
            {
                name = JobjInfo["name"].ToString();
                picture = JobjInfo["picture"]["data"]["url"].ToString();
                msg = JobjInfo["__debug__"]["messages"][1]["message"].ToString();
            }
            catch
            {
                return RedirectPermanent("");
            }

            //return RedirectPermanent("http://localhost:8888/home");

            return Ok(new { name = name, picture = picture, url = StrInfoUrl, msg = msg });
        }
        /// <summary>
        /// Line 驗證登入授權
        /// </summary>
        [HttpGet]
        [Route("line")]
        public IActionResult GetLineLogin()
        {
            string strUrl = "https://access.line.me/oauth2/v2.1/authorize";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("response_type=code&");
            StrParam.Append("client_id=1657846388&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/line/callback&");
            StrParam.Append("state=111&");
            StrParam.Append("scope=profile%20openid%20email");

            return Redirect(strUrl + "?" + StrParam.ToString());
        }
        /// <summary>
        /// 接收 Line CallBack 回來的參數及取得使用者資訊
        /// </summary>
        [HttpGet]
        [Route("line/callback")]
        public IActionResult LinePageLoad()
        {
            string AuthError = Request.Query["error"];
            if (AuthError != null) return NotFound();

            string AuthCode = Request.Query["code"];
            if (AuthCode == null) return NotFound();

            string StrUrl = "https://api.line.me/oauth2/v2.1/token";
            StringBuilder StrParam = new StringBuilder();

            StrParam.Append("grant_type=authorization_code&");
            StrParam.Append("code=" + AuthCode + "&");
            StrParam.Append("redirect_uri=http://localhost:3000/api/OAuth/line/callback&");
            StrParam.Append("client_id=1657846388&");
            StrParam.Append("client_secret=76003c3274ba8cf0c6659044c2000d06&");

            string StrReJson = "";
            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("content-type", "application/x-www-form-urlencoded");
                try
                {
                    StrReJson = WClient.UploadString(StrUrl, "POST", StrParam.ToString());
                }
                catch
                {
                    return NotFound();
                }
            }

            JObject JobjAccToken = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string AccToken = "", idToken = "";
            try
            {
                AccToken = JobjAccToken["access_token"].ToString();
                idToken = JobjAccToken["id_token"].ToString();
            }
            catch
            {
                return NotFound();
            }

            string StrInfoUrl = "https://api.line.me/v2/profile";
            using (WebClient WClient = new WebClient())
            {
                WClient.Headers.Add("Authorization", "Bearer " + AccToken);
                try
                {
                    StrReJson = WClient.DownloadString(StrInfoUrl);
                }
                catch
                {
                    return NotFound();
                }
            }

            JObject JobjInfo = JsonConvert.DeserializeObject<JObject>(StrReJson);
            string userId = JobjInfo["userId"].ToString();
            string name = JobjInfo["displayName"].ToString();
            string picture = JobjInfo["pictureUrl"].ToString();

            var email = new JwtSecurityToken(idToken).Payload["email"];

            string strSql = @"
                declare @MID int
                declare @PassportCode nvarchar(300)

                exec xps_SignIn @ClientID, @LoginMethod, @CName, @AvatarURL, @EMail, @MID output, @PassportCode output
                select @MID as MID, @PassportCode as PassportCode
            ";

            var p = new DynamicParameters();
            p.Add("@ClientID", userId);
            p.Add("@CName", name);
            p.Add("@EMail", email);
            p.Add("@AvatarURL", picture);
            p.Add("@LoginMethod", "line");

            using (var db = new AppDb())
            {
                List<Oauth> userInfo = db.Connection.Query<Oauth>(strSql, p).ToList();

                Response.Cookies.Append("mid", userInfo[0].MID.ToString());
                Response.Cookies.Append("passportCode", userInfo[0].PassportCode);

                return Redirect("http://localhost:8888/home");
            }
        }
    }
}
