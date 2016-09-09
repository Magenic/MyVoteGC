using System;
using Csla.Data;
using MyVote.Services.AppServer.Auth;
using MyVote.Services.AppServer.Models;
using MyVote.BusinessObjects.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using static MyVote.Services.AppServer.Extensions.IBusinessBaseExtensions;
using System.Web.Http;
using System.Net.Http;
using System.Net;

namespace MyVote.Services.AppServer.Controllers
{
	[Route("api/[controller]")]
	public sealed class UserController
		: Controller
	{
		public const string GetByIdUri = "GetById";
		private readonly IObjectFactory<IUser> userFactory;
		private readonly IMyVoteAuthentication authentication;

		public UserController(IObjectFactory<IUser> userFactory, IMyVoteAuthentication authentication)
		{
			if (userFactory == null)
			{
				throw new ArgumentNullException(nameof(userFactory));
			}

			if (authentication == null)
			{
				throw new ArgumentNullException(nameof(authentication));
			}

			this.userFactory = userFactory;
			this.authentication = authentication;
		}

		// GET api/user/5
		//[Authorize]
		[HttpGet("{userProfileId}", Name = UserController.GetByIdUri)]
		public IActionResult Get(string userProfileId)
		{
            try
            {
                var user = this.userFactory.Fetch(userProfileId);
                var authUserID = this.authentication.GetCurrentUserID().Value;
                return new OkObjectResult(new User
                {
                    UserID = user.UserID,
                    ProfileID = user.ProfileID,
                    EmailAddress = user.EmailAddress,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Gender = user.Gender,
                    BirthDate = user.BirthDate,
                    PostalCode = user.PostalCode,
                    UserName = user.UserName
                });
            }
            catch (Exception ex)
            {
                return new OkObjectResult(new User
                {
                    UserID = -99,
                    ProfileID = string.Empty,
                    EmailAddress = string.Empty,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    Gender = "M",
                    BirthDate = DateTime.Now,
                    PostalCode = string.Empty,
                    UserName = string.Empty,
                });
            }
		}

		// PUT api/user
		//[Authorize]
		[HttpPut]
		public async Task<IActionResult> Put([FromBody]User value)
		{
			IUser user = this.userFactory.Create(value.ProfileID);

			user.EmailAddress = value.EmailAddress;
			user.FirstName = value.FirstName;
			user.LastName = value.LastName;
			user.Gender = value.Gender;
			user.BirthDate = value.BirthDate;
			user.PostalCode = value.PostalCode;
			user.UserName = value.UserName;

            try
            {
                return await user.PersistAsync(
                () =>
                {
                    var id = user.UserID;
                    return Ok(0);
                });
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("An error occurred, please try again or contact MyVote administrator."),
                    ReasonPhrase = "Critical Exception"
                });
            }
        }

		// PUT api/user/5
		//[Authorize]
		[HttpPut("{id}")]
		public async Task<IActionResult> Put(string userProfileId, [FromBody]User value)
		{
			var user = this.userFactory.Fetch(userProfileId);
			var authUserID = this.authentication.GetCurrentUserID();

			if (user.UserID != authUserID)
			{
				return new UnauthorizedResult();
			}

			DataMapper.Map(value, user, nameof(user.ProfileID), nameof(user.UserID));
			return await user.PersistAsync();
		}
	}
}