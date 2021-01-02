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
        public PolicyChecker( IUserRepository userRepository)
        {
            _userRepos = userRepository;
        }
        public bool CheckPolicy(int userId, int pollId)
        {
            foreach (var a in _userRepos.GetAllAccessPolicies(userId))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
        public bool CheckAdminPolicy(int userId, int pollId)
        {
            foreach (var a in _userRepos.GetAllAdminPolicies(userId))
            {
                if (a.PollId == pollId)
                    return true;
            }
            return false;
        }
    }
}
