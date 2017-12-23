using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xmu.Crms.Shared.Exceptions;
using Xmu.Crms.Shared.Models;
using Xmu.Crms.Shared.Service;


namespace Xmu.Crms.Services.HotPot
{
    public class LoginService : ILoginService
    {
        private readonly CrmsContext _db;

        public LoginService(CrmsContext db)
        {
            _db = db;
        }

        public bool DeleteStudentAccount(long userId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteTeacherAccount(long userId)
        {
            throw new NotImplementedException();
        }

        public UserInfo SignInPhone(UserInfo user)
        {
            throw new NotImplementedException();
        }

        public UserInfo SignInWeChat(long userId, string code, string state, string successUrl)
        {
            throw new NotImplementedException();
        }

        public UserInfo SignUpPhone(UserInfo user)
        {
            throw new NotImplementedException();
        }
    }
}
