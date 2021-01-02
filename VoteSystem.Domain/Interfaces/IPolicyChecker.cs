using System;
using System.Collections.Generic;
using System.Text;

namespace VoteSystem.Domain.Interfaces
{
    public interface IPolicyChecker
    {
        bool CheckPolicy(int userId, int pollId);
        bool CheckAdminPolicy(int userId, int pollId);
    }
}
