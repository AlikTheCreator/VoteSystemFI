﻿using System;
using System.Collections.Generic;
using System.Text;
using VoteSystem.Data.Entities.UserPolicyAggregate;

namespace VoteSystem.Data.Repositories
{
    public interface IUserRepository
    {
        void CreateUser(User user);
        void UpdateUser(User user);
        bool IsInRegion(int regionId, int userId);
        List<User> GetAllUsersForRegion(int regionId);
        public List<UserPolicy> GetAllUserPolicies();
        public bool UserExists(string paspCode, int IndefCode);
        public User GetUser(string PaspCode, int IndefCode);
    }
}
