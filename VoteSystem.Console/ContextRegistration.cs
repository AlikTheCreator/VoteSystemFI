using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.Entities.UserPolicyAggregate;
using VoteSystem.Data.Repositories;
using VoteSystem.Domain.Interfaces;

namespace VoteSystem.Cosnole
{
    class ContextRegistration : IContextRegistration
    {
        private string _passportCode;
        private int _indefCode;
        private IUserRepository _userRepository;
        private User _loggedInUser;

        public ContextRegistration(IUserRepository userRepository) {
            _userRepository = userRepository;
        }

        public Tuple<string, int> GetPassportInfo()
        {
            return new Tuple<string, int>(_passportCode, _indefCode);
        }

        public bool SetPasswordInfo(string passportCode, int indefCode)
        {
            _passportCode = passportCode;
            _indefCode = indefCode;
            _loggedInUser = _userRepository.GetUser(passportCode, indefCode);
            return true;
        }

        public User GetLoggedUser()
        {
            return _loggedInUser;
        }
    }
}
