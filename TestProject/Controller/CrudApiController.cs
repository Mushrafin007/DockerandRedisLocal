using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net;
using TestProject.Services.CrudApiService;
using static TestProject.Model.CrudApiModel;

namespace TestProject.Controller
{
    [ApiController]
    public class CrudApiController : ControllerBase
    {
        ICrudApiService _ICrudApiService;
        IConnectionMultiplexer _redis;

        public CrudApiController(ICrudApiService CrudApiService, IConnectionMultiplexer redis) 
        {
            _ICrudApiService = CrudApiService;
            _redis = redis;
        }
        [Route("api/Crud/Create")]
        [HttpPost]
        public  async Task<JsonResult> create(CreateMode obj)
        {
            HttpContext.Response.ContentType = "application/json";
            HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ApiCommonResponseModel apiCommonResponse = new ApiCommonResponseModel();
            try 
            {
                object parameter = new
                {
                    Name = obj.Name,
                    Phone = obj.Phone,
                    Email = obj.Email,
                    Gender = obj.Gender
                }; 
                apiCommonResponse =  await _ICrudApiService.USP_CREATE_INS(parameter);
                if (apiCommonResponse.status == 200) 
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    apiCommonResponse.message = "Success";
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    apiCommonResponse.message = "failed";

                }

            }
            catch(Exception ex) 
            {
               
            }
            return new JsonResult(apiCommonResponse);
        }
        [Route("api/Crud/Createwithredis")]
        [HttpPost]
        public async Task<IActionResult> Createwithredis(CreateMode obj)
        {
            var db = _redis.GetDatabase();
            string jsonData = System.Text.Json.JsonSerializer.Serialize(obj);

            await db.ListLeftPushAsync("create-queue", jsonData); // Push to Redis queue
            return Ok(new { status = 200, message = "Request received" });
        }
        [Route("api/Crud/getDataById")]
        [HttpGet]
        public async Task<IActionResult> getDataById([FromQuery] int id)
        {
            getUserDataModel _getUserDataModel = new getUserDataModel();
            try
            {
                var db = _redis.GetDatabase(); // Redis connection (injected IConnectionMultiplexer)
                string cacheKey = $"user:{id}";
                string cachedData = await db.StringGetAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    // Found in Redis
                    _getUserDataModel = JsonConvert.DeserializeObject<getUserDataModel>(cachedData);
                    _getUserDataModel.message = "User Found (from cache)";
                }
                else
                {
                    object parameter = new
                    {
                        Id = id
                    };
                    _getUserDataModel = await _ICrudApiService.USP_USERDATA_GET(parameter);
                    if (_getUserDataModel != null && _getUserDataModel.Id != 0)
                    {
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                        _getUserDataModel.message = "User Found";

                        string jsonData = JsonConvert.SerializeObject(_getUserDataModel);
                        await db.StringSetAsync(cacheKey, jsonData, TimeSpan.FromMinutes(5));
                    }
                    else
                    {
                        ApiCommonResponseModel ApiCommonResponseModel = new ApiCommonResponseModel();
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                        ApiCommonResponseModel.status = 404;
                        ApiCommonResponseModel.message = "User Not Found";
                        return new JsonResult(ApiCommonResponseModel);
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return new JsonResult(_getUserDataModel);
        }
        [Route("api/Crud/deleteuserredis")]
        [HttpDelete]
        public async Task<IActionResult> Dedeleteuserredislete([FromQuery] int id)
        {
            try
            {
                ApiCommonResponseModel apiCommonResponseModel = new ApiCommonResponseModel();
                object parameter = new { Id = id };
                apiCommonResponseModel = await _ICrudApiService.USP_DELETE_BY_ID(parameter);

                if (apiCommonResponseModel.status == 200)
                {
                    var db = _redis.GetDatabase();

                    // Remove from Redis cache (assuming you cache user data by "user:{id}" key)
                    string redisKey = $"user:{id}";
                    await db.KeyDeleteAsync(redisKey);

                    return Ok(apiCommonResponseModel);
                }
                else
                {
                    return Ok(apiCommonResponseModel);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = 500, message = "Internal server error", error = ex.Message });
            }
        }

    }
}
