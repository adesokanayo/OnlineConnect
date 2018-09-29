using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OnlineAD.Api.Domain;
using Serilog;

namespace OnlineAD.Api.Controllers
{
    [Route("api/onlinead")]
    public class ActiveDirectoryController : Controller
    {
        private readonly IConfiguration _config;

        public IActiveDirectoryService _Service;

        public ActiveDirectoryController(IConfiguration config, IActiveDirectoryService service)
        {
            _config = config;
            _Service = service;
        }


        // POST api/values
        [HttpPost("validate")]
        public async Task<IActionResult> PostAD([FromBody]ADModel model)
        {
            Log.Information("Starting call to the API");
            var response = await ValidateNTUser(model);

            

            return Ok(response);
        }

        private async Task<ADResponse> ValidateNTUser(ADModel model)
        {
            return await Task.Run(() =>
            {
               
                    if (model == null)
                    {
                        Log.Error("NT User paramter cannot be empty");

                        return new ADResponse()
                        {
                            ErrorMessage = "NT User paramter cannot be empty",
                            Status = StatusType.Failed,
                             UserExist = false
                        };
                    }

                    if (string.IsNullOrEmpty(model.username) || string.IsNullOrEmpty(model.password) || string.IsNullOrEmpty(model.key))

                    {
                        Log.Error("NT User either username or password or key is empty");

                        return new ADResponse()
                        {
                            ErrorMessage = "NT User either username or password or key is empty",
                            Status = StatusType.Failed,
                            UserExist = false
                        };
                    }


                    string passkey = _config["appkey"];

                    if (passkey != model.key)
                    {
                        Log.Error("Application key is not valid");
                        return new ADResponse()
                        {
                            ErrorMessage = "Application key is not valid",
                            Status = StatusType.Failed,
                            UserExist = false
                        };
                    }

                    model.username = model.username.ToLower();

                    bool login_response = _Service.authlogindetails(model.username, model.password);

                    if (!login_response)
                    {
                        Log.Error("Username or password invalid");
                        return new ADResponse()
                        {
                            ErrorMessage = "Username or password invalid",
                            Status = StatusType.Failed,
                            UserExist = false
                        };
                    }

                    var searchResult = _Service.GetUserDirectoryEntryDetails(model.username);

                    if (searchResult == null)
                    {
                        Log.Error("User search information cannot be found");
                        return new ADResponse()
                        {
                            ErrorMessage = "User search information cannot be found",
                            Status = StatusType.Failed,
                            UserExist = false
                        };
                    }

                    _Service.PopulateUserDataStruct(searchResult);

                    var userData = _Service.GetUserData(model.username);

                    var groups = _Service.Getmemberof(searchResult);

                    Log.Information("Active directory calls successful");

                    return new ADResponse()
                    {
                        UserExist = true,
                        UserGroup = string.Join(",", groups.ToArray()),
                         Status = StatusType.Success
                    };
                });

        }

    }
}
