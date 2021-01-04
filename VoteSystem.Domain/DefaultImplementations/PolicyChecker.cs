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
        IAuthorizationContext _authorizationContext;
        public PolicyChecker(IUserRepository userRepository, IAuthorizationContext authorizationContext)
        {
            _userRepos = userRepository;
            _authorizationContext = authorizationContext;
        }
        public bool CheckPolicy(int pollId)
        {
            foreach (var a in _userRepos.GetAllAccessPolicies(_authorizationContext.GetLoggedUser().Id))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
        public bool CheckAdminPolicy(int pollId)
        {
            foreach (var a in _userRepos.GetAllAdminPolicies(_authorizationContext.GetLoggedUser().Id))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
    }
}
