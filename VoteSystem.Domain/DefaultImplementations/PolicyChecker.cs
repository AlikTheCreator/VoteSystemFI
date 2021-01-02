using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.Repositories;
using VoteSystem.Domain.Interfaces;

namespace VoteSystem.Domain.DefaultImplementations
{
    public class PolicyChecker : IPolicyChecker
    {
        IUserRepository _userRepos;
        IContextRegistration _contextRegistration;
        public PolicyChecker(IUserRepository userRepository, IContextRegistration contextRegistration)
        {
            _userRepos = userRepository;
            _contextRegistration = contextRegistration;
        }
        public bool CheckPolicy(int pollId)
        {
            foreach (var a in _userRepos.GetAllAccessPolicies(_contextRegistration.GetLoggedUser().Id))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
        public bool CheckAdminPolicy(int pollId)
        {
            foreach (var a in _userRepos.GetAllAdminPolicies(_contextRegistration.GetLoggedUser().Id))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
    }
}
